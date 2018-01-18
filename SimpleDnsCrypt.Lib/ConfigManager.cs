using System;
using Nett;
using SimpleDnsCrypt.Lib.Models;

namespace SimpleDnsCrypt.Lib
{
	/// <summary>
	/// 
	/// </summary>
    public static class ConfigManager
    {
		/// <summary>
		/// 
		/// </summary>
		private const string ConfigurationFile = "dnscrypt-proxy.toml";

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
			    DnscryptProxyConfiguration = Toml.ReadFile<DnscryptProxyConfiguration>(ConfigurationFile);
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
				Toml.WriteFile(DnscryptProxyConfiguration, ConfigurationFile);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
