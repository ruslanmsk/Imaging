using System;
using System.Linq;

namespace ImageReadCS.task1
{
    public class ConvolutionGauss
    {
        private static double FunctionGauss(double x, double sig)
        {
            return Math.Exp(-(x * x) / (2 * sig * sig)) / (sig * Math.Sqrt(2 * Math.PI));
        }


        public static int TurningPoint(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }


        public static void GaussProcess(double[] field, int width, int height, double sigma, int windowSize, double[] temp, double[] dest)
        {
            var n = (windowSize - 1) / 2;
            var arr = new double[windowSize];
            for (var i = 0; i < windowSize; i++)
                arr[i] = FunctionGauss(i - n, sigma);
            var summ = arr.Sum();

            for (var i = 0; i < windowSize; i++)
                arr[i] = arr[i] / summ;

            for (var j = 0; j < height; j++)
                for (var i = 0; i < width; i++)
                {
                    double r = 0;
                    for (var k = 0; k < windowSize; k++)
                        r += arr[k] * field[TurningPoint(i + k - n, 0, width - 1) + j * width];
                    temp[i + j * width] = r;
                }

            for (var j = 0; j < height; j++)
                for (var i = 0; i < width; i++)
                {
                    double r = 0;
                    for (var k = 0; k < windowSize; k++)
                        r += arr[k] * temp[TurningPoint(j + k - n, 0, height - 1) * width + i];
                    dest[i + j * width] = r;
                }
        }


    }
}