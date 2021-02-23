using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using LazyCache;
using System.Drawing;
using System;

namespace Challenge
{
    public class SimpleJsonRequest
    {
        public static IAppCache cache = new CachingService();
        public SimpleJsonRequest()
        {
            //cache will be available for 2 minutes
            cache.DefaultCachePolicy.DefaultCacheDurationSeconds = 120;
        }

        public static async Task<string> GetImageAsync(string url)
        {
            var base64 = cache.Get<string>(url);

            if (!string.IsNullOrEmpty(base64))
            {
                return base64;
            }

            using (WebClient webClient = new WebClient())
            {
                byte[] dataArr = webClient.DownloadData(url);
                //save file to local
                base64 = Convert.ToBase64String(dataArr);
                cache.Add<string>(url, base64);
                return base64;
            }

        }

        public static async Task<string> GetAsync(string url)
        {

            //try get image from cache
            var text = cache.Get<string>(url);

            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                text = await reader.ReadToEndAsync();

                cache.Add<string>(url, text);

                return text;
            }
        }

        public static async Task<string> PostAsync(string url, Dictionary<string, string> data)
        {

            return await RequestAsync("POST", url, data);
        }

        public static async Task<string> PutAsync(string url, Dictionary<string, string> data)
        {
            return await RequestAsync("PUT", url, data);
        }

        public static async Task<string> PatchAsync(string url, Dictionary<string, string> data)
        {
            return await RequestAsync("PATCH", url, data);
        }

        public static async Task<string> DeleteAsync(string url, Dictionary<string, string> data)
        {
            return await RequestAsync("DELETE", url, data);
        }

        private static async Task<string> RequestAsync(string method, string url, Dictionary<string, string> data)
        {

            string dataString = JsonConvert.SerializeObject(data);
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataString);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = "application/json";
            request.Method = method;

            using (Stream requestBody = request.GetRequestStream())
            {
                await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        
    }
}