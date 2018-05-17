using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp30Qrcode
{
    public class PostDataModel
    {
        public Stream PostSteam { set; get; }
        public string FileName { set; get; }
        public bool IsFile => PostSteam != null;

        public string Value { set; get; }
    }
    public class QRCodeService
    {
        public string GenCode(int n)
        {
            
            var accessTokenOrAppid = "9_K1DyzpFdLTDkvYXCU2hpEZviU7UKpQ5IV9dLaSKQPj3FNpgkjnVAaa-Dj2UUGxmgsyYdYWZcezyV70D-Ss7JozU6FgATJiJrbOy-BUYsK7D8DSoc0KH_ZFBwJL8Uy_7yBULL186xSMfFffguHRVeAKDXHL";
            var result=Senparc.Weixin.MP.AdvancedAPIs.QrCodeApi.Create(accessTokenOrAppid, 1000 * 60 * 60 * 4, n, Senparc.Weixin.MP.QrCode_ActionName.QR_SCENE);

            using (var file = File.AppendText("1.txt"))
            {
                file.WriteLineAsync(Environment.NewLine);
                file.WriteLineAsync(result.ticket);
            }

            return result.ticket;
            
        }

        /// <summary>
        /// 【异步方法】新增其他类型永久素材(图片（image）、语音（voice）和缩略图（thumb）)
        /// </summary>
        /// <param name="accessTokenOrAppId">AccessToken或AppId（推荐使用AppId，需要先注册）</param>
        /// <param name="file">文件路径</param>
        /// <param name="timeOut">代理请求超时时间（毫秒）</param>
        /// <returns></returns>
        public async Task<string> UploadForeverMediaAsync()
        {
            var bytes=await HttpClientHelper.Client.GetByteArrayAsync("https://imgcache.mysodao.com/img3/M0A/1B/8B/CgAPEFr7xhGJh4MVABn4Emb1Zhs397-70e6084f.PNG");

            string accessTokenOrAppId= "9_3bjh-Roo9zSu0SI3a--GRiAMC6BIGavVr-nEYdKJvdYEl7iZDfsN3-uVMrhERXI_KLu3K_5EI1LYNUPimt_w4Wwe1wcbec0jDZCj1QKkh79UvVwASrBlbJvywPfwfYPtD6_rwIWZNlfGN9tfUBUhAJDGNG";
            var url = string.Format($"https://api.weixin.qq.com/cgi-bin/material/add_material?access_token={accessTokenOrAppId}&type=");

            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(bytes, 0, bytes.Length);
                var dict = new Dictionary<string, PostDataModel>();
                dict.Add("media", new PostDataModel { FileName = $"素材{DateTime.Now.Ticks.ToString()}.png", PostSteam = ms });
                dict.Add("media2", new PostDataModel { Value="1231231" });
                string returnText = await HttpPostFileAsync(url, null, dict, null, timeOut: 10000);
                dict = null;
                return returnText;
            }
        }

        /// <summary>
        /// 使用Post方法上传文件，获取字符串结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="postDict"></param>
        /// <param name="refererUrl"></param>
        /// <param name="encoding"></param>
        /// <param name="cer"></param>
        /// <param name="timeOut"></param>
        /// <param name="checkValidationResult"></param>
        /// <returns></returns>
        public async Task<string> HttpPostFileAsync(string url, CookieContainer cookieContainer = null,Dictionary<string, PostDataModel> postDict=null, string refererUrl = null, Encoding encoding = null, X509Certificate2 cer = null, int timeOut =10000, bool checkValidationResult = false)
        {
            if (postDict == null)
                    throw new ArgumentNullException("postDict不能为空");

            var formUploadFile = postDict.Values.FirstOrDefault(m => m.IsFile) != null;//是否用Form上传文件
                if (!formUploadFile)
                    throw new Exception("必须有上传文件");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = timeOut;
                
            if (cer != null)
            {
                request.ClientCertificates.Add(cer);
            }

            var postStream = new MemoryStream();

            #region 处理Form表单文件上传

            foreach (var m in postDict)
            {
                //通过表单上传文件
                var isFile = m.Value.IsFile;

                string boundary = "----" + DateTime.Now.Ticks.ToString("x");

                string fileFormdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                string dataFormdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

                string formdata = null;

                if (isFile)
                {
                    //存在文件
                    formdata = string.Format(fileFormdataTemplate, m.Key, m.Value.FileName);
                }
                else
                {
                    //不存在文件或只是注释
                    formdata = string.Format(dataFormdataTemplate, m.Key, m.Value.Value);
                }

                // 统一处理
                var formdataBytes = Encoding.UTF8.GetBytes(postStream.Length == 0 ? formdata.Substring(2, formdata.Length - 2) : formdata);//第一行不需要换行
                await postStream.WriteAsync(formdataBytes, 0, formdataBytes.Length);

                if (isFile)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    try
                    {
                        m.Value.PostSteam.Seek(0, SeekOrigin.Begin);
                        m.Value.PostSteam.Position = 0;
                        while ((bytesRead = await m.Value.PostSteam.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await postStream.WriteAsync(buffer, 0, bytesRead);
                        }

                        //m.Value.PostSteam.Dispose();
                        //m.Value.PostSteam = null;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                //结尾
                var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                await postStream.WriteAsync(footer, 0, footer.Length);

                request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            }
            #endregion

            request.ContentLength = postStream != null ? postStream.Length : 0;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.KeepAlive = true;

            if (!string.IsNullOrEmpty(refererUrl))
            {
                request.Referer = refererUrl;
            }
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";

            if (cookieContainer != null)
            {
                request.CookieContainer = cookieContainer;
            }

            #region 输入二进制流
            if (postStream != null)
            {
                postStream.Position = 0;

                //直接写入流
                Stream requestStream = await request.GetRequestStreamAsync();

                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = await postStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await requestStream.WriteAsync(buffer, 0, bytesRead);
                }


                //debug
                //postStream.Seek(0, SeekOrigin.Begin);
                //StreamReader sr = new StreamReader(postStream);
                //var postStr = await sr.ReadToEndAsync();

                postStream.Close();//关闭文件访问
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.GetEncoding("utf-8")))
                {
                    string retString = await myStreamReader.ReadToEndAsync();
                    return retString;
                }
            }
        }
        
    }
}
