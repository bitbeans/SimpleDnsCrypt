using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	///     Class to manage the dnscrypt-proxy service and maintain the registry.
	/// </summary>
	public static class DnsCryptProxyManager
	{
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
			catch (Exception)
			{
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
			catch (Exception)
			{
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
			catch (Exception)
			{
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
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		///     Install the dnscrypt-proxy service.
		/// </summary>
		/// <returns>A ProcessResult.</returns>
		public static ProcessResult Install()
		{
			var processResult = new ProcessResult();
			try
			{
				if (!IsDnsCryptProxyInstalled())
				{
					const int timeout = 9000;
					const string arguments = "-service install";

					using (var process = new Process())
					{
						process.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.DnsCryptProxyExecutableName);
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
			}
			catch (Exception exception)
			{
				processResult.StandardError = exception.Message;
				processResult.Success = false;
			}
			return processResult;
		}
	}
}
