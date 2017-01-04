using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Tools;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Application = System.Windows.Application;
using Screen = Caliburn.Micro.Screen;

namespace SimpleDnsCrypt.ViewModels
{
	[Export]
	public class BlockViewModel : Screen
	{
		private readonly IWindowManager _windowManager;

		public List<string> Plugins
		{
			get { return _plugins; }
			set
			{
				_plugins = value;
				NotifyOfPropertyChange(() => Plugins);
			}
		}

		[ImportingConstructor]
		public BlockViewModel(IWindowManager windowManager)
		{
			_windowManager = windowManager;
			LoadDomainBlacklist();
			LoadAddressBlacklist();
			var localDomainRules = new CollectionContainer {Collection = DomainBlacklist.LocalRules};
			var remoteDomainRules = new CollectionContainer {Collection = DomainBlacklist.RemoteRules};
			var addressRules = new CollectionContainer {Collection = AddressBlacklist.Rules};
			DomainRuleCollection.Add(localDomainRules);
			DomainRuleCollection.Add(remoteDomainRules);
			AddressRuleCollection.Add(addressRules);
			_isWorking = false;
			SetPlugins(MainViewModel.Instance.Plugins);

			if (File.Exists(_domainBlacklistPath))
			{
				UpdateDomainBlacklistPathInfoString();
			}

			if (File.Exists(_addressBlacklistPath))
			{
				UpdateAddressBlacklistPathInfoString();
			}
		}

		private void UpdateDomainBlacklistPathInfoString()
		{
			DomainBlacklistPathInfoString = string.Format("({0} {1}; {2})", File.ReadLines(_domainBlacklistPath).Count(),
				LocalizationEx.GetUiString("blacklist_entries",
					Thread.CurrentThread.CurrentCulture), new FileInfo(_domainBlacklistPath).LastWriteTime);
		}

		private void UpdateAddressBlacklistPathInfoString()
		{
			AddressBlacklistPathInfoString = string.Format("({0} {1}; {2})", File.ReadLines(_addressBlacklistPath).Count(),
				LocalizationEx.GetUiString("blacklist_entries", Thread.CurrentThread.CurrentCulture),
				new FileInfo(_addressBlacklistPath).LastWriteTime);
		}

		public void SetPlugins(List<string> plugins)
		{
			_plugins = plugins;
			foreach (var plugin in _plugins)
			{
				if (plugin.StartsWith(Global.LibdcpluginLdns))
				{
					var a = plugin.Split(',');

					for (var r = 1; r < a.Length; r++)
					{
						if (a[r].StartsWith("--ips"))
						{
							var b = a[r].Split('=');
							_addressBlacklistPath = b[1];
						}
						if (a[r].StartsWith("--domains"))
						{
							var b = a[r].Split('=');
							_domainBlacklistPath = b[1];
						}
					}

					if (!string.IsNullOrEmpty(_addressBlacklistPath))
					{
						_addressBlacklistPlugin = true;
					}

					if (!string.IsNullOrEmpty(_domainBlacklistPath))
					{
						_domainBlacklistPlugin = true;
					}
				}
			}
		}

		public bool IsWorking
		{
			get { return _isWorking; }
			set
			{
				_isWorking = value;
				NotifyOfPropertyChange(() => IsWorking);
			}
		}

		public CompositeCollection DomainRuleCollection { get; } = new CompositeCollection();
		public CompositeCollection AddressRuleCollection { get; } = new CompositeCollection();

		public object SelectedDomainBlockRule { get; set; }

		public object SelectedAddressBlockRule { get; set; }

		private DomainBlacklist _domainBlacklist;
		private AddressBlacklist _addressBlacklist;
		private bool _isWorking;
		private string _domainBlacklistPath;
		private string _addressBlacklistPath;
		private bool _addressBlacklistPlugin;
		private bool _domainBlacklistPlugin;
		private List<string> _plugins;
		private string _domainBlacklistPathInfoString;
		private string _addressBlacklistPathInfoString;

