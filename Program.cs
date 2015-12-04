using System;
using System.IO;
using ImageReadCS.task1;
using ImageReadCS.task2;
using ImageReadCS.task3;

namespace ImageReadCS
{
    class Program
    {
        static void Main()
        {
            //var readLine = Console.ReadLine();
            //if (readLine == null) return;
            //var args = readLine.Split(' ');
            var args = new[] {"lena.bmp", "test2.bmp", "canny", "2", "40", "120"};
            //task1
            //Инверсия значений пикселей изображения
            if (args.Length == 3 && args[2] == "invert")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                Inversion.InversionProcess(image);
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Отражение изображения по вертикали и по горизонтали
            if (args.Length == 4 && args[2] == "mirror")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                if (args[3] == "x") Reflection.FlipHorizontal(image);
                if (args[3] == "y") Reflection.FlipVertical(image);
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Поворот изображений по и против часовой стрелки на 90, 180 и 270 градусов(на произвольный угол)
            if (args.Length == 5 && args[2] == "rotate")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                if (args[3] == "cw") image = Rotate.ClockWiseRotate(image, angle: int.Parse(args[4]));
                if (args[3] == "ccw") image = Rotate.CounterClockWiseRotate(image, angle: int.Parse(args[4]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //фильтр Превитта
            if (args.Length == 4 && args[2] == "prewitt")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                if (args[3] == "x") image = MedianFiltering.Convolution(image, Filters.PrewittHorizontal(), 3, 2);
                if (args[3] == "y") image = MedianFiltering.Convolution(image, Filters.PrewittVertical(), 3, 2);
                ImageIO.ImageToFile(image, outputFileName);
            }
            //фильтр Собеля
            if (args.Length == 4 && args[2] == "sobel")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                if (args[3] == "x") image = MedianFiltering.Convolution(image, Filters.SobelHorizontal(), 3);
                if (args[3] == "y") image = MedianFiltering.Convolution(image, Filters.SobelVertical(), 3);
                ImageIO.ImageToFile(image, outputFileName);
            }
            //фильтр Робертса
            if (args.Length == 4 && args[2] == "roberts")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                if (args[3] == "1") image = MedianFiltering.Convolution(image, Filters.RobertsMainDiagonal(), 2, 2);
                if (args[3] == "2") image = MedianFiltering.Convolution(image, Filters.RobertsAdditionalDiagonal(), 2, 2);
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Медианная фильтрация с квадратным окном произвольного размера
            if (args.Length == 4 && args[2] == "median")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                image = MedianFiltering.MedianFilter(image, kernelSize: int.Parse(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Свёртка с фильтром Гаусса с произвольным выбором параметра — радиуса σ
            if (args.Length == 4 && args[2] == "gauss")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                var data = new double[image.rawdata.Length];
                var template = new double[image.rawdata.Length];
                for (var i = 0; i < image.rawdata.Length; i++)
                {
                    data[i] = Convert.ToDouble(image.rawdata[i]);
                }
                ConvolutionGauss.GaussProcess(data, image.Width, image.Height, sigma: double.Parse(args[3]), windowSize: 9, temp: template, dest: data);
                for (var i = 0; i < image.rawdata.Length; i++)
                {
                    image.rawdata[i] = Convert.ToSingle(data[i]);
                }
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Вычисление модуля градиента как корень из суммы квадратов свёрток с первой производной фильтра Гаусса по горизонтали и вертикали
            if (args.Length == 4 && args[2] == "gradient")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                ConvolutionGauss.GradientProcess(image.rawdata, image.Width, image.Height, double.Parse(args[3]), (int)(double.Parse(args[3]) * 6), image.rawdata);
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Фильтр Габора с произвольными параметрами
            if (args.Length == 8 && args[2] == "gabor")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                image = ConvolutionGauss.Gabor(image, Filters.Gabor((int)(6 * double.Parse(args[3])), double.Parse(args[3]), double.Parse(args[6]), double.Parse(args[5]), double.Parse(args[4]), double.Parse(args[6])), (int)(6 * double.Parse(args[3])), (int)(6 * double.Parse(args[3])), int.Parse(args[7]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Обнаружение сосудов на изображениях глазного дна с помощью фильтров Габора
            if (args.Length == 4 && args[2] == "vessels")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);
                image = ConvolutionGauss.Vessels(image, (int)(6 * Convert.ToDouble(args[3])), Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //task2
            //Увеличение изображений в вещественное число раз с помощью билинейной интерполяции
            if (args.Length == 4 && args[2] == "up_bilinear")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = ImageResolution.Bilinear(image, Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Увеличение изображений в вещественное число раз с помощью бикубической интерполяции
            if (args.Length == 4 && args[2] == "up_bicubic")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                var resultImage = ImageResolution.Bicubic(image, Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(resultImage, outputFileName);
            }
            //Понижение разрешения изображений в вещественное число раз
            if (args.Length == 4 && args[2] == "downsample")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = ImageResolution.DownBilinear(image, Convert.ToDouble(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Вычисление метрик сравнения изображений(MSE и PSNR, SSIM и MSSIM)
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
            //task3
            //Алгоритм детектирования границ Канни
            if (args.Length == 6 && args[2] == "canny")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = Canny.Process(image, Convert.ToSingle(args[3]), Convert.ToSingle(args[4]), Convert.ToSingle(args[5]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Алгоритм Харриса для детектирования углов
            if (args.Length == 4 && args[2] == "harris")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = Harris.Process(image, double.Parse(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }
            //Билатеральная фильтрация изображений
            if (args.Length == 5 && args[2] == "bilateral")
            {
                string inputFileName = args[0], outputFileName = args[1];
                if (!File.Exists(inputFileName))
                    return;
                var image = ImageIO.FileToGrayscaleFloatImage(inputFileName);

                image = Bilateral.Process(image, double.Parse(args[3]), double.Parse(args[3]));
                ImageIO.ImageToFile(image, outputFileName);
            }

        }
    }
}
