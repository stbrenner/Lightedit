using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Lightedit
{
    internal class Engine
    {
        public void Run()
        {
            DeleteOldFiles();
            string tifFile = FindImageFile();
            string jpgFile = ConvertToJpg(tifFile);
            OpenWithDefaultApp(jpgFile);
        }

        private string ConvertToJpg(string tifFile)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            using (EncoderParameters encoderParams = GetEncoderParams())
            {
                string pngFile = Path.ChangeExtension(tifFile, ".le.jpg");
                Image image = Bitmap.FromFile(tifFile);
                image.Save(pngFile, jpgEncoder, encoderParams);
                return pngFile;
            }
        }

        private static EncoderParameters GetEncoderParams()
        {
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            return myEncoderParameters;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void OpenWithDefaultApp(string imageFile)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(imageFile);
                process.Start();
            }
        }

        private string FindImageFile()
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetTempPath());
            FileInfo file = directory.GetFiles("*.tif")
                         .OrderByDescending(f => f.LastWriteTime)
                         .First();
            return file.FullName;
        }

        private void DeleteOldFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(Path.GetTempPath());
            FileInfo[] files = directory.GetFiles("*.le.jpg");
            foreach (var file in files)
            {
                if (file.CreationTime < DateTime.Now.AddDays(-1))
                file.Delete();
            }
        }
    }
}
