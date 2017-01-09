using System.Threading.Tasks;
using NUnit.Framework;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Tools;

namespace Tests
{
	[TestFixture]
    public class UpdateTests
    {
		[Test]
	    public async Task CheckForRemoteUpdateTest()
	    {
		    var remoteUpdate = await ApplicationUpdater.CheckForRemoteUpdateAsync();
			Assert.AreNotEqual(remoteUpdate, null);
			Assert.AreNotEqual(remoteUpdate.Update, null);
			Assert.AreNotEqual(remoteUpdate.Update.Installer, null);
			Assert.AreNotEqual(remoteUpdate.Update.Signature, null);
			Assert.AreEqual(remoteUpdate.Update.Publickey, Global.ApplicationUpdatePublicKey);
			Assert.AreEqual(remoteUpdate.Update.Installer.Name, "SimpleDNSCrypt.msi");
		}
    }
}
