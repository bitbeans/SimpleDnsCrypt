﻿using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SimpleDnsCrypt.Tools
{
    public static class ApplicationUpdater
    {
        public static async Task<RemoteUpdate> CheckForRemoteUpdateAsync()
        {
            var remoteUpdate = new RemoteUpdate();
            try
            {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                remoteUpdate.CanUpdate = false;
                var remoteUpdateData = await DownloadRemoteUpdateFileAsync().ConfigureAwait(false);

                if (remoteUpdateData != null)
                {
                    using (var remoteUpdateDataStream = new MemoryStream(remoteUpdateData))
                    {
                        using (var remoteUpdateDataStreamReader = new StreamReader(remoteUpdateDataStream))
                        {
							var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
							remoteUpdate = deserializer.Deserialize<RemoteUpdate>(remoteUpdateDataStreamReader);
                        }
                    }
                }

                if (remoteUpdate != null)
                {
                    var status = remoteUpdate.Update.Version.CompareTo(currentVersion);
                    // The local version is newer as the remote version
                    if (status < 0)
                    {
                        remoteUpdate.CanUpdate = false;
                    }
                    //The local version is the same as the remote version
                    else if (status == 0)
                    {
                        remoteUpdate.CanUpdate = false;
                    }
                    else
                    {
                        // the remote version is newer as the local version
                        remoteUpdate.CanUpdate = true;
                    }
                }
            }
            catch (Exception)
            {
                remoteUpdate.CanUpdate = false;
            }

            return remoteUpdate;
        }

        private static async Task<byte[]> DownloadRemoteUpdateFileAsync()
        {
            using (var client = new HttpClient())
            {
                var getDataTask = client.GetByteArrayAsync(Global.ApplicationUpdateUri);
                return await getDataTask.ConfigureAwait(false);
            }
        }

        public static async Task<byte[]> DownloadRemoteInstallerAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var getDataTask = client.GetByteArrayAsync(uri);
                return await getDataTask.ConfigureAwait(false);
            }
        }

        public static async Task<string> DownloadRemoteSignatureAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var getDataTask = client.GetStringAsync(uri);
                var resolverList = await getDataTask.ConfigureAwait(false);
                return resolverList;
            }
        }
    }
}