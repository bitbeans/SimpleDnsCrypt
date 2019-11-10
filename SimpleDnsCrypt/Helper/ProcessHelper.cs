using Caliburn.Micro;
using SimpleDnsCrypt.Models;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SimpleDnsCrypt.Helper
{
	public static class ProcessHelper
	{
		private static readonly ILog Log = LogManagerHelper.Factory();

		/// <summary>
		///		Execute process with arguments
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static ProcessResult ExecuteWithArguments(string filename, string arguments)
		{
			var processResult = new ProcessResult();
			try
			{
				const int timeout = 9000;
				using (var process = new Process())
				{
					process.StartInfo.FileName = filename;
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
