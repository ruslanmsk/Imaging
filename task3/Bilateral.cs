using System;

namespace ImageReadCS.task3
{
    public class Bilateral
    {
        public static GrayscaleFloatImage Process(GrayscaleFloatImage image, double sigmaD, double sigmaR)
        {
            var width = image.Width;
            var height = image.Height;
            var windowSize = 15;
            var result = image;
            windowSize = windowSize % 2 == 0 ? windowSize - 1 : windowSize;
            var N = (windowSize - 1) / 2;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    double ch = 0;
                    double zn = 0;
                    for (int j = 0; j < windowSize; j++)
                        for (int i = 0; i < windowSize; i++)
                        {
                            var w = Math.Exp((-(i - N) * (i - N) - (j - N) * (j - N)) / (2 * sigmaD * sigmaD) - (image[x, y] -
                                image[x + i - N < 0 ? 0 : x + i - N >= width ? width - 1 : x + i - N, y + j - N < 0 ? 0 : y + j - N >= height ? height - 1 : y + j - N]) * (image[x, y] -
                                image[x + i - N < 0 ? 0 : x + i - N >= width ? width - 1 : x + i - N, y + j - N < 0 ? 0 : y + j - N >= height ? height - 1 : y + j - N]) / (2 * sigmaR * sigmaR));
                            ch += image[x + i - N <= 0 ? 0 : x + i - N >= width ? width - 1 : x + i - N, y + j - N < 0 ? 0 : y + j - N >= height ? height - 1 : y + j - N] * w;
                            zn += w;
                        }
                    result[x, y] = (float)(ch / zn);
                }
            return result;
        }
    }
}