		public DomainBlacklist DomainBlacklist
		{
			get { return _domainBlacklist; }
			set
			{
				_domainBlacklist = value;
				NotifyOfPropertyChange(() => DomainBlacklist);
			}
		}

		public AddressBlacklist AddressBlacklist
		{
			get { return _addressBlacklist; }
			set
			{
				_addressBlacklist = value;
				NotifyOfPropertyChange(() => AddressBlacklist);
			}
		}

		private void SaveDomainBlacklist()
		{
			try
			{
				var domainBlacklistFilePath = Path.Combine("data", Global.DomainBlacklistConfigFile);
				using (var domainBlacklistWriter = new StreamWriter(domainBlacklistFilePath))
				{
					var serializer = new SerializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
					serializer.Serialize(domainBlacklistWriter, DomainBlacklist);
				}
			}
			catch (Exception)
			{
			}
		}

		private void LoadDomainBlacklist()
		{
			try
			{
				DomainBlacklist = new DomainBlacklist();
				var domainBlacklistFilePath = Path.Combine("data", Global.DomainBlacklistConfigFile);
				if (!File.Exists(domainBlacklistFilePath)) return;
				using (var domainBlacklistReader = new StreamReader(domainBlacklistFilePath))
				{
					var deserializer = new DeserializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
					var domainBlacklist = deserializer.Deserialize<DomainBlacklist>(domainBlacklistReader);
					DomainBlacklist = domainBlacklist ?? new DomainBlacklist();
				}
			}
			catch (Exception)
			{
			}
		}

		private void SaveAddressBlacklist()
		{
			try
			{
				var addressBlacklistFilePath = Path.Combine("data", Global.AddressBlacklistConfigFile);
				using (var addressBlacklistWriter = new StreamWriter(addressBlacklistFilePath))
				{
					var serializer = new SerializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
					serializer.Serialize(addressBlacklistWriter, AddressBlacklist);
				}
			}
			catch (Exception)
			{
			}
		}

		private void LoadAddressBlacklist()
		{
			try
			{
				AddressBlacklist = new AddressBlacklist();
				var addressBlacklistFilePath = Path.Combine("data", Global.AddressBlacklistConfigFile);
				if (!File.Exists(addressBlacklistFilePath)) return;
				using (var addressBlacklistReader = new StreamReader(addressBlacklistFilePath))
				{
					var deserializer = new DeserializerBuilder().WithNamingConvention(new PascalCaseNamingConvention()).Build();
					var addressBlacklist = deserializer.Deserialize<AddressBlacklist>(addressBlacklistReader);
					AddressBlacklist = addressBlacklist ?? new AddressBlacklist();
				}
			}
			catch (Exception)
			{
			}
		}

