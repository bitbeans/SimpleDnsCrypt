using System;
using System.Diagnostics;
using System.Text;
using DNS.Client;
using DNS.Protocol;
using Helper;
using SimpleDnsCrypt.Models;
using Sodium;

namespace SimpleDnsCrypt.Tools
{
	public class Certificate
	{
		public byte[] MagicQuery { get; set; }
		public int Serial { get; set; }
		public DateTime TsBegin { get; set; }
		public DateTime TsEnd { get; set; }
		public bool Valid { get; set; }
	}

	public class DnsCryptProxyEntryExtra
	{
		public Certificate Certificate { get; set; }
		public bool Succeeded { get; set; }
		public long ResponseTime { get; set; }
	}

	public static class AnalyseProxy
	{
		public static DnsCryptProxyEntryExtra Analyse(DnsCryptProxyEntry dnsCryptProxyEntry)
		{
			var dnsCryptProxyEntryExtra = new DnsCryptProxyEntryExtra();
			try
			{
				var address = "";
				var port = 443;
				if (dnsCryptProxyEntry.ResolverAddress.Contains(":"))
				{
					var t = dnsCryptProxyEntry.ResolverAddress.Split(':');
					address = t[0];
					port = Convert.ToInt32(t[1]);
				}
				else
				{
					address = dnsCryptProxyEntry.ResolverAddress;
				}

				var providerKey = Utilities.HexToBinary(dnsCryptProxyEntry.ProviderPublicKey);
				var request = new ClientRequest(address, port);
				request.Questions.Add(new Question(Domain.FromString(dnsCryptProxyEntry.ProviderName), RecordType.TXT));
				request.RecursionDesired = true;
				var sw = Stopwatch.StartNew();
				var response = request.Resolve();
				sw.Stop();
				var data = response.AnswerRecords[0].Data;

				if (Encoding.ASCII.GetString(ArrayHelper.SubArray(data, 0, 9)).Equals("|DNSC\0\u0001\0\0"))
				{
					var certificate = ExtractCertificate(ArrayHelper.SubArray(data, 9), providerKey);
					if (certificate != null)
					{
						dnsCryptProxyEntryExtra.Certificate = certificate;
						if (certificate.Valid)
						{
							dnsCryptProxyEntryExtra.Succeeded = true;
						}
						else
						{
							dnsCryptProxyEntryExtra.Succeeded = false;
						}
					}
					else
					{
						dnsCryptProxyEntryExtra.Succeeded = false;
					}
				}
				dnsCryptProxyEntryExtra.ResponseTime = sw.ElapsedMilliseconds;
			}
			catch (Exception)
			{
				dnsCryptProxyEntryExtra.Succeeded = false;
			}
			return dnsCryptProxyEntryExtra;
		}

		private static Certificate ExtractCertificate(byte[] data, byte[] providerKey)
		{
			var certificate = new Certificate();
			if (data.Length != 116) return null;
			certificate.MagicQuery = ArrayHelper.SubArray(data, 96, 8);
			var serial = ArrayHelper.SubArray(data, 104, 4);
			var tsBegin = ArrayHelper.SubArray(data, 108, 4);
			var tsEnd = ArrayHelper.SubArray(data, 112, 4);

			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(serial);
				Array.Reverse(tsBegin);
				Array.Reverse(tsEnd);
			}
			certificate.Serial = BitConverter.ToInt32(serial, 0);
			certificate.TsBegin = UnixTimeStampToDateTime(BitConverter.ToInt32(tsBegin, 0));
			certificate.TsEnd = UnixTimeStampToDateTime(BitConverter.ToInt32(tsEnd, 0));

			try
			{
				var m = PublicKeyAuth.Verify(data, providerKey);
				certificate.Valid = true;
				return certificate;
			}
			catch (Exception)
			{
			}
			return null;
		}

		private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dateTime;
		}
	}
}
