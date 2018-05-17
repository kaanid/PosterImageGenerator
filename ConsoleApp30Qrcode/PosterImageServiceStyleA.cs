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
    public class PosterImageServiceStyleA:PosterImageService
    {
        protected override byte[] GeneratorImage(Image bg,Image border,Image qrImg,Image headerPic=null)
        {
            if (bg == null)
                throw new ArgumentNullException("backgroupUrl");

            if (border == null)
                throw new ArgumentNullException("border");

            if (qrImg == null)
                throw new ArgumentNullException("qrImg");

            //生成图片
            var w = 46;
            var h = 1184;

            Bitmap img = new Bitmap(750, 1334);
            Graphics g = Graphics.FromImage(img);

            //背景图
            Rectangle rect0 = new Rectangle(0, 0, bg.Width, bg.Height);
            g.DrawImage(bg, rect0);

            //边框
            var w1 = border.Width;
            var h1 = border.Height;
            Rectangle rect = new Rectangle(w, h, w1, h1);
            g.DrawImage(border, rect);

            //二维码
            var w2 = qrImg.Width;
            var h2 = qrImg.Height;
            Rectangle rect2 = new Rectangle(w + 9, h + 9, 103, 103);
            g.DrawImage(qrImg, rect2);

            //头像
            if (headerPic != null)
            {
                Rectangle rect4 = new Rectangle(w + 36, h + 40, 50, 50);
                g.DrawImage(headerPic, rect4);

            }
            // 插值算法的质量 
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            //释放对象
            g.Dispose();

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}
