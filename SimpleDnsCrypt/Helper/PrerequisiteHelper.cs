using Caliburn.Micro;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleDnsCrypt.Config;

namespace SimpleDnsCrypt.Helper
{
	public static class  PrerequisiteHelper
	{
		private static readonly ILog Log = LogManagerHelper.Factory();

		public static bool IsRedistributablePackageInstalled()
		{
			try
			{
				if (Environment.Is64BitProcess)
				{
					var parametersVc2017X64 = Registry.ClassesRoot.OpenSubKey(@"Installer\Dependencies\,,amd64,14.0,bundle", false);
					var vc2017X64Version = parametersVc2017X64?.GetValue("Version");
					if (vc2017X64Version == null) return false;
					if (((string) vc2017X64Version).StartsWith("14"))
					{
						return true;
					}
				}
				else
				{
					var parametersVc2017X86 = Registry.ClassesRoot.OpenSubKey(@"Installer\Dependencies\,,x86,14.0,bundle", false);
					var vc2017X86Version = parametersVc2017X86?.GetValue("Version");
					if (vc2017X86Version == null) return false;
					if (((string)vc2017X86Version).StartsWith("14"))
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
		}

		public static async Task DownloadAndInstallRedistributablePackage()
		{
			try
			{
				var url = Environment.Is64BitProcess ? Global.RedistributablePackage64 : Global.RedistributablePackage86;
				var path = Path.Combine(Path.GetTempPath(), "VC_redist.exe");
				using (var client = new HttpClient())
				{
					var getDataTask = client.GetByteArrayAsync(url);
					var file = await getDataTask.ConfigureAwait(false);
					if (file != null)
					{
						File.WriteAllBytes(path, file);
					}
				}
				if (File.Exists(path))
				{
					const string arguments = "/install /passive /norestart";
					var startInfo = new ProcessStartInfo(path)
					{
						Arguments = arguments,
						UseShellExecute = false
					};
					Process.Start(startInfo);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}
	}
}
