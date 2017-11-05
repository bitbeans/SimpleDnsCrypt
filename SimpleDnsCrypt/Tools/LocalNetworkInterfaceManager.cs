using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Tools
{
	/// <summary>
	///     Class to manage the local network interfaces.
	/// </summary>
	internal static class LocalNetworkInterfaceManager
	{
		/// <summary>
		///     Get a list of the local network interfaces.
		/// </summary>
		/// <param name="userData"></param>
		/// <param name="showHiddenCards">Show hidden cards.</param>
		/// <param name="showOnlyOperationalUp">Include only connected network cards.</param>
		/// <param name="optionalAddress"></param>
		/// <returns>A (filtered) list of the local network interfaces.</returns>
		/// <exception cref="NetworkInformationException">A Windows system function call failed. </exception>
		/// <exception cref="ArgumentNullException"></exception>
		internal static List<LocalNetworkInterface> GetLocalNetworkInterfaces(UserData userData, bool showHiddenCards = false,
			bool showOnlyOperationalUp = true, string optionalAddress = "")
		{
			var interfaces = new List<LocalNetworkInterface>();
			foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (showOnlyOperationalUp)
				{
					if (nic.OperationalStatus != OperationalStatus.Up)
					{
						continue;
					}
				}

				if (!showHiddenCards)
				{
					var add = true;
					foreach (var blacklistEntry in Global.NetworkInterfaceBlacklist)
					{
						if (nic.Description.Contains(blacklistEntry) || nic.Name.Contains(blacklistEntry))
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
					Ipv6Support = nic.Supports(NetworkInterfaceComponent.IPv6),
					OperationalStatus = nic.OperationalStatus
				};

				localNetworkInterface.UseDnsCrypt = IsUsingDnsCrypt(localNetworkInterface, optionalAddress);
				if (!localNetworkInterface.UseDnsCrypt)
				{
					localNetworkInterface.UseInsecureFallbackDns = IsUsingInsecureFallbackDns(localNetworkInterface, userData);
				}

				interfaces.Add(localNetworkInterface);
			}
			return interfaces;
		}

		internal static bool IsUsingInsecureFallbackDns(LocalNetworkInterface localNetworkInterface, UserData userData)
		{
			if (!(userData?.InsecureResolverPair?.Addresses?.Count > 0)) return false;
			var fallbackAddresses = userData.InsecureResolverPair.Addresses;
			return fallbackAddresses.Any(fallbackAddress => localNetworkInterface.Ipv4Dns.Contains(fallbackAddress));
		}

		/// <summary>
		///     Simple check if the network interface contains any of resolver addresses.
		/// </summary>
		/// <param name="localNetworkInterface">The interface to check.</param>
		/// <param name="optionalAddress"></param>
		/// <returns><c>true</c> if a address was found, otherwise <c>false</c></returns>
		internal static bool IsUsingDnsCrypt(LocalNetworkInterface localNetworkInterface, string optionalAddress = "")
		{
			var dns = new List<string> {Global.PrimaryResolverAddress, Global.SecondaryResolverAddress};
			var dns6 = new List<string> {Global.PrimaryResolverAddress6, Global.SecondaryResolverAddress6};
			if (localNetworkInterface.Ipv4Dns.Contains(dns[0]) || localNetworkInterface.Ipv4Dns.Contains(dns[1]))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(optionalAddress))
			{
				if (localNetworkInterface.Ipv4Dns.Contains(optionalAddress))
				{
					return true;
				}
			}
			if (localNetworkInterface.Ipv6Dns.Contains(dns6[0]) || localNetworkInterface.Ipv6Dns.Contains(dns6[1]))
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
			catch (Exception)
			{
			}
			return serverAddresses;
		}

		/// <summary>
		///     Set's the IPv4 or IPv6 DNS Servers of an interface.
		/// </summary>
		/// <param name="localNetworkInterface">The interface to work with.</param>
		/// <param name="dnsServers">List of dns servers to set.</param>
		/// <param name="networkInterfaceComponent">IPv4 or IPv6.</param>
		/// <returns><c>true</c> on success, otherwise <c>false</c></returns>
		/// <remarks>Win32_NetworkAdapter class is deprecated.
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/hh968170(v=vs.85).aspx (only on windows 8+)</remarks>
		public static bool SetNameservers(LocalNetworkInterface localNetworkInterface, List<string> dnsServers,
			NetworkInterfaceComponent networkInterfaceComponent = NetworkInterfaceComponent.IPv4)
		{
			var status = false;
			if (networkInterfaceComponent == NetworkInterfaceComponent.IPv4)
			{
				using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapterConfiguration"))
				{
					using (var networkConfigs = networkConfigMng.GetInstances())
					{
						//(bool)mo["IPEnabled"] &&
						foreach (
							var managementObject in
								networkConfigs.Cast<ManagementObject>()
									.Where(mo => (string)mo["Description"] == localNetworkInterface.Description))
						{
							var enabled = (bool)managementObject["IPEnabled"];
							if (enabled)
							{
								// the network adapter is enabled, so we can change the dns settings per WMI.
								using (var newDns = managementObject.GetMethodParameters("SetDNSServerSearchOrder"))
								{
									newDns["DNSServerSearchOrder"] = dnsServers.ToArray();
									var outputBaseObject = managementObject.InvokeMethod("SetDNSServerSearchOrder", newDns, null);
									if (outputBaseObject == null) continue;
									var state = (uint)outputBaseObject["returnValue"];
									switch (state)
									{
										case 0: // Successful completion, no reboot required
										case 1: // Successful completion, reboot required
											status = true;
											break;
										case 84: // IP not enabled on adapter
											status = false;
											break;
										default:
											status = false;
											break;
									}
								}
							}
							else
							{
								// seems to be unplugged or disabled, so we need change this per registry (not optimal)
								var registryId = managementObject["SettingID"];
								if (registryId == null) continue;
								var localMachine = Registry.LocalMachine;
								var interfaceEntry = localMachine.OpenSubKey(
									@"SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + registryId, true);
								if (interfaceEntry == null) continue;
								interfaceEntry.SetValue("NameServer", dnsServers.Count > 0 ? string.Join(",", dnsServers.ToArray()) : "",
									RegistryValueKind.String);
								status = true;
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
						WindowStyle = ProcessWindowStyle.Hidden,
						CreateNoWindow = true
					};
					process.StartInfo = processStartInfo;
					process.Start();

					foreach (var address in dnsServers)
					{
						//netsh interface ipv6 add dns "Interface Name" 127.0.0.1 validate=no
						processStartInfo = new ProcessStartInfo("netsh",
							"interface ipv6 add dns \"" + localNetworkInterface.Name + "\" " + address + " validate=no")
						{
							WindowStyle = ProcessWindowStyle.Hidden,
							CreateNoWindow = true
						};
						process.StartInfo = processStartInfo;
						process.Start();
					}
					status = true;
				}
			}
			return status;
		}
	}
}