using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp30Qrcode
{
    public class HttpClientHelper
    {
        private static System.Net.Http.HttpClient client=null;
        private static object objLock = new object();
        public static System.Net.Http.HttpClient Client
        {
            get
            {
                if (client == null)
                {
                    lock (objLock)
                    {
                        if (client==null)
                        {
                            client = new System.Net.Http.HttpClient();
                            client.Timeout = TimeSpan.FromSeconds(6);
                            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                            
                        }
                    }
                }
                return client;
            }
        }

        public async Task<string> GetStringAsync(string url)
        {
            return await client.GetStringAsync(url);
        }
    }
}
