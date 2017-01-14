using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using minisign;
using Microsoft.VisualBasic.FileIO;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Tools
{
    public static class DnsCryptProxyListManager
    {
        internal static string ClearString(string s)
        {
            return s.Replace("\"", "").Trim();
        }

        public static async Task<bool> UpdateResolverListAsync()
        {
            try
            {
                var resolverList = await DownloadResolverListAsync().ConfigureAwait(false);
                var signature = await DownloadSignatureAsync().ConfigureAwait(false);
                if ((resolverList != null) && (signature != null))
                {
                    //TODO: add an overload to minisign-net
                    var s = signature.Split('\n');
                    var trimmedComment = s[2].Replace("trusted comment: ", "").Trim();
                    var trustedCommentBinary = Encoding.UTF8.GetBytes(trimmedComment);
                    var loadedSignature = Minisign.LoadSignature(Convert.FromBase64String(s[1]), trustedCommentBinary, Convert.FromBase64String(s[3]));
                    var publicKey = Minisign.LoadPublicKeyFromString(Global.PublicKey);
                    var valid = Minisign.ValidateSignature(resolverList, loadedSignature, publicKey);

                    if (valid)
                    {
	                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder));
		                File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
			                Global.DnsCryptProxyResolverListName), resolverList);
		                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
			                Global.DnsCryptProxySignatureFileName), signature);
                    }
                    return valid;
                }
                return false;
            }
            catch (Exception)
            {
                // As this application can manipulate the network
                // settings, there could be multiple problems.
                // But we still have our local backup file.
                return false;
            }
        }

        private static async Task<byte[]> DownloadResolverListAsync()
        {
	        try
	        {
		        using (var client = new HttpClient())
		        {
			        var getDataTask = client.GetByteArrayAsync(Global.ResolverUrl);
			        var resolverList = await getDataTask.ConfigureAwait(false);
			        return resolverList;
		        }
	        }
	        catch (HttpRequestException)
	        {
				using (var client = new HttpClient())
				{
					var getDataTask = client.GetByteArrayAsync(Global.ResolverBackupUrl);
					var resolverList = await getDataTask.ConfigureAwait(false);
					return resolverList;
				}
			}
        }

        private static async Task<string> DownloadSignatureAsync()
        {
	        try
	        {
		        using (var client = new HttpClient())
		        {
			        var getDataTask = client.GetStringAsync(Global.SignatureUrl);
			        var resolverList = await getDataTask.ConfigureAwait(false);
			        return resolverList;
		        }
	        }
			catch (HttpRequestException)
			{
				using (var client = new HttpClient())
				{
					var getDataTask = client.GetStringAsync(Global.SignatureBackupUrl);
					var resolverList = await getDataTask.ConfigureAwait(false);
					return resolverList;
				}
			}
		}

        public static List<DnsCryptProxyEntry> ReadProxyList(string proxyListFile, string proxyListSignature, bool filterIpv6 = true)
        {
            if (!File.Exists(proxyListFile)) return null;
            if (!File.Exists(proxyListSignature)) return null;

            var dnsCryptProxyList = new List<DnsCryptProxyEntry>();

		    var signature = Minisign.LoadSignatureFromFile(proxyListSignature);
		    var publicKey = Minisign.LoadPublicKeyFromString(Global.PublicKey);

		    // only load signed files!
	        if (Minisign.ValidateSignature(proxyListFile, signature, publicKey))
	        {
		        using (var parser = new TextFieldParser(proxyListFile) {HasFieldsEnclosedInQuotes = true})
		        {
			        parser.SetDelimiters(",");
			        while (!parser.EndOfData)
			        {
				        var s = parser.ReadFields();
				        var tmp = new DnsCryptProxyEntry
				        {
					        Name = ClearString(s[0]),
					        FullName = ClearString(s[1]),
					        Description = ClearString(s[2]),
					        Location = ClearString(s[3]),
					        Coordinates = ClearString(s[4]),
					        Url = ClearString(s[5]),
					        Version = s[6],
					        DnssecValidation = (s[7].Equals("yes")),
					        NoLogs = (s[8].Equals("yes")),
					        Namecoin = (s[9].Equals("yes")),
					        ResolverAddress = ClearString(s[10]),
					        ProviderName = ClearString(s[11]),
					        ProviderPublicKey = ClearString(s[12]),
					        ProviderPublicKeyTextRecord = ClearString(s[13]),
					        LocalPort = Global.PrimaryResolverPort //set the default port 
				        };
				        if (!tmp.Description.Equals("Description"))
				        {
					        if (filterIpv6)
					        {
						        if (!tmp.ResolverAddress.StartsWith("["))
						        {
							        dnsCryptProxyList.Add(tmp);
						        }
					        }
					        else
					        {
						        dnsCryptProxyList.Add(tmp);
					        }
				        }
			        }
		        }
	        }
	        return dnsCryptProxyList;
        }
    }
}