using System;
using System.IO;
using ImageReadCS.task1;
using ImageReadCS.task2;

namespace ImageReadCS
{
    class Program
    {
        static void Main()
        {
            //var readLine = Console.ReadLine();
            //if (readLine == null) return;
            //var args = readLine.Split(' ');
            var args = new[] { "baboon.bmp", "test.bmp", "up_bicubic", "2" };

            if (args.Length == 4 && args[2] == "up_bilinear")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = ImageResolution.UpSampleBilinear(image, Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            if (args.Length == 4 && args[2] == "up_bicubic")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                var resultImage = ImageResolution.UpSampleBicubic(image, Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(resultImage, outputFileName);
            }
            if (args.Length == 4 && args[2] == "downsample")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = ImageResolution.DownSampleBilinear(image, Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            if (args.Length == 4 && args[2] == "metric")
            {
                string inputFileName = args[0], inputFileName2 = args[1];
                if (!File.Exists(inputFileName) || !File.Exists(inputFileName2))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                var image2 = ImageIO.FileToGrayscaleFloatImage(inputFileName2);
                double result;
                switch (args[3])
                {

                    case "mse":
                        result = Metrics.Mse(image, image2);
                        Console.WriteLine(result);
                        Console.ReadKey();
                        break;
                    case "psnr":
                        result = Metrics.Psnr(image, image2);
                        Console.WriteLine(result);
                        Console.ReadKey();
                        break;
                    case "ssim":
                        result = Metrics.Ssim(image.rawdata, image2.rawdata, image.Width, image.Height);
                        Console.WriteLine(result);
                        Console.ReadKey();
                        break;
                    case "mssim":
                        result = Metrics.Mssim(image.rawdata, image2.rawdata, image.Width, image.Height);
                        Console.WriteLine(result);
                        Console.ReadKey();
                        break;
                }
            }

            
        }
    }
}
