using System;
using System.Diagnostics;
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
                UninstallKnownServices();
            }
            finally
            {
                Environment.Exit(0);
            }
        }

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
                        Console.WriteLine(string.Format("Removing {0}", service));
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