using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Tools
{
    /// <summary>
    ///     Class to manage the dnscrypt-proxy service and maintain the registry.
    /// </summary>
    public class DnsCryptProxyManager
    {
        /// <summary>
        ///     Initialize a new DnsCryptProxyManager instance.
        /// </summary>
        /// <param name="dnsCryptProxyType"></param>
        public DnsCryptProxyManager(DnsCryptProxyType dnsCryptProxyType = DnsCryptProxyType.Primary)
        {
            DnsCryptProxy = new DnsCryptProxy(dnsCryptProxyType);
            ReadRegistry(dnsCryptProxyType);
        }

        /// <summary>
        ///     The DnsCryptProxy of this instance.
        /// </summary>
        public DnsCryptProxy DnsCryptProxy { get; set; }

        /// <summary>
        ///     Check if the DNSCrypt proxy service is installed.
        /// </summary>
        /// <returns><c>true</c> if the service is installed, otherwise <c>false</c></returns>
        /// <exception cref="Win32Exception">An error occurred when accessing a system API. </exception>
        public bool IsDnsCryptProxyInstalled()
        {
            try
            {
                if (!DnsCryptProxy.IsReady) return false;
                if (DnsCryptProxy.DisplayName == null) return false;
                var dnscryptService = new ServiceController {ServiceName = DnsCryptProxy.DisplayName};
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
        public bool IsDnsCryptProxyRunning()
        {
	        if (DnsCryptProxy.DisplayName == null) return false;
            try
            {
                if (!DnsCryptProxy.IsReady) return false;
                var dnscryptService = new ServiceController {ServiceName = DnsCryptProxy.DisplayName};

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
        public bool Restart()
        {
            try
            {
                var dnscryptService = new ServiceController {ServiceName = DnsCryptProxy.DisplayName};
                dnscryptService.Stop();
				Thread.Sleep(1000);
                dnscryptService.Start();
                return (dnscryptService.Status == ServiceControllerStatus.Running);
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
        public bool Stop()
        {
            try
            {
                var dnscryptService = new ServiceController {ServiceName = DnsCryptProxy.DisplayName};
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
                return (dnscryptService.Status == ServiceControllerStatus.Stopped);
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
        public bool Start()
        {
            try
            {
                var dnscryptService = new ServiceController {ServiceName = DnsCryptProxy.DisplayName};

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
                return (dnscryptService.Status == ServiceControllerStatus.Running);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Uninstall the dnscrypt-proxy service.
        /// </summary>
        /// <returns>A ProcessResult.</returns>
        public ProcessResult Uninstall()
        {
            var processResult = new ProcessResult();
            try
            {
                // we do not check if the proxy is installed,
                // just let them clear the registry.
                const int timeout = 9000;
                using (var process = new Process())
                {
	                process.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, DnsCryptProxy.Type == DnsCryptProxyType.Primary ? Global.DnsCryptProxyExecutableName : Global.DnsCryptProxyExecutableSecondaryName);
	                process.StartInfo.Arguments = "--uninstall";
					process.StartInfo.Arguments += " --service-name=" + DnsCryptProxy.DisplayName;
					process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.Start();
                    if (process.WaitForExit(timeout))
                    {
                        if (process.ExitCode == 0)
                        {
                            processResult.Success = true;
                        }
                        else
                        {
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
            catch (Exception exception)
            {
                processResult.StandardError = exception.Message;
                processResult.Success = false;
            }

            return processResult;
        }

        /// <summary>
        ///     Install the dnscrypt-proxy service.
        /// </summary>
        /// <returns>A ProcessResult.</returns>
        public ProcessResult Install()
        {
            var processResult = new ProcessResult();
            try
            {
                if (!IsDnsCryptProxyInstalled())
                {
                    const bool actAsGateway = false;
                    const int timeout = 9000;

                    var arguments = "--install";
                    // update the registry
                    WriteRegistry(DnsCryptProxy.Type);

                    arguments += " -R \"" + DnsCryptProxy.Parameter.ResolverName + "\"";
                    arguments += " -L \"" +
                                 Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
                                     Global.DnsCryptProxyResolverListName) + "\"";
                    if (actAsGateway)
                    {
                        arguments += " -a 0.0.0.0";
                    }
                    if (DnsCryptProxy.Type == DnsCryptProxyType.Primary)
                    {
						arguments += " --service-name="+ Global.PrimaryResolverServiceName;
						arguments += " -a " + Global.PrimaryResolverAddress + ":" + Global.PrimaryResolverPort;
                    }
                    else
                    {
						arguments += " --service-name=" + Global.SecondaryResolverServiceName;
						arguments += " -a " + Global.SecondaryResolverAddress + ":" + Global.SecondaryResolverPort;
                    }
                    // always use ephermeral keys
                    arguments += " -E";
                    using (var process = new Process())
                    {
	                    process.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, 
							DnsCryptProxy.Type == DnsCryptProxyType.Primary ? Global.DnsCryptProxyExecutableName : Global.DnsCryptProxyExecutableSecondaryName);
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
                                    ReadRegistry(DnsCryptProxy.Type);
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

        /// <summary>
        ///     Write the data to the registry.
        /// </summary>
        /// <param name="dnsCryptProxyType">Type of the proxy (primary or secondary)</param>
        /// <returns><c>true</c> on success, otherwise <c>false</c></returns>
        public bool WriteRegistry(DnsCryptProxyType dnsCryptProxyType)
        {
            try
            {
                var proxyName = Global.PrimaryResolverServiceName;
	            var proxyPort = DnsCryptProxy.Parameter.LocalPort;
                if (dnsCryptProxyType != DnsCryptProxyType.Primary)
                {
                    proxyName = Global.SecondaryResolverServiceName;
                    proxyPort = Global.SecondaryResolverPort;
                }

                var localMachine = Registry.LocalMachine;
				var parameters = localMachine.OpenSubKey(
                    @"SYSTEM\\CurrentControlSet\\Services\\" + proxyName + "\\Parameters", true);

                if (parameters == null)
                {
                    localMachine.CreateSubKey(@"SYSTEM\\CurrentControlSet\\Services\\" + proxyName + "\\Parameters");
                    parameters = localMachine.OpenSubKey(
                        @"SYSTEM\\CurrentControlSet\\Services\\" + proxyName + "\\Parameters", true);
                }
				
                parameters.SetValue("ResolverName", DnsCryptProxy.Parameter.ResolverName, RegistryValueKind.String);
                parameters.SetValue("Plugins", DnsCryptProxy.Parameter.Plugins, RegistryValueKind.MultiString);
                parameters.SetValue("LocalAddress", DnsCryptProxy.Parameter.LocalAddress + ":" + proxyPort,
                    RegistryValueKind.String);
                parameters.SetValue("ProviderKey", DnsCryptProxy.Parameter.ProviderKey, RegistryValueKind.String);
                parameters.SetValue("ResolversList", DnsCryptProxy.Parameter.ResolversList, RegistryValueKind.String);
                parameters.SetValue("ProviderName", DnsCryptProxy.Parameter.ProviderName, RegistryValueKind.String);
                parameters.SetValue("ResolverAddress", DnsCryptProxy.Parameter.ResolverAddress, RegistryValueKind.String);
                parameters.SetValue("EphemeralKeys", Convert.ToInt32(DnsCryptProxy.Parameter.EphemeralKeys),
                    RegistryValueKind.DWord);
                parameters.SetValue("TCPOnly", Convert.ToInt32(DnsCryptProxy.Parameter.TcpOnly), RegistryValueKind.DWord);
				return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

		/// <summary>
		///	Add quotation marks for existing installations.
		/// This fixes a vulnerability, where the dnscrypt-proxy 
		/// path contains spaces.
		/// </summary>
		/// <remarks>Thanks to @rugk!</remarks>
		/// <param name="dnsCryptProxyType"></param>
		public void FixImagePath(DnsCryptProxyType dnsCryptProxyType)
	    {
			var proxyName = Global.PrimaryResolverServiceName;
			if (dnsCryptProxyType != DnsCryptProxyType.Primary)
			{
				proxyName = Global.SecondaryResolverServiceName;
			}

			var localMachine = Registry.LocalMachine;
			var main = localMachine.OpenSubKey(
				@"SYSTEM\\CurrentControlSet\\Services\\" + proxyName, true);

			if (main == null)
			{
				return;
			}
		    var imagePath = string.Empty;
			foreach (var mainName in main.GetValueNames())
			{
				switch (mainName)
				{
					case "ImagePath":
						imagePath = (string)main.GetValue(mainName);
						break;
				}
			}

		    if (string.IsNullOrEmpty(imagePath)) return;

		    if (!imagePath.StartsWith("\""))
		    {
			    imagePath = "\"" + imagePath;
		    }

			if (!imagePath.EndsWith("\""))
			{
				imagePath = imagePath + "\"";
			}
			main.SetValue("ImagePath", imagePath, RegistryValueKind.ExpandString);
	    }

        /// <summary>
        ///     Read the current registry values.
        /// </summary>
        /// <param name="dnsCryptProxyType">Type of the proxy (primary or secondary)</param>
        public void ReadRegistry(DnsCryptProxyType dnsCryptProxyType)
        {
            try
            {
	            FixImagePath(dnsCryptProxyType);
				var proxyName = Global.PrimaryResolverServiceName;
                if (dnsCryptProxyType != DnsCryptProxyType.Primary)
                {
                    proxyName = Global.SecondaryResolverServiceName;
                }

                var localMachine = Registry.LocalMachine;
                var main = localMachine.OpenSubKey(
                    @"SYSTEM\\CurrentControlSet\\Services\\" + proxyName, false);

                if (main == null)
                {
                    DnsCryptProxy.IsReady = false;
                    return;
                }

                foreach (var mainName in main.GetValueNames())
                {
                    switch (mainName)
                    {
                        case "DisplayName":
                            DnsCryptProxy.DisplayName = (string) main.GetValue(mainName);
                            break;
                        case "ImagePath":
                            DnsCryptProxy.ImagePath = (string) main.GetValue(mainName);
                            break;
                    }
                }
                var parameters = localMachine.OpenSubKey(
                    @"SYSTEM\\CurrentControlSet\\Services\\" + proxyName + "\\Parameters", false);
                foreach (var parameterName in parameters.GetValueNames())
                {
                    switch (parameterName)
                    {
                        case "EphemeralKeys":
                            DnsCryptProxy.Parameter.EphemeralKeys = Convert.ToBoolean(parameters.GetValue(parameterName));
                            break;
                        case "LocalAddress":
							// the LocalAddress is stored in the following format: 127.0.0.1:53
		                    var mergedAddress = ((string) parameters.GetValue(parameterName)).Split(':');
                            DnsCryptProxy.Parameter.LocalAddress = mergedAddress[0];
							DnsCryptProxy.Parameter.LocalPort = Convert.ToInt32(mergedAddress[1]);
							break;
                        case "Plugins":
                            DnsCryptProxy.Parameter.Plugins = (string[]) parameters.GetValue(parameterName);
                            break;
                        case "ProviderName":
                            DnsCryptProxy.Parameter.ProviderName = (string) parameters.GetValue(parameterName);
                            break;
                        case "ProviderKey":
                            DnsCryptProxy.Parameter.ProviderKey = (string) parameters.GetValue(parameterName);
                            break;
                        case "ResolversList":
                            DnsCryptProxy.Parameter.ResolversList = (string) parameters.GetValue(parameterName);
                            break;
                        case "ResolverAddress":
                            DnsCryptProxy.Parameter.ResolverAddress = (string) parameters.GetValue(parameterName);
                            break;
                        case "ResolverName":
                            DnsCryptProxy.Parameter.ResolverName = (string) parameters.GetValue(parameterName);
                            break;
                        case "TCPOnly":
                            DnsCryptProxy.Parameter.TcpOnly = Convert.ToBoolean(parameters.GetValue(parameterName));
                            break;
                        case "EDNSPayloadSize":
                            // not yet supported, will be ignored
                            break;
                        case "MaxActiveRequests":
                            // not yet supported, will be ignored
                            break;
                        case "ClientKeyFile":
                            // not yet supported, will be ignored
                            break;
						case "IgnoreTimestamps":
							// not yet supported, will be ignored
							break;
						case "LogFile":
							// not yet supported, will be ignored
							break;
						case "LogLevel":
							// not yet supported, will be ignored
							break;
						case "ConfigFile":
							// not yet supported, will be ignored
							break;
					}
                }
                DnsCryptProxy.IsReady = true;
            }
            catch (Exception)
            {
                DnsCryptProxy.IsReady = false;
            }
        }
    }
}