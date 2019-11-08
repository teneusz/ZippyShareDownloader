using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using FileDownloader;
using Newtonsoft.Json;

namespace TenekDownloader.download.service
{
    public class Cache : IDownloadCache
    {
        private static readonly string CACHE_LOCATION = Application.StartupPath + "\\cache\\";

        public Cache()
        {
            if (!Directory.Exists(CACHE_LOCATION)) Directory.CreateDirectory(CACHE_LOCATION);
        }

        void IDownloadCache.Add(Uri uri, string path, WebHeaderCollection headers)
        {
            var cach = new CachObject { URI = uri.AbsoluteUri, PATH = path, HEADERS = headers.AllKeys };
            var jsonData = JsonConvert.SerializeObject(cach);
            var Path = Base64.Base64Encode(uri.AbsolutePath).Replace("\\", "");
            File.WriteAllText(CACHE_LOCATION + Path, jsonData);
        }

        string IDownloadCache.Get(Uri uri, WebHeaderCollection headers)
        {
            var Path = Base64.Base64Encode(uri.AbsolutePath).Replace("\\", "");
            var PATH = CACHE_LOCATION + Path;
            if (File.Exists(PATH))
            {
                var JsonEncoded = File.ReadAllText(PATH);
                var account = JsonConvert.DeserializeObject<CachObject>(JsonEncoded);
                return account.PATH;
            }

            return null;
        }

        void IDownloadCache.Invalidate(Uri uri)
        {
            var Path = Base64.Base64Encode(uri.AbsolutePath).Replace("\\", "");
            var PATH = CACHE_LOCATION + Path;
            if (File.Exists(PATH))
                File.Delete(PATH);
        }
    }
}