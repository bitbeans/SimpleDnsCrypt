using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Tools;
using SimpleDnsCrypt.ViewModels;

namespace Tests
{
	[TestFixture]
    public class UpdateTests
	{
		private RemoteUpdate _remoteUpdate;

		[Test, Order(1)]
		public async Task CheckForRemoteUpdateTest()
	    {
			_remoteUpdate = await ApplicationUpdater.CheckForRemoteUpdateAsync();
			Assert.AreNotEqual(_remoteUpdate, null);
			Assert.AreNotEqual(_remoteUpdate.Update, null);
			Assert.AreNotEqual(_remoteUpdate.Update.Installer, null);
			Assert.AreNotEqual(_remoteUpdate.Update.Signature, null);
			Assert.AreEqual(_remoteUpdate.Update.Publickey, Global.ApplicationUpdatePublicKey);
			Assert.AreEqual(_remoteUpdate.Update.Installer.Name, "SimpleDNSCrypt.msi");
		}

		[Test, Order(2)]
		public async Task DownloadRemoteUpdateTest()
		{
			var updateViewModel = new UpdateViewModel();
			await updateViewModel.DownloadSignatureAsync(_remoteUpdate.Update.Signature).ConfigureAwait(false);
			Assert.AreEqual(updateViewModel.IsSignatureDownloaded, true);
			await updateViewModel.DownloadInstallerAsync(_remoteUpdate.Update.Installer).ConfigureAwait(false);
			Assert.AreEqual(updateViewModel.IsInstallerDownloaded, true);
		}

		[Test, Order(3)]
		public async Task DownloadResolverListTest()
		{
			var updated = await DnsCryptProxyListManager.UpdateResolverListAsync();
			Assert.AreEqual(updated, true);
			Console.WriteLine(@"updated: " + updated);
			var dnsCryptProxyResolverListFile = Path.Combine(TestContext.CurrentContext.TestDirectory, Global.DnsCryptProxyFolder, Global.DnsCryptProxyResolverListName);
			var dnsCryptProxySignatureFile = Path.Combine(TestContext.CurrentContext.TestDirectory, Global.DnsCryptProxyFolder, Global.DnsCryptProxySignatureFileName);
			Assert.AreEqual(File.Exists(dnsCryptProxyResolverListFile), true);
			Assert.AreEqual(File.Exists(dnsCryptProxySignatureFile), true);
			Console.WriteLine(@"dnsCryptProxyResolverListFile: " + dnsCryptProxyResolverListFile);
			Console.WriteLine(@"dnsCryptProxySignatureFile: " + dnsCryptProxySignatureFile);
			var resolverList = DnsCryptProxyListManager.ReadProxyList(dnsCryptProxyResolverListFile, dnsCryptProxySignatureFile, true);
			Assert.Greater(resolverList.Count, 0);
			Console.WriteLine(@"resolverList.Count: " + resolverList.Count);
		}
	}
}
