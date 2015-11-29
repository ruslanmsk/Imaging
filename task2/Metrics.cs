using System;
using System.Linq;

namespace ImageReadCS.task2
{
    public class Metrics
    {
        public static double Mse(GrayscaleFloatImage image1, GrayscaleFloatImage image2)
        {
            double sum = 0;
            var height = image1.Height;
            var width = image1.Width;
            for (var j = 0; j < height; j++)
                for (var i = 0; i < width; i++)
                {
                    sum += (image1.rawdata[i + j * width] - image2.rawdata[i + j * width]) * (image1.rawdata[i + j * width] - image2.rawdata[i + j * width]);
                }
            return sum / (height * width);
        }

        public static double Psnr(GrayscaleFloatImage image1, GrayscaleFloatImage image2)
        {
            var mse = Mse(image1, image2);
            if (Math.Abs(mse) <= 0) return 0;
            return 10 * Math.Log10(255 * 255 / mse);
        }

        public static double Ssim(float[] srcImage, float[] photo, int width, int height, int mode = 0)
        {
            var dataX = srcImage;
            var dataY = photo;
            var n = srcImage.Count();
            var mx = dataX.Sum() / n;
            var my = dataY.Sum() / n;

            var sigX = Math.Sqrt(1.0 / (n - 1) * dataX.Sum(e => Math.Pow(e - mx, 2)));
            var sigY = Math.Sqrt(1.0 / (n - 1) * dataY.Sum(e => Math.Pow(e - my, 2)));

            var cov = 1.0 / (n - 1) * dataX.Select((e, i) => (e - mx) * (dataY[i] - my)).Sum();
            const double C = 0.0001;
            var l = (2 * mx * my + C) / (mx * mx + my * my + C);
            var c = (2 * sigX * sigY + C) / (sigX * sigX + sigY * sigY + C);
            var s = (cov + C) / (sigX * sigY + C);
            return l * c * s;
        }

        public static double Mssim(float[] srcImage, float[] photo, int width, int height, int mode = 0)
        {
            const int blockSize = 10;
            const int blockPixCount = blockSize;
            var blockCountX = width / blockSize;
            var blockCountY = height / blockSize;
            const int N = blockPixCount * blockPixCount;
            var dataX = new double[N];
            var dataY = new double[N];
            var sum = 0.0;
            for (var k = 0; k < blockCountY; k++)
                for (var n = 0; n < blockCountX; n++)
                {
                    for (var j = 0; j < blockPixCount; j++)
                        for (var i = 0; i < blockPixCount; i++)
                        {
                            dataX[i + j * blockPixCount] = srcImage[i + n * blockCountX + (j + k) * width];
                            dataY[i + j * blockPixCount] = photo[i + n * blockCountX + (j + k) * width];
                        }
                    var mx = dataX.Sum() / N;
                    var my = dataY.Sum() / N;

                    var sigX = Math.Sqrt(1.0 / (N - 1) * dataX.Sum(e => Math.Pow(e - mx, 2)));
                    var sigY = Math.Sqrt(1.0 / (N - 1) * dataY.Sum(e => Math.Pow(e - my, 2)));

                    var cov = 1.0 / (N - 1) * dataX.Select((e, i) => (e - mx) * (dataY[i] - my)).Sum();
                    const double C = 0.0001;
                    var l = (2 * mx * my + C) / (mx * mx + my * my + C);
                    var c = (2 * sigX * sigY + C) / (sigX * sigX + sigY * sigY + C);
                    var s = (cov + C) / (sigX * sigY + C);
                    sum += l * c * s;
                }
            sum /= blockCountX * blockCountY;
            return sum;
        }
    }
}