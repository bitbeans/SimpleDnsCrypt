using Nett;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using System;
using System.IO;

namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	/// 
	/// </summary>
	public static class DnscryptProxyConfigurationManager
	{
		/// <summary>
		/// 
		/// </summary>
		public static DnscryptProxyConfiguration DnscryptProxyConfiguration { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static bool LoadConfiguration()
		{
			try
			{
				var configFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.DnsCryptConfigurationFile);
				if (!File.Exists(configFile)) return false;
				var settings = TomlSettings.Create(s => s.ConfigurePropertyMapping(m => m.UseTargetPropertySelector(standardSelectors => standardSelectors.IgnoreCase)));
				DnscryptProxyConfiguration = Toml.ReadFile<DnscryptProxyConfiguration>(configFile, settings);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static bool SaveConfiguration()
		{
			try
			{
				var configFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.DnsCryptConfigurationFile);
				var settings = TomlSettings.Create(s => s.ConfigurePropertyMapping(m => m.UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));
				Toml.WriteFile(DnscryptProxyConfiguration, configFile, settings);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
