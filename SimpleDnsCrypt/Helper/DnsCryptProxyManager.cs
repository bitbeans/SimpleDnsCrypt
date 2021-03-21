using Caliburn.Micro;
using Microsoft.Win32;
using Newtonsoft.Json;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;

namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	///     Class to manage the dnscrypt-proxy service and maintain the registry.
	/// </summary>
	public static class DnsCryptProxyManager
	{
		private static string DnsCryptProxyExecutablePath => Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
																		  Environment.Is64BitOperatingSystem
																			  ? Global.DnsCryptProxyExecutableName64
																			  : Global.DnsCryptProxyExecutableName86);

		private static readonly ILog Log = LogManagerHelper.Factory();
		private const string DnsCryptProxyServiceName = "dnscrypt-proxy";

		/// <summary>
		///     Check if the DNSCrypt proxy service is installed.
		/// </summary>
		/// <returns><c>true</c> if the service is installed, otherwise <c>false</c></returns>
		/// <exception cref="Win32Exception">An error occurred when accessing a system API. </exception>
		public static bool IsDnsCryptProxyInstalled()
		{
			try
			{
				var dnscryptService = new ServiceController { ServiceName = DnsCryptProxyServiceName };
				var proxyStatus = dnscryptService.Status;
				return true;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
		}

		/// <summary>
		///     Check if the DNSCrypt proxy service is running.
		/// </summary>
		/// <returns><c>true</c> if the service is running, otherwise <c>false</c></returns>
		public static bool IsDnsCryptProxyRunning()
		{
			try
			{
				var dnscryptService = new ServiceController { ServiceName = DnsCryptProxyServiceName };

				var proxyStatus = dnscryptService.Status;
				switch (proxyStatus)
				{
					case ServiceControllerStatus.Running:
						return true;
					case ServiceControllerStatus.Stopped:
					case ServiceControllerStatus.ContinuePending:
					case ServiceControllerStatus.Paused:
					case ServiceControllerStatus.PausePending:
					case ServiceControllerStatus.StartPending:
					case ServiceControllerStatus.StopPending:
						return false;
					default:
						return false;
				}
			}
			catch (InvalidOperationException)
			{
				return false;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
		}

		/// <summary>
		///     Restart the dnscrypt-proxy service.
		/// </summary>
		/// <returns><c>true</c> on success, otherwise <c>false</c></returns>
		public static bool Restart()
		{
			try
			{
				var dnscryptService = new ServiceController { ServiceName = DnsCryptProxyServiceName };
				dnscryptService.Stop();
				Thread.Sleep(1000);
				dnscryptService.Start();
				return dnscryptService.Status == ServiceControllerStatus.Running;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
		}

		/// <summary>
		///     Stop the dnscrypt-proxy service.
		/// </summary>
		/// <returns><c>true</c> on success, otherwise <c>false</c></returns>
		public static bool Stop()
		{
			try
			{
				var dnscryptService = new ServiceController { ServiceName = DnsCryptProxyServiceName };
				var proxyStatus = dnscryptService.Status;
				switch (proxyStatus)
				{
					case ServiceControllerStatus.ContinuePending:
					case ServiceControllerStatus.Paused:
					case ServiceControllerStatus.PausePending:
					case ServiceControllerStatus.StartPending:
					case ServiceControllerStatus.Running:
						dnscryptService.Stop();
						break;
				}
				return dnscryptService.Status == ServiceControllerStatus.Stopped;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
		}

		/// <summary>
		///     Start the dnscrypt-proxy service.
		/// </summary>
		/// <returns><c>true</c> on success, otherwise <c>false</c></returns>
		public static bool Start()
		{
			try
			{
				var dnscryptService = new ServiceController { ServiceName = DnsCryptProxyServiceName };

				var proxyStatus = dnscryptService.Status;
				switch (proxyStatus)
				{
					case ServiceControllerStatus.ContinuePending:
					case ServiceControllerStatus.Paused:
					case ServiceControllerStatus.PausePending:
					case ServiceControllerStatus.Stopped:
					case ServiceControllerStatus.StopPending:
						dnscryptService.Start();
						break;
				}
				return dnscryptService.Status == ServiceControllerStatus.Running;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
		}

		/// <summary>
		/// Get the version of the dnscrypt-proxy.exe.
		/// </summary>
		/// <returns></returns>
		public static string GetVersion()
		{
			var dnsCryptProxyExecutablePath = DnsCryptProxyExecutablePath;
			var result = ProcessHelper.ExecuteWithArguments(dnsCryptProxyExecutablePath, "-version");
			return result.Success ? result.StandardOutput.Replace(Environment.NewLine, "") : string.Empty;
		}

		/// <summary>
		///  Check the configuration file.
		/// </summary>
		/// <returns></returns>
		public static bool IsConfigurationFileValid()
		{
			try
			{
				var dnsCryptProxyExecutablePath = DnsCryptProxyExecutablePath;
				var result = ProcessHelper.ExecuteWithArguments(dnsCryptProxyExecutablePath, "-check");
				return result.Success;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
		}

		/// <summary>
		/// Get the list of available (active) resolvers for the enabled filters.
		/// </summary>
		/// <returns></returns>
		public static List<AvailableResolver> GetAvailableResolvers()
		{
			var resolvers = new List<AvailableResolver>();
			var dnsCryptProxyExecutablePath = DnsCryptProxyExecutablePath;
			var result = ProcessHelper.ExecuteWithArguments(dnsCryptProxyExecutablePath, "-list -json");
			if (!result.Success) return resolvers;
			if (string.IsNullOrEmpty(result.StandardOutput)) return resolvers;
			try
			{
				var res = JsonConvert.DeserializeObject<List<AvailableResolver>>(result.StandardOutput);
				if (res.Count > 0)
				{
					resolvers = res;
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
			return resolvers;
		}

		/// <summary>
		/// Get the list of all resolvers.
		/// </summary>
		/// <returns></returns>
		public static List<AvailableResolver> GetAllResolversWithoutFilters()
		{
			var resolvers = new List<AvailableResolver>();
			var dnsCryptProxyExecutablePath = DnsCryptProxyExecutablePath;
			var result = ProcessHelper.ExecuteWithArguments(dnsCryptProxyExecutablePath, "-list-all -json");
			if (!result.Success) return resolvers;
			if (string.IsNullOrEmpty(result.StandardOutput)) return resolvers;
			try
			{
				var res = JsonConvert.DeserializeObject<List<AvailableResolver>>(result.StandardOutput);
				if (res.Count > 0)
				{
					resolvers = res;
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
			return resolvers;
		}

		/// <summary>
		/// Install the dnscrypt-proxy service.
		/// </summary>
		/// <returns></returns>
		public static bool Install()
		{
			var dnsCryptProxyExecutablePath = DnsCryptProxyExecutablePath;
			var result = ProcessHelper.ExecuteWithArguments(dnsCryptProxyExecutablePath, "-service install");
			if (result.Success)
			{
				return true;
			}

			try
			{
				if (string.IsNullOrEmpty(result.StandardError)) return false;
				if (result.StandardError.Contains("SYSTEM\\CurrentControlSet\\Services\\EventLog\\Application\\dnscrypt-proxy"))
				{
					Registry.LocalMachine.DeleteSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application\dnscrypt-proxy",
						false);
					return Install();
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}

			return false;
		}

		/// <summary>
		/// Uninstall the dnscrypt-proxy service.
		/// </summary>
		/// <returns></returns>
		public static bool Uninstall()
		{
			var dnsCryptProxyExecutablePath = DnsCryptProxyExecutablePath;
			var result = ProcessHelper.ExecuteWithArguments(dnsCryptProxyExecutablePath, "-service uninstall");
			try
			{
				var eventLogKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application\dnscrypt-proxy",
					RegistryRights.ReadKey);
				var eventLogKeyValue = eventLogKey?.GetValue("CustomSource");
				if (eventLogKeyValue != null)
				{
					Registry.LocalMachine.DeleteSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog\Application\dnscrypt-proxy", false);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}

			return result.Success;
		}
	}
}
