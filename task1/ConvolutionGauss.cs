using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageReadCS.task1
{
    public class ConvolutionGauss
    {
        private static double FunctionGauss(double x, double sig)
        {
            return Math.Exp(-(x * x) / (2 * sig * sig)) / (sig * Math.Sqrt(2 * Math.PI));
        }

        public static float TurningPoint(float value, float min, float max)
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
                        r += arr[k] * field[(int)TurningPoint(i + k - n, 0, width - 1) + j * width];
                    temp[i + j * width] = r;
                }

            for (var j = 0; j < height; j++)
                for (var i = 0; i < width; i++)
                {
                    double r = 0;
                    for (var k = 0; k < windowSize; k++)
                        r += arr[k] * temp[(int)TurningPoint(j + k - n, 0, height - 1) * width + i];
                    dest[i + j * width] = r;
                }
        }

        public static void GradientProcess(float[] field, int width, int height, double sigma, int windowSize, float[] dest)
        {
            var horz = new float[field.Length];
            var vert = new float[field.Length];
            HorizontalConvolution(field, width, height, sigma, windowSize, horz);
            VerticalConvolution(field, width, height, sigma, windowSize, vert);
            for (var i = 0; i < field.Length; i++)
            {
                dest[i] = TurningPoint((float)Math.Sqrt(horz[i] * horz[i] + vert[i] * vert[i]), 0, 255);
            }
        }

        private static void HorizontalConvolution(IList<float> field, int width, int height, double sigma, int windowSize, IList<float> dest)
        {
            var n = (windowSize - 1) / 2;
            var arr = new double[windowSize];
            for (var i = 0; i < windowSize; i++)
                arr[i] = (n - i) * FunctionGauss(i - n, sigma);
            for (var j = 0; j < height; j++)
                for (var i = 0; i < width; i++)
                {
                    double r = 0;
                    for (var k = 0; k < windowSize; k++)
                        r += arr[k] * field[(int)TurningPoint(i + k - n, 0, width - 1) + j * width];
                    dest[i + j * width] = (float)r;
                }
        }

        private static void VerticalConvolution(IList<float> field, int width, int height, double sigma, int windowSize, IList<float> dest)
        {
            var n = (windowSize - 1) / 2;
            var arr = new double[windowSize];
            for (var i = 0; i < windowSize; i++)
                arr[i] = (i - n) * FunctionGauss(i - n, sigma);
            for (var j = 0; j < height; j++)
                for (var i = 0; i < width; i++)
                {
                    double r = 0;
                    for (var k = 0; k < windowSize; k++)
                        r += arr[k] * field[(int)TurningPoint(j + k - n, 0, height - 1) * width + i];
                    dest[i + j * width] = (float)r;
                }
        }

        public static GrayscaleFloatImage Gabor(GrayscaleFloatImage image, float[] kernel, int kernelWidth, int kernelHeight, int mode = 0)
        {
            var tmpImage = new GrayscaleFloatImage(image.Width, image.Height);
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    float sum = 0;
                    for (var j = 0; j < kernelHeight; j++)
                        for (var i = 0; i < kernelWidth; i++)
                        {
                            var dx = x + i - kernelWidth / 2;
                            var dy = y + j - kernelHeight / 2;
                            if (dx < 0)
                                dx = 0;
                            if (dy < 0)
                                dy = 0;
                            if (dx >= image.Width)
                                dx = image.Width - 1;
                            if (dy >= image.Height)
                                dy = image.Height - 1;
                            sum += image[dx, dy] * kernel[i + j * kernelWidth];
                        }
                    switch (mode)
                    {
                        case 1:
                            sum += 128;
                            break;
                        case 2:
                            sum = Math.Abs(sum);
                            break;
                    }
                    tmpImage[x, y] = sum;
                }

            return tmpImage;
        }

        public static GrayscaleFloatImage Vessels(GrayscaleFloatImage image, int size, double sigma)
        {
            var angle = 15;
            var images = new GrayscaleFloatImage[6];
            for (var i = 0; i < 6; i++)
            {
                images[i] = Gabor(image, Filters.Gabor(6, 2, 3, angle, 1, 0), 6, 6);
                angle += 30;
            }
            var result = new GrayscaleFloatImage(image.Width, image.Height);
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    result[x, y] = 0;
                    for (var i = 0; i < 6; i++)
                    {
                        if (images[i][x, y] <
                            result[x, y])
                            result[x, y] = images[i][x, y];
                    }
                    result[x, y] = Math.Abs(result[x, y]);
                }
            return result;
        }


    }
}