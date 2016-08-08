using System;
using System.IO;
using System.Linq;
using SimpleDnsCrypt.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimpleDnsCrypt.Tools
{
	/// <summary>
	///		Class to handle the configuration file.
	/// </summary>
	public class UserData : IDisposable
	{
		private readonly string _configFile;
		private string _language;
		private string _primaryResolver;
		private string _secondaryResolver;
		private int _primaryResolverPort;
		private int _secondaryResolverPort;
		private bool _useIpv6;
		private bool _updateResolverListOnStart;

		public UserData(string configFile)
		{
			_configFile = configFile;
			// set default values
			_language = "auto";
			_primaryResolver = "auto";
			_secondaryResolver = "auto";
			_useIpv6 = false;
			_updateResolverListOnStart = Global.UpdateResolverListOnStart;
			_primaryResolverPort = Global.PrimaryResolverPort;
			_secondaryResolverPort = Global.SecondaryResolverPort;
			// load configuration file (if exists) and overwrite the default values
			LoadConfigurationFile();
			// update the configuration file
			SaveConfigurationFile();
		}

		public UserData()
		{
		}

		public string Language
		{
			get { return _language; }
			set
			{
				if (value.Equals(_language)) return;
				_language = value;
			}
		}

		public string PrimaryResolver
		{
			get { return _primaryResolver; }
			set
			{
				if (value.Equals(_primaryResolver)) return;
				_primaryResolver = value;
			}
		}

		public bool UpdateResolverListOnStart
		{
			get { return _updateResolverListOnStart; }
			set
			{
				if (value.Equals(_updateResolverListOnStart)) return;
				_updateResolverListOnStart = value;
			}
		}

		public bool UseIpv6
		{
			get { return _useIpv6; }
			set
			{
				if (value.Equals(_useIpv6)) return;
				_useIpv6 = value;
			}
		}

		public string SecondaryResolver
		{
			get { return _secondaryResolver; }
			set
			{
				if (value.Equals(_secondaryResolver)) return;
				_secondaryResolver = value;
			}
		}

		[YamlIgnore]
		public int PrimaryResolverPort
		{
			get { return _primaryResolverPort; }
			set
			{
				if (value.Equals(_primaryResolverPort)) return;
				_primaryResolverPort = value;
			}
		}

		[YamlIgnore]
		public int SecondaryResolverPort
		{
			get { return _secondaryResolverPort; }
			set
			{
				if (value.Equals(_secondaryResolverPort)) return;
				_secondaryResolverPort = value;
			}
		}

		public void Dispose()
		{
		}

		private void LoadConfigurationFile()
		{
			try
			{
				if (!File.Exists(_configFile)) return;
				using (var userDataConfigFile = new StreamReader(_configFile))
				{
					var deserializer = new Deserializer(namingConvention: new PascalCaseNamingConvention());
					var storedConfiguration = deserializer.Deserialize<UserData>(userDataConfigFile);

					if (!string.IsNullOrEmpty(storedConfiguration.Language))
					{
						var l = storedConfiguration.Language.Trim().ToLower();
						_language = Global.SupportedLanguages.Contains(l) ? l : "auto";
					}
					else
					{
						_language = "auto";
					}

					_useIpv6 = storedConfiguration.UseIpv6;
					_updateResolverListOnStart = storedConfiguration.UpdateResolverListOnStart;

					if (!string.IsNullOrEmpty(storedConfiguration.PrimaryResolver))
					{
						_primaryResolver = storedConfiguration.PrimaryResolver.Trim().ToLower();
					}

					if (!string.IsNullOrEmpty(storedConfiguration.SecondaryResolver))
					{
						_secondaryResolver = storedConfiguration.SecondaryResolver.Trim().ToLower();
					}

					
				}
			}
			catch (Exception)
			{
			}
		}

		public void SaveConfigurationFile()
		{
			try
			{
				var userDataFile = Path.Combine(Directory.GetCurrentDirectory(), _configFile);
				using (var userDataConfigFile = new StreamWriter(userDataFile))
				{
					var serializer = new Serializer(namingConvention: new PascalCaseNamingConvention());
					serializer.Serialize(userDataConfigFile, this);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}