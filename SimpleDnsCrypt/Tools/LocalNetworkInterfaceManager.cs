using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security;
using Microsoft.Win32;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Tools
{
    /// <summary>
    /// Class to manage the local network interfaces.
    /// </summary>
    internal static class LocalNetworkInterfaceManager
    {
        /// <summary>
        /// Get a list of the local network interfaces.
        /// </summary>
        /// <param name="showHiddenCards">Show hidden cards.</param>
        /// <returns>A (filtered) list of the local network interfaces.</returns>
        /// <exception cref="NetworkInformationException">A Windows system function call failed. </exception>
        internal static List<LocalNetworkInterface> GetLocalNetworkInterfaces(bool showHiddenCards = false)
        {
            var interfaces = new List<LocalNetworkInterface>();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }
                if (!showHiddenCards)
                {
                    var add = true;
                    foreach (var blacklistEntry in Global.NetworkInterfaceBlacklist)
                    {
                        if ((nic.Description.Contains(blacklistEntry)) || (nic.Name.Contains(blacklistEntry)))
                        {
                            add = false;
                        }
                    }
                    if (!add) continue;
                }
                var localNetworkInterface = new LocalNetworkInterface
                {
                    Name = nic.Name,
                    Description = nic.Description,
                    Type = nic.NetworkInterfaceType,
                    Ipv4Dns = GetDnsServerList(nic.Id, NetworkInterfaceComponent.IPv4),
                    Ipv6Dns = GetDnsServerList(nic.Id, NetworkInterfaceComponent.IPv6),
                    Ipv4Support = nic.Supports(NetworkInterfaceComponent.IPv4),
                    Ipv6Support = nic.Supports(NetworkInterfaceComponent.IPv6)
                };

                localNetworkInterface.UseDnsCrypt = IsUsingDnsCrypt(localNetworkInterface);

                interfaces.Add(localNetworkInterface);
            }
            return interfaces;
        }

        /// <summary>
        ///     Simple check if the network interface contains any of resolver addresses.
        /// </summary>
        /// <param name="localNetworkInterface">The interface to check.</param>
        /// <returns><c>true</c> if a address was found, otherwise <c>false</c></returns>
        internal static bool IsUsingDnsCrypt(LocalNetworkInterface localNetworkInterface)
        {
            var dns = new List<string> {Global.PrimaryResolverAddress, Global.SecondaryResolverAddress};
            if ((localNetworkInterface.Ipv4Dns.Contains(dns[0])) || (localNetworkInterface.Ipv4Dns.Contains(dns[1])))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Get the nameservers of an interface.
        /// </summary>
        /// <param name="localNetworkInterface">The interface to extract from.</param>
        /// <param name="networkInterfaceComponent">IPv4 or IPv6.</param>
        /// <returns>A list of nameservers.</returns>
        internal static List<string> GetDnsServerList(string localNetworkInterface,
            NetworkInterfaceComponent networkInterfaceComponent)
        {
            var serverAddresses = new List<string>();
            try
            {
                var registryKey = networkInterfaceComponent == NetworkInterfaceComponent.IPv6
                    ? Registry.GetValue(
                        "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\TCPIP6\\Parameters\\Interfaces\\" +
                        localNetworkInterface,
                        "NameServer", "")
                    : Registry.GetValue(
                        "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" +
                        localNetworkInterface,
                        "NameServer", "");

                if (registryKey != null && registryKey.ToString().Length > 0)
                {
                    serverAddresses =
                        new List<string>(((string) registryKey).Split(new[] {","}, StringSplitOptions.None));
                }
            }
            catch(Exception) { }
            return serverAddresses;
        }

        /// <summary>
        ///     Set's the IPv4 DNS Servers of an interface.
        /// </summary>
        /// <param name="localNetworkInterface">The interface to work with.</param>
        /// <param name="dnsServers">List of dns servers to set.</param>
        /// <param name="networkInterfaceComponent">IPv4 or IPv6.</param>
        public static void SetNameservers(LocalNetworkInterface localNetworkInterface, List<string> dnsServers,
            NetworkInterfaceComponent networkInterfaceComponent = NetworkInterfaceComponent.IPv4)
        {
            if (networkInterfaceComponent == NetworkInterfaceComponent.IPv4)
            {
                using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (var networkConfigs = networkConfigMng.GetInstances())
                    {
                        foreach (
                            var managementObject in
                                networkConfigs.Cast<ManagementObject>()
                                    .Where(
                                        mo =>
                                            (bool) mo["IPEnabled"] &&
                                            (string) mo["Description"] == localNetworkInterface.Description))
                        {
                            using (var newDns = managementObject.GetMethodParameters("SetDNSServerSearchOrder"))
                            {
                                newDns["DNSServerSearchOrder"] = dnsServers.ToArray();
                                managementObject.InvokeMethod("SetDNSServerSearchOrder", newDns, null);
                            }
                        }
                    }
                }
            }
            else
            {
                //TODO: find better way to set IPv6 nameservers
                using (var process = new Process())
                {
                    var processStartInfo = new ProcessStartInfo("netsh",
                        "interface ipv6 delete dns \"" + localNetworkInterface.Name + "\" all")
                    {
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    process.StartInfo = processStartInfo;
                    process.Start();

                    foreach (var address in dnsServers)
                    {
                        //netsh interface ipv6 add dns "Interface Name" 127.0.0.1 validate=no
                        processStartInfo = new ProcessStartInfo("netsh",
                            "interface ipv6 add dns \"" + localNetworkInterface.Name + "\" " + address + " validate=no")
                        {
                            WindowStyle = ProcessWindowStyle.Minimized
                        };
                        process.StartInfo = processStartInfo;
                        process.Start();
                    }
                }
            }
        }
    }
}