		public void SelectAddressBlacklist()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Multiselect = false,
					CheckFileExists = true
				};
				var result = dialog.ShowDialog();
				if (result != DialogResult.OK) return;
				AddressBlacklistPath = dialog.FileName;
			}
			catch (Exception)
			{
				AddressBlacklistPath = string.Empty;
			}
		}

		public void SelectDomainBlacklist()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Multiselect = false,
					CheckFileExists = true
				};
				var result = dialog.ShowDialog();
				if (result != DialogResult.OK) return;
				DomainBlacklistPath = dialog.FileName;
			}
			catch (Exception)
			{
				DomainBlacklistPath = string.Empty;
			}
		}


		public bool AddressBlacklistPlugin
		{
			get { return _addressBlacklistPlugin; }
			set
			{
				_addressBlacklistPlugin = value;
				if (value)
				{
					if (AddressBlacklistPath != null && File.Exists(AddressBlacklistPath))
					{
						if (File.ReadAllLines(AddressBlacklistPath).Length != 0)
						{
							var foundPluginString = false;
							for (var p = 0; p < _plugins.Count; p++)
							{
								if (_plugins[p].StartsWith(Global.LibdcpluginLdns))
								{
									foundPluginString = true;
									var pluginPartList = _plugins[p].Split(',').ToList();
									var foundIpString = false;
									for (var r = 1; r < pluginPartList.Count; r++)
									{
										if (pluginPartList[r].StartsWith("--ips"))
										{
											pluginPartList[r] = "--ips=" + _addressBlacklistPath;
											foundIpString = true;
										}
									}
									if (!foundIpString)
									{
										pluginPartList.Add("--ips=" + _addressBlacklistPath);
									}
									_plugins[p] = string.Join(",", pluginPartList);
								}
							}
							if (!foundPluginString)
							{
								_plugins.Add(Global.LibdcpluginLdns + ",--ips=" + _addressBlacklistPath);
							}
						}
						else
						{
							//empty file
							_addressBlacklistPlugin = false;
							_windowManager.ShowMetroMessageBox(
								LocalizationEx.GetUiString("blacklist_modal_empty_list_text", Thread.CurrentThread.CurrentCulture),
								LocalizationEx.GetUiString("blacklist_modal_empty_list_header", Thread.CurrentThread.CurrentCulture),
								MessageBoxButton.OK, BoxType.Error);
						}
					}
					else
					{
						_addressBlacklistPlugin = false;
					}
				}
				else
				{
					AddressBlacklistPath = string.Empty;
					AddressBlacklistPathInfoString = string.Empty;
					for (var p = 0; p < _plugins.Count; p++)
					{
						if (_plugins[p].StartsWith(Global.LibdcpluginLdns))
						{
							var pluginPartList = _plugins[p].Split(',').ToList();
							if (pluginPartList.Count > 0)
							{
								for (var r = 1; r < pluginPartList.Count; r++)
								{
									if (pluginPartList[r].StartsWith("--ips"))
									{
										pluginPartList.RemoveAt(r);
									}
								}
								if (pluginPartList.Count > 1)
								{
									_plugins[p] = string.Join(",", pluginPartList);
								}
								else
								{
									_plugins.RemoveAt(p);
								}
							}
						}
					}
				}
				NotifyOfPropertyChange(() => AddressBlacklistPlugin);
				SavePlugin();
			}
		}

		public bool DomainBlacklistPlugin
		{
			get { return _domainBlacklistPlugin; }
			set
			{
				_domainBlacklistPlugin = value;
				if (value)
				{
					if (DomainBlacklistPath != null && File.Exists(DomainBlacklistPath))
					{
						if (File.ReadAllLines(DomainBlacklistPath).Length != 0)
						{
							var foundPluginString = false;
							for (var p = 0; p < _plugins.Count; p++)
							{
								if (_plugins[p].StartsWith(Global.LibdcpluginLdns))
								{
									foundPluginString = true;
									var pluginPartList = _plugins[p].Split(',').ToList();
									var foundDomainString = false;
									for (var r = 1; r < pluginPartList.Count; r++)
									{
										if (pluginPartList[r].StartsWith("--domains"))
										{
											pluginPartList[r] = "--domains=" + _domainBlacklistPath;
											foundDomainString = true;
										}
									}
									if (!foundDomainString)
									{
										pluginPartList.Add("--domains=" + _domainBlacklistPath);
									}
									_plugins[p] = string.Join(",", pluginPartList);
								}
							}
							if (!foundPluginString)
							{
								_plugins.Add(Global.LibdcpluginLdns + ",--domains=" + _domainBlacklistPath);
							}
						}
						else
						{
							//empty file
							_domainBlacklistPlugin = false;
							_windowManager.ShowMetroMessageBox(
								LocalizationEx.GetUiString("blacklist_modal_empty_list_text", Thread.CurrentThread.CurrentCulture),
								LocalizationEx.GetUiString("blacklist_modal_empty_list_header", Thread.CurrentThread.CurrentCulture),
								MessageBoxButton.OK, BoxType.Error);
						}
					}
					else
					{
						_domainBlacklistPlugin = false;
					}
				}
				else
				{
					DomainBlacklistPath = string.Empty;
					DomainBlacklistPathInfoString = string.Empty;
					for (var p = 0; p < _plugins.Count; p++)
					{
						if (_plugins[p].StartsWith(Global.LibdcpluginLdns))
						{
							var pluginPartList = _plugins[p].Split(',').ToList();
							if (pluginPartList.Count > 0)
							{
								for (var r = 1; r < pluginPartList.Count; r++)
								{
									if (pluginPartList[r].StartsWith("--domains"))
									{
										pluginPartList.RemoveAt(r);
									}
								}
								if (pluginPartList.Count > 1)
								{
									_plugins[p] = string.Join(",", pluginPartList);
								}
								else
								{
									_plugins.RemoveAt(p);
								}
							}
						}
					}
				}
				NotifyOfPropertyChange(() => DomainBlacklistPlugin);
				SavePlugin();
			}
		}

		private void SavePlugin()
		{
			IsWorking = true;
			MainViewModel.Instance.Plugins = _plugins;
			MainViewModel.Instance.ReloadResolver(DnsCryptProxyType.Primary);
			MainViewModel.Instance.ReloadResolver(DnsCryptProxyType.Secondary);
			IsWorking = false;
		}

		/// <summary>
		///     The full path to the domain blacklist file.
		/// </summary>
		public string DomainBlacklistPath
		{
			get { return _domainBlacklistPath; }
			set
			{
				_domainBlacklistPath = value;
				NotifyOfPropertyChange(() => DomainBlacklistPath);
			}
		}

		public string DomainBlacklistPathInfoString
		{
			get { return _domainBlacklistPathInfoString; }
			set
			{
				_domainBlacklistPathInfoString = value;
				NotifyOfPropertyChange(() => DomainBlacklistPathInfoString);
			}
		}

		public string AddressBlacklistPathInfoString
		{
			get { return _addressBlacklistPathInfoString; }
			set
			{
				_addressBlacklistPathInfoString = value;
				NotifyOfPropertyChange(() => AddressBlacklistPathInfoString);
			}
		}


		/// <summary>
		///     The full path to the address blacklist file.
		/// </summary>
		public string AddressBlacklistPath
		{
			get { return _addressBlacklistPath; }
			set
			{
				_addressBlacklistPath = value;
				NotifyOfPropertyChange(() => AddressBlacklistPath);
			}
		}

		public void AddAddressBlockRule()
		{
			try
			{
				var metroWindow = Application.Current.MainWindow as MetroWindow;
				var metroDialogSettings = new MetroDialogSettings
				{
					AffirmativeButtonText = LocalizationEx.GetUiString("ok", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture)
				};
				var result =
					metroWindow.ShowModalInputExternal(
						LocalizationEx.GetUiString("blacklist_modal_new_address_rule_header", Thread.CurrentThread.CurrentCulture),
						LocalizationEx.GetUiString("blacklist_modal_new_address_rule_text", Thread.CurrentThread.CurrentCulture), metroDialogSettings);
				if (result == null) return;
				var newRule = result.ToLower().Trim();
				if (!ValidateAddressInput(newRule)) return;
				if (AddressBlacklist.Rules.FirstOrDefault(l => l.Equals(newRule)) != null) return;
				AddressBlacklist.Rules.Add(newRule);
				SaveAddressBlacklist();
			}
			catch (Exception)
			{
			}
		}

		public void AddLocalDomainBlockRule()
		{
			try
			{
				var metroWindow = Application.Current.MainWindow as MetroWindow;
				var metroDialogSettings = new MetroDialogSettings
				{
					AffirmativeButtonText = LocalizationEx.GetUiString("ok", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture)
				};
				var result =
					metroWindow.ShowModalInputExternal(
						LocalizationEx.GetUiString("blacklist_modal_new_local_domain_rule_header", Thread.CurrentThread.CurrentCulture),
						LocalizationEx.GetUiString("blacklist_modal_new_local_domain_rule_text", Thread.CurrentThread.CurrentCulture), metroDialogSettings);
				if (result == null) return;
				var newRule = result.ToLower().Trim();
				var response = ParseBlacklist(newRule, true);
				if (response.Count() != 1) return;
				if (DomainBlacklist.LocalRules.FirstOrDefault(l => l.Rule.Equals(newRule)) != null) return;
				DomainBlacklist.LocalRules.Add(new LocaleRule {Rule = newRule});
				SaveDomainBlacklist();
			}
			catch (Exception)
			{
			}
		}

		public void AddRemoteDomainBlockRule()
		{
			try
			{
				var metroWindow = Application.Current.MainWindow as MetroWindow;
				var metroDialogSettings = new MetroDialogSettings
				{
					AffirmativeButtonText = LocalizationEx.GetUiString("ok", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture)
				};
				var result =
					metroWindow.ShowModalInputExternal(
						LocalizationEx.GetUiString("blacklist_modal_new_remote_domain_rule_header", Thread.CurrentThread.CurrentCulture),
						LocalizationEx.GetUiString("blacklist_modal_new_remote_domain_rule_text", Thread.CurrentThread.CurrentCulture), metroDialogSettings);
				if (result == null) return;
				var newRule = result.ToLower().Trim();
				if (!Uri.IsWellFormedUriString(newRule, UriKind.Absolute)) return;
				if (DomainBlacklist.RemoteRules.FirstOrDefault(l => l.Rule.Equals(newRule)) != null) return;
				DomainBlacklist.RemoteRules.Add(new RemoteRule {Rule = newRule});
				SaveDomainBlacklist();
			}
			catch (Exception)
			{
			}
		}

		public void RemoveSelectedDomainBlockRule()
		{
			if (SelectedDomainBlockRule == null) return;
			if (SelectedDomainBlockRule.GetType() == typeof(RemoteRule))
			{
				DomainBlacklist.RemoteRules.Remove((RemoteRule) SelectedDomainBlockRule);
			}
			else
			{
				DomainBlacklist.LocalRules.Remove((LocaleRule) SelectedDomainBlockRule);
			}
			SaveDomainBlacklist();
		}

		public void RemoveSelectedAddressBlockRule()
		{
			if (SelectedAddressBlockRule == null) return;
			if (SelectedAddressBlockRule is string)
			{
				AddressBlacklist.Rules.Remove((string) SelectedAddressBlockRule);
			}
			SaveAddressBlacklist();
		}


		public async void BuildDomainBlacklist()
		{
			try
			{
				if (IsWorking) return;
				IsWorking = true;
				DomainBlacklistPathInfoString = LocalizationEx.GetUiString("blacklist_building_list",
					Thread.CurrentThread.CurrentCulture);
				var domainBlacklist = await GenerateDomainBlacklist(DomainBlacklist).ConfigureAwait(false);
				if (domainBlacklist != null)
				{
					File.WriteAllLines(Path.Combine("data", Global.DomainBlacklistFile), domainBlacklist);
					DomainBlacklistPath = Path.Combine(Directory.GetCurrentDirectory(), "data", Global.DomainBlacklistFile);
					UpdateDomainBlacklistPathInfoString();
					if (DomainBlacklistPlugin)
					{
						SavePlugin();
					}
				}
				IsWorking = false;
			}
			catch (Exception exception)
			{
				IsWorking = false;
			}
		}

		public void BuildAddressBlacklist()
		{
			try
			{
				if (IsWorking) return;
				IsWorking = true;
				DomainBlacklistPathInfoString = LocalizationEx.GetUiString("blacklist_building_list",
					Thread.CurrentThread.CurrentCulture);
				var addressBlacklist = GenerateAddressBlacklist(AddressBlacklist);
				File.WriteAllLines(Path.Combine("data", Global.AddressBlacklistFile), addressBlacklist);
				AddressBlacklistPath = Path.Combine(Directory.GetCurrentDirectory(), "data", Global.AddressBlacklistFile);
				UpdateAddressBlacklistPathInfoString();
				if (AddressBlacklistPlugin)
				{
					SavePlugin();
				}
				IsWorking = false;
			}
			catch (Exception)
			{
				IsWorking = false;
			}
		}

		private static async Task<string> FetchRemoteList(string requestUri)
		{
			try
			{
				using (var client = new HttpClient())
				{
					var getDataTask = client.GetStringAsync(requestUri);
					return await getDataTask.ConfigureAwait(false);
				}
			}
			catch (Exception)
			{

			}
			return null;
		}


		public static List<string> GenerateAddressBlacklist(AddressBlacklist addressBlacklist)
		{
			var allNames = new List<string>();
			try
			{
				foreach (var rule in addressBlacklist.Rules)
				{
					if (!allNames.Contains(rule))
					{
						allNames.Add(rule);
					}
				}

				return allNames;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<List<string>> GenerateDomainBlacklist(DomainBlacklist domainBlacklist)
		{
			var allNames = new List<string>();
			try
			{
				var local = ParseBlacklist(domainBlacklist.LocalRules.Select(l => l.Rule).ToList(), true);
				allNames.AddUnique(local);

				foreach (var remoteRule in domainBlacklist.RemoteRules)
				{
					var r = await FetchRemoteList(remoteRule.Rule).ConfigureAwait(false);
					if (r != null)
					{
						var remote = ParseBlacklist(r);
						allNames.AddUnique(remote);
					}
				}

				return allNames;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static bool ValidateAddressInput(string input)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(input))
				{
					return false;
				}
				var regexList = new List<Regex>();
				var ipv6 = new Regex(@"^(?:[A-F0-9]{1,4}:){7}[A-F0-9]{1,4}$");
				var ipv4 = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
				var ipv4s =
					new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){1,3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|\*)$");
					//allow *
				regexList.Add(ipv4);
				regexList.Add(ipv4s);
				regexList.Add(ipv6);
				foreach (var regex in regexList)
				{
					var isMatching = regex.Match(input);
					if (isMatching.Success)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static IEnumerable<string> ParseBlacklist(IEnumerable<string> lines, bool trusted = false)
		{
			var names = new List<string>();
			var rxComment = new Regex(@"^(#|$)");
			var rxU = new Regex(@"^@*\|\|([a-z0-9.-]+[.][a-z]{2,})\^?(\$(popup|third-party))?$");
			var rxL = new Regex(@"^([a-z0-9.-]+[.][a-z]{2,})$");
			var rxH = new Regex(@"^[0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}\s+([a-z0-9.-]+[.][a-z]{2,})$");
			var rxMdl = new Regex(@"^""[^""]+"",""([a-z0-9.-]+[.][a-z]{2,})"",");
			var rxB = new Regex(@"^([a-z0-9.-]+[.][a-z]{2,}),.+,[0-9: /-]+,");
			var rxTrusted = new Regex(@"^([*a-z0-9.-]+)$");

			foreach (var line in lines)
			{
				var tmp = line.ToLower().Trim();
				var regexList = new List<Regex>();
				if (trusted)
				{
					regexList.Add(rxTrusted);
				}
				else
				{
					regexList.Add(rxU);
					regexList.Add(rxL);
					regexList.Add(rxH);
					regexList.Add(rxMdl);
					regexList.Add(rxB);
				}

				var isComment = rxComment.Match(tmp);
				if (isComment.Success)
				{
					continue;
				}

				foreach (var regex in regexList)
				{
					var isMatching = regex.Match(tmp);
					if (!isMatching.Success)
					{
						continue;
					}
					if (!names.Contains(isMatching.Groups[1].Value))
					{
						names.Add(isMatching.Groups[1].Value);
					}
				}
			}
			return names;
		}


		private static IEnumerable<string> ParseBlacklist(string blacklist, bool trusted = false)
		{
			var lines = blacklist.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
			return ParseBlacklist(lines, trusted);
		}
	}
}
