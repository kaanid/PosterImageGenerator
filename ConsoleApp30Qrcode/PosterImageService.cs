using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using QRCoder;

namespace ConsoleApp30Qrcode
{
    public abstract class PosterImageService
    {
        public async Task<bool> CreateImageAndSave()
        {
            //var bytes = CreateImageTest();

            var bytes = await CreateImage("http://sports.163.com/photoview/00C90005/161627.html#p=DHTNTRQO00C90005NOS",
                "https://imgcache.mysodao.com/img3/M0A/1B/8B/CgAPEFr7xhGJh4MVABn4Emb1Zhs397-70e6084f.PNG",
                "https://imgcache.mysodao.com/img3/M02/1B/8C/CgAPEFr7xkSSdnVeAABImwMjIsc568-8f771e34.PNG",
                "http://thirdwx.qlogo.cn/mmopen/oQ7QIr12iawp04JwPpgsb7363TXh2Rib1BWHgD4IBtq0MYWV9DsYTibruP3icDevQa3icRB5X6aHXQdPCMP4CnGz60g/132",
                null);

            //var bytes = await CreateImage("",
            //    "http://localhost:8081/Activity/1.6.15/png/style/bg.jpg",
            //    "http://localhost:8081/Activity/1.6.15/png/style/border.png",
            //    "http://thirdwx.qlogo.cn/mmopen/vi_32/zP0hUKFyB2QTmkORAZ3B3PbkfOozYRyIsQu6tUVHiaB7Vfd0N6AFZqWGL9u1ibrpFx5EXzJMeqmZCWP4uTNG9Fjw/132",
            //    "https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket=gQEj8DwAAAAAAAAAAS5odHRwOi8vd2VpeGluLnFxLmNvbS9xLzAyeGFLbFVac0llbDMxMU1qeWhyMWwAAgRwhvpaAwQAjScA");

            string fileName = DateTime.Now.Second.ToString() + ".jpg";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (FileStream file = File.Create(fileName, 1024))
            {
                await file.WriteAsync(bytes, 0, bytes.Length);
            }
            return true;
        }

        public Byte[] CreateImageTest()
        {
            var w = 46;
            var h = 1184;

            Image bg = Image.FromFile("Style/bg.png");
            Image border = Image.FromFile("Style/border.png");
            //Image qr = Image.FromFile("Style/qr.png");
            Image qr = Image.FromFile("Style/showqrcode.jpg");//showqrcode.jpg

            var w1 = border.Width;
            var h1 = border.Height;

            //背景图
            Graphics g = Graphics.FromImage(bg);

            //边框
            Rectangle rect = new Rectangle(w, h, w1, h1);
            g.DrawImage(border, rect);

            //二维码
            //去白边
            Bitmap asd = new Bitmap(110, 110);
            Graphics g2 = Graphics.FromImage(asd);
            g2.DrawImage(qr, new Rectangle(0, 0, 110, 110), new Rectangle(30, 30, 370, 370), GraphicsUnit.Pixel);
            g2.Dispose();

            var w2 = qr.Width;
            var h2 = qr.Height;
            Rectangle rect2 = new Rectangle(w+9, h+9, 103, 103);
            g.DrawImage(asd, rect2);

            //头像
            var headerPic = GetHeaderPic(null);
            Rectangle rect4 = new Rectangle(w + 40, h + 40, 40, 40);
            g.DrawImage(headerPic, rect4);

            // 插值算法的质量 
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            //释放对象
            g.Dispose();

            MemoryStream ms = new MemoryStream();
            bg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            border.Dispose();
            qr.Dispose();
            bg.Dispose();
            headerPic.Dispose();

            var arrByte = ms.ToArray();
            ms.Dispose();


            return arrByte;
        }

