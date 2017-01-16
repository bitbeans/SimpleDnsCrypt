using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace Uninstall
{
	internal class Program
	{
		internal static readonly string[] KnownServices =
		{
			"dnscrypt-proxy",
			"dnscrypt-proxy-secondary"
		};

		private static void Main(string[] args)
		{
			try
			{
				ClearLocalNetworkInterfaces();
				UninstallKnownServices();
			}
			finally
			{
				Environment.Exit(0);
			}
		}

		/// <summary>
		///     Clear all network interfaces.
		/// </summary>
		internal static void ClearLocalNetworkInterfaces()
		{
			string[] networkInterfaceBlacklist =
			{
				"Microsoft Virtual",
				"Hamachi Network",
				"VMware Virtual",
				"VirtualBox",
				"Software Loopback",
				"Microsoft ISATAP",
				"Microsoft-ISATAP",
				"Teredo Tunneling Pseudo-Interface",
				"Microsoft Wi-Fi Direct Virtual",
				"Microsoft Teredo Tunneling Adapter",
				"Von Microsoft gehosteter",
				"Microsoft hosted",
				"Virtueller Microsoft-Adapter",
				"TAP"
			};

			try
			{
				var networkInterfaces = new List<NetworkInterface>();
				foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (nic.OperationalStatus != OperationalStatus.Up)
					{
						continue;
					}
					foreach (var blacklistEntry in networkInterfaceBlacklist)
					{
						if (nic.Description.Contains(blacklistEntry) || nic.Name.Contains(blacklistEntry)) continue;
						if (!networkInterfaces.Contains(nic))
						{
							networkInterfaces.Add(nic);
						}
					}
				}

				foreach (var networkInterface in networkInterfaces)
				{
					//TODO: Add IPv6 support
					var registryKey =
						Registry.GetValue(
							"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + networkInterface.Id,
							"NameServer", "");
					if (registryKey == null || registryKey.ToString().Length <= 0) continue;
					var serverAddresses = new List<string>(((string) registryKey).Split(new[] {","}, StringSplitOptions.None));
					if (serverAddresses.Contains("127.0.0.1") || serverAddresses.Contains("127.0.0.2"))
					{
						SetNameservers(networkInterface, new List<string>(), NetworkInterfaceComponent.IPv4);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		internal static void SetNameservers(NetworkInterface localNetworkInterface, List<string> dnsServers,
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
									.Where(mo => (string) mo["Description"] == localNetworkInterface.Description))
						{
							var enabled = (bool) managementObject["IPEnabled"];
							if (enabled)
							{
								// the network adapter is enabled, so we can change the dns settings per WMI.
								using (var newDns = managementObject.GetMethodParameters("SetDNSServerSearchOrder"))
								{
									newDns["DNSServerSearchOrder"] = dnsServers.ToArray();
									var outputBaseObject = managementObject.InvokeMethod("SetDNSServerSearchOrder", newDns, null);
									if (outputBaseObject == null) continue;
									var state = (uint) outputBaseObject["returnValue"];
									switch (state)
									{
										case 0: // Successful completion, no reboot required
										case 1: // Successful completion, reboot required
											break;
										case 84: // IP not enabled on adapter
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
				}
			}
		}

		/// <summary>
		///     Uninstall all known dnscrypt-proxy services.
		/// </summary>
		internal static void UninstallKnownServices()
		{
			try
			{
				foreach (var service in KnownServices)
				{
					var localMachine = Registry.LocalMachine;
					var main = localMachine.OpenSubKey(
						@"SYSTEM\\CurrentControlSet\\Services\\" + service, false);

					if (main == null)
					{
						return;
					}

					var dnsCryptoProxyExecutablePath = (string) main.GetValue("ImagePath");
					if (!string.IsNullOrEmpty(dnsCryptoProxyExecutablePath))
					{
						Console.WriteLine("Removing {0}", service);
						const int timeout = 9000;
						using (var process = new Process())
						{
							process.StartInfo.FileName = dnsCryptoProxyExecutablePath;
							process.StartInfo.Arguments = "--uninstall";
							process.StartInfo.Arguments += " --service-name=" + service;
							process.StartInfo.UseShellExecute = false;
							process.StartInfo.CreateNoWindow = true;
							process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							process.Start();
							if (process.WaitForExit(timeout))
							{
								if (process.ExitCode == 0)
								{
									//do nothing
								}
							}
							else
							{
								// Timed out.
								throw new Exception("Timed out");
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}