using System;

namespace ImageReadCS.task3
{
    public class Bilateral
    {
        public static GrayscaleFloatImage Process(GrayscaleFloatImage Image, double SigmaD, double SigmaR)
        {
            var width = Image.Width;
            var height = Image.Height;
            var windowSize = 15;
            var result = Image;
            windowSize = windowSize % 2 == 0 ? windowSize - 1 : windowSize;
            var n = (windowSize - 1) / 2;
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    double ch = 0;
                    double zn = 0;
                    for (var j = 0; j < windowSize; j++)
                        for (var i = 0; i < windowSize; i++)
                        {
                            var w = Math.Exp((-(i - n) * (i - n) - (j - n) * (j - n)) / (2 * SigmaD * SigmaD) - (Image[x, y] -
                                Image[x + i - n < 0 ? 0 : x + i - n >= width ? width - 1 : x + i - n, y + j - n < 0 ? 0 : y + j - n >= height ? height - 1 : y + j - n]) * (Image[x, y] -
                                Image[x + i - n < 0 ? 0 : x + i - n >= width ? width - 1 : x + i - n, y + j - n < 0 ? 0 : y + j - n >= height ? height - 1 : y + j - n]) / (2 * SigmaR * SigmaR));
                            ch += Image[x + i - n <= 0 ? 0 : x + i - n >= width ? width - 1 : x + i - n, y + j - n < 0 ? 0 : y + j - n >= height ? height - 1 : y + j - n] * w;
                            zn += w;
                        }
                    result[x, y] = (float)(ch / zn);
                }
            return result;
        }
    }
}