        public async Task<Byte[]> CreateImage(string qrCodeUrl, string backgroupUrl, string borderUrl, string headerPicUrl=null, string qrCodePicUrl=null)
        {
            if (string.IsNullOrWhiteSpace(backgroupUrl) )
            {
                throw new ArgumentNullException("backgroupUrl is null ");
            }

            if (string.IsNullOrWhiteSpace(borderUrl))
            {
                throw new ArgumentNullException("borderUrl is null ");
            }

            if (string.IsNullOrWhiteSpace(qrCodeUrl)&&string.IsNullOrWhiteSpace(qrCodePicUrl))
            {
                throw new ArgumentNullException("qrCodeUrl or qrCodePicUrl is null ");
            }

            //背景图
            Image bg =await GetImage(backgroupUrl);
            if (bg == null)
                throw new ArgumentNullException("backgroupUrl:"+ backgroupUrl);
            
            //边框
            Image border = await GetImage(borderUrl);
            if (border == null)
                throw new ArgumentNullException("borderUrl:" + borderUrl);

            //二维码
            Image qrImg = null;
            if (string.IsNullOrWhiteSpace(qrCodeUrl))
            {
                using (var qrImgPic = await GetImage(qrCodePicUrl))
                {
                    if (qrImgPic == null)
                    {
                        throw new ArgumentNullException("qrCodePicUrl:" + qrCodePicUrl);
                    }
                    //去白边
                    qrImg = GetQrCodeWithOutBorder(qrImgPic);
                }
            }
            else
            {
                qrImg = GeneratorQrCode(qrCodeUrl);
            }
            if (qrImg == null)
                throw new ArgumentNullException($"qrCodeUrl:{qrCodeUrl} qrCodePicUrl:{qrCodePicUrl}");

            //头像
            Image headerPic = null;
            if (!string.IsNullOrWhiteSpace(headerPicUrl))
            {
                using (var stream = await GetStream(headerPicUrl))
                {
                    if (stream != null)
                    {
                        headerPic = GetHeaderPic(stream);
                    }
                }
            }

            var arrByte = GeneratorImage(bg, border, qrImg, headerPic);
            bg.Dispose();
            border.Dispose();
            qrImg.Dispose();
            headerPic?.Dispose();

            return arrByte;
        }

        private Bitmap GetQrCodeWithOutBorder(Image qr)
        {
            if(qr==null)
                qr = Image.FromFile("Style/showqrcode.jpg");

            //去白边
            Bitmap bitmap = new Bitmap(110, 110);
            using (Graphics g2 = Graphics.FromImage(bitmap))
            {
                g2.DrawImage(qr, new Rectangle(0, 0, 110, 110), new Rectangle(30, 30, 370, 370), GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        private Bitmap GetHeaderPic(Stream stream)
        {
            Image headerPic = null;
            if (stream == null)
            {
                //测试使用
                headerPic = Image.FromFile("Style/headerPic.jpg");
            }
            else
            {
                headerPic = Image.FromStream(stream);
            }
            var bmap = new Bitmap(40, 40);
            using (var g = Graphics.FromImage(bmap))
            {
                Rectangle rect4 = new Rectangle(0, 0, 132, 132);
                Rectangle rectImg = new Rectangle(0, 0, 40, 40);
                using (TextureBrush br = new TextureBrush(headerPic, System.Drawing.Drawing2D.WrapMode.Clamp, rect4))
                {
                    br.ScaleTransform(rectImg.Width / (float)rect4.Width, rectImg.Height / (float)rect4.Height);
                    //br.ScaleTransform(0.25f, 0.25f);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.FillEllipse(br, rectImg);
                    g.DrawEllipse(new Pen(Color.White,3), rectImg);
                }
                return bmap;
            }
        }

        private async Task<Image> GetImage(string url)
        {
            using (var ms = new MemoryStream())
            {
                var bytes = await HttpClientHelper.Client.GetByteArrayAsync(url);
                if(bytes==null||bytes.Length==0)
                {
                    return null;
                }

                await ms.WriteAsync(bytes, 0, bytes.Length);
                return Image.FromStream(ms);
            }
        }

        private async Task<Stream> GetStream(string url)
        {
            var ms = new MemoryStream();
            
            var bytes = await HttpClientHelper.Client.GetByteArrayAsync(url);
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            await ms.WriteAsync(bytes, 0, bytes.Length);
            return ms;
        }

        private Bitmap GeneratorQrCode(string url)
        {
            string foreground = "#000000";
            string background = "#FFFFFF";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.H))
                {
                    var qr = new QRCode(qrCodeData);
                    var img = qr.GetGraphic(20, foreground, background, false);
                    //BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                    return img;
                }
            }
        }

        protected abstract byte[] GeneratorImage(Image bg, Image border, Image qrImg, Image headerPic = null);
    }
}
