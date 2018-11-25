using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Caliburn.Micro;
using Microsoft.Win32;
using Nett;
using Newtonsoft.Json;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	///     Class to manage the dnscrypt-proxy service and maintain the registry.
	/// </summary>
	public static class DnsCryptProxyManager
	{
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
			var result = ExecuteWithArguments("-version");
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
				var result = ExecuteWithArguments("-check");
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
			var result = ExecuteWithArguments("-list -json");
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
			var result = ExecuteWithArguments("-list-all -json");
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
		/// Get the list of available (inactive) resolvers for the enabled filters.
		/// </summary>
		/// <returns></returns>
		[Obsolete("Use GetAllResolversWithoutFilters instead.")]
		public static List<AvailableResolver> GetAllResolversWithFilters()
		{
			var resolvers = new List<AvailableResolver>();
			var dnscryptFolder = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder);
			// we use a second config file
			var tmpToml = Path.Combine(dnscryptFolder, "_tmp_.toml");
			if (File.Exists(tmpToml))
			{
				File.Delete(tmpToml);
			}
			File.Copy(Path.Combine(dnscryptFolder, Global.DnsCryptConfigurationFile), Path.Combine(dnscryptFolder, tmpToml));
			var config = Toml.ReadFile<DnscryptProxyConfiguration>(tmpToml);
			//clear the array
			config.server_names = null;
			Toml.WriteFile(config, tmpToml);
			var result = ExecuteWithArguments("-config _tmp_.toml -list -json");
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
			finally
			{
				if (File.Exists(tmpToml))
				{
					File.Delete(tmpToml);
				}
			}
			return resolvers;
		}


		/// <summary>
		/// Install the dnscrypt-proxy service.
		/// </summary>
		/// <returns></returns>
		public static bool Install()
		{
			var result = ExecuteWithArguments("-service install");
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
			var result = ExecuteWithArguments("-service uninstall");
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


		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		private static ProcessResult ExecuteWithArguments(string arguments)
		{
			var processResult = new ProcessResult();
			try
			{
				var dnsCryptProxyExecutablePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
					Global.DnsCryptProxyExecutableName);
				if (!File.Exists(dnsCryptProxyExecutablePath))
				{
					throw new Exception($"Missing {dnsCryptProxyExecutablePath}");
				}

				const int timeout = 9000;
				using (var process = new Process())
				{
					process.StartInfo.FileName = dnsCryptProxyExecutablePath;
					process.StartInfo.Arguments = arguments;
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.RedirectStandardError = true;

					var output = new StringBuilder();
					var error = new StringBuilder();

					using (var outputWaitHandle = new AutoResetEvent(false))
					using (var errorWaitHandle = new AutoResetEvent(false))
					{
						process.OutputDataReceived += (sender, e) =>
						{
							if (e.Data == null)
							{
								outputWaitHandle.Set();
							}
							else
							{
								output.AppendLine(e.Data);
							}
						};
						process.ErrorDataReceived += (sender, e) =>
						{
							if (e.Data == null)
							{
								errorWaitHandle.Set();
							}
							else
							{
								error.AppendLine(e.Data);
							}
						};
						process.Start();
						process.BeginOutputReadLine();
						process.BeginErrorReadLine();
						if (process.WaitForExit(timeout) &&
							outputWaitHandle.WaitOne(timeout) &&
							errorWaitHandle.WaitOne(timeout))
						{
							if (process.ExitCode == 0)
							{
								processResult.StandardOutput = output.ToString();
								processResult.StandardError = error.ToString();
								processResult.Success = true;
							}
							else
							{
								processResult.StandardOutput = output.ToString();
								processResult.StandardError = error.ToString();
								Log.Warn(processResult.StandardError);
								processResult.Success = false;
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
			catch (Exception exception)
			{
				Log.Error(exception);
				processResult.StandardError = exception.Message;
				processResult.Success = false;
			}
			return processResult;
		}
	}
}
