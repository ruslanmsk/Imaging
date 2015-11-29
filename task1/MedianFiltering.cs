using System;
using System.Linq;

namespace ImageReadCS.task1
{
    public class MedianFiltering
    {

        public static GrayscaleFloatImage MedianFilter(GrayscaleFloatImage image, int kernelSize)
        {
            var tmpImage = new GrayscaleFloatImage(image.Width, image.Height);
            var kernel = new float[kernelSize * kernelSize];
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    for (var j = 0; j < kernelSize; j++)
                        for (var i = 0; i < kernelSize; i++)
                        {
                            var dx = x + i - kernelSize / 2;
                            var dy = y + j - kernelSize / 2;
                            if (dx < 0)
                                dx = 0;
                            if (dy < 0)
                                dy = 0;
                            if (dx >= image.Width)
                                dx = image.Width - 1;
                            if (dy >= image.Height)
                                dy = image.Height - 1;
                            kernel[i + j * kernelSize] = image[dx, dy];
                        }
                    kernel = kernel.OrderBy(e => e).ToArray();
                    tmpImage[x, y] = kernel[kernel.Length / 2 + 1];
                }

            return tmpImage;
        }

        public static GrayscaleFloatImage Convolution(GrayscaleFloatImage image, float[] kernel, int kernelSize, int mode = 0)
        {
            var tmpImage = new GrayscaleFloatImage(image.Width, image.Height);
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    float sum = 0;
                    for (var j = 0; j < kernelSize; j++)
                        for (var i = 0; i < kernelSize; i++)
                        {
                            var dx = x + i - kernelSize / 2;
                            var dy = y + j - kernelSize / 2;
                            if (dx < 0)
                                dx = 0;
                            if (dy < 0)
                                dy = 0;
                            if (dx >= image.Width)
                                dx = image.Width - 1;
                            if (dy >= image.Height)
                                dy = image.Height - 1;
                            sum += image[dx, dy] * kernel[i + j * kernelSize];
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

    }
}