using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp30Qrcode
{
    class Program
    {
        static void Main(string[] args)
        {
            //var qrService = new QRCodeService();
            //var str = qrService.UploadForeverMediaAsync().Result;
            //Console.WriteLine("str:"+str);

            var service = new PosterImageServiceStyleA();
            service.CreateImageAndSave().Wait();
            Console.WriteLine("ok");

            Console.ReadLine();
        }
    }
}
