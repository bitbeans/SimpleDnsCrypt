using System;
using System.Reflection;

namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	///     Helper class to show retrieve the application version.
	/// </summary>
	public static class VersionHelper
	{
		/// <summary>
		///     Get the current Application version.
		/// </summary>
		public static string PublishVersion
		{
			get
			{
				try
				{
					var version = Assembly.GetExecutingAssembly().GetName().Version;
					return $"{version.Major}.{version.Minor}.{version.Build}";
				}
				catch (FormatException)
				{
					return "0";
				}
				catch (ArgumentNullException)
				{
					return "0";
				}
			}
		}

		public static string PublishBuild => Environment.Is64BitProcess ? "(x64)" : "(x86)";
	}
}
