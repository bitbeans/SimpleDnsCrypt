using SimpleDnsCrypt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimpleDnsCrypt.Tools
{
	public class InsecureResolvers : IDisposable
	{
		private readonly string _configFile;
		private List<InsecureResolverPair> _insecureResolverPairs;

		public InsecureResolvers()
		{

		}

		public InsecureResolvers(string configFile)
		{
			_configFile = configFile;
			_insecureResolverPairs = new List<InsecureResolverPair>();
			LoadConfigurationFile();
		}

		public List<InsecureResolverPair> InsecureResolverPairs
		{
			get => _insecureResolverPairs;
			set
			{
				if (value.Equals(_insecureResolverPairs)) return;
				_insecureResolverPairs = value;
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
					var deserializer = new DeserializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
					var storedConfiguration = deserializer.Deserialize<InsecureResolvers>(userDataConfigFile);

					if (storedConfiguration.InsecureResolverPairs != null)
					{
						_insecureResolverPairs = storedConfiguration.InsecureResolverPairs;
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
				var insecureResolversFile = Path.Combine(Directory.GetCurrentDirectory(), _configFile);
				using (var insecureResolversFileConfigFile = new StreamWriter(insecureResolversFile))
				{
					var serializer = new SerializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
					serializer.Serialize(insecureResolversFileConfigFile, this);
				}
			}
			catch (Exception)
			{

			}
		}
	}
}
