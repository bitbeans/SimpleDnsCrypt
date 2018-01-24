using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Uninstall
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				ClearLocalNetworkInterfaces();
				UninstallService();
			}
			finally
			{
				Environment.Exit(0);
			}
		}

		/// <summary>
		///		Uninstall the dnscrypt-proxy service.
		/// </summary>
		internal static void UninstallService()
		{
			try
			{
				const int timeout = 9000;
				const string dnsCryptProxyFolder = "dnscrypt-proxy";
				const string dnsCryptProxyExecutableName = "dnscrypt-proxy.exe";
				const string arguments = "-service uninstall";
				Console.WriteLine("removing dnscrypt service");
				using (var process = new Process())
				{
					process.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), dnsCryptProxyFolder, dnsCryptProxyExecutableName);
					process.StartInfo.Arguments = arguments;
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
			catch (Exception) { }
		}

		/// <summary>
		///		 Clear all network interfaces.
		/// </summary>
		internal static void ClearLocalNetworkInterfaces()
		{
			try
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
					using (var process = new Process())
					{
						Console.WriteLine("clearing {0}", networkInterface.Name);
						var processStartInfoV4 = new ProcessStartInfo("netsh",
							"interface ipv4 delete dns \"" + networkInterface.Name + "\" all")
						{
							WindowStyle = ProcessWindowStyle.Hidden,
							CreateNoWindow = true
						};
						process.StartInfo = processStartInfoV4;
						process.Start();


						var processStartInfoV6 = new ProcessStartInfo("netsh",
							"interface ipv6 delete dns \"" + networkInterface.Name + "\" all")
						{
							WindowStyle = ProcessWindowStyle.Hidden,
							CreateNoWindow = true
						};
						process.StartInfo = processStartInfoV6;
						process.Start();
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
