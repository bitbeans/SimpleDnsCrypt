using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace Uninstall
{
	internal class Program
	{
		private const string DnsCryptProxyFolder = "dnscrypt-proxy";
		private const string DnsCryptProxyExecutableName = "dnscrypt-proxy.exe";
		private const string DnsCryptProxyConfigName = "dnscrypt-proxy.toml";

		private static void Main(string[] args)
		{
			try
			{
				BackupConfigurationFile();
				ClearLocalNetworkInterfaces();
				StopService();
				Thread.Sleep(500);
				UninstallService();
			}
			finally
			{
				Environment.Exit(0);
			}
		}

		/// <summary>
		///		Copy dnscrypt-proxy.toml to tmp folder.
		/// </summary>
		internal static void BackupConfigurationFile()
		{
			try
			{
				var sdcConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleDnsCrypt.exe.config");
				if (!File.Exists(sdcConfig)) return;
				var sdcConfigMap = new ExeConfigurationFileMap
				{
					ExeConfigFilename = sdcConfig
				};
				var sdcConfigContent =
					ConfigurationManager.OpenMappedExeConfiguration(sdcConfigMap, ConfigurationUserLevel.None);
				if (!sdcConfigContent.HasFile) return;
				var section = (ClientSettingsSection)sdcConfigContent.GetSection("userSettings/SimpleDnsCrypt.Properties.Settings");
				var setting = section.Settings.Get("BackupAndRestoreConfigOnUpdate");
				var backupAndRestoreConfigOnUpdate = Convert.ToBoolean(setting.Value.ValueXml.LastChild.InnerText);
				if (!backupAndRestoreConfigOnUpdate) return;
				var config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DnsCryptProxyFolder,
					DnsCryptProxyConfigName);
				if (!File.Exists(config)) return;
				var tmp = Path.Combine(Path.GetTempPath(), DnsCryptProxyConfigName + ".bak");
				Console.WriteLine($"backup configuration to {tmp}");
				File.Copy(config, tmp);
			}
			catch (Exception) { }
		}

		/// <summary>
		///		Stop the dnscrypt-proxy service.
		/// </summary>
		internal static void StopService()
		{
			Console.WriteLine("stopping dnscrypt service");
			ExecuteWithArguments("-service stop");
		}

		/// <summary>
		///		Uninstall the dnscrypt-proxy service.
		/// </summary>
		internal static void UninstallService()
		{
			Console.WriteLine("removing dnscrypt service");
			ExecuteWithArguments("-service uninstall");
			Registry.LocalMachine.DeleteSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application\dnscrypt-proxy", false);
		}

		/// <summary>
		///		Uninstall the dnscrypt-proxy service.
		/// </summary>
		internal static void ExecuteWithArguments(string arguments)
		{
			try
			{
				const int timeout = 9000;
				using (var process = new Process())
				{
					process.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
						DnsCryptProxyFolder, DnsCryptProxyExecutableName);
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
