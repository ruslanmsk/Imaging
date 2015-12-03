using System;

namespace ImageReadCS.task1
{
    public static class Rotate
    {

        public static GrayscaleFloatImage ClockWiseRotate(GrayscaleFloatImage image, int angle)
        {
            GrayscaleFloatImage resultImage;
            switch (angle)
            {
                case 90:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Height - 1 - y, x] = image[x, y];
                        }
                    break;
                case 180:
                    resultImage = new GrayscaleFloatImage(image.Width, image.Height);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Width - 1 - x, image.Height - 1 - y] = image[x, y];
                        }
                    break;
                case 270:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[y, image.Width - 1 - x] = image[x, y];
                        }
                    break;
                default:
                    var cos = Math.Cos(angle * 0.0174533);
                    var sin = Math.Sin(angle * 0.0174533);
                    var width = (int)Math.Floor(cos * image.Height + sin * image.Width);
                    var height = (int)Math.Floor(sin * image.Height + cos * image.Width);
                    resultImage = new GrayscaleFloatImage(width, height);
                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var b = sin * image.Height;
                            var hx = (x - b) * cos + y * sin;
                            var hy = -(x - b) * sin + y * cos;
                            float c = 0;
                            if (hx >= 1 && hy >= 1 && hx < image.Width - 1 && hy < image.Height - 1)
                            {
                                var x1 = (int)hx;
                                var y1 = (int)hy;
                                c = (float)((hy - y1) * ((x1 + 1 - hx) * image[x1, y1 + 1] + (hx - x1) * image[x1 + 1, y1 + 1]) +
                                                (y1 + 1 - hy) * ((x1 + 1 - hx) * image[x1, y1] + (hx - x1) * image[x1 + 1, y1]));
                            }
                            resultImage[x, y] = c;
                        }
                    }
                    break;
            }
            return resultImage;
        }

        public static GrayscaleFloatImage CounterClockWiseRotate(GrayscaleFloatImage image, int angle)
        {
            GrayscaleFloatImage resultImage;
            switch (angle)
            {
                case 90:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[y, image.Width - 1 - x] = image[x, y];
                        }
                    break;
                case 180:
                    resultImage = new GrayscaleFloatImage(image.Width, image.Height);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Width - 1 - x, image.Height - 1 - y] = image[x, y];
                        }
                    break;
                case 270:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Height - 1 - y, x] = image[x, y];
                        }
                    break;
                default:
                    var cos = Math.Cos(angle * 0.0174533);
                    var sin = Math.Sin(angle * 0.0174533);
                    var width = (int)Math.Floor(cos * image.Width + sin * image.Height);
                    var height = (int)Math.Floor(sin * image.Width + cos * image.Height);
                    resultImage = new GrayscaleFloatImage(width, height);
                    for (var y = 0; y < height; y++)
                        for (var x = 0; x < width; x++)
                        {
                            var b = sin * image.Width;
                            var hx = (x) * cos - (y - b) * sin;
                            var hy = (x) * sin + (y - b) * cos;
                            float c = 0;
                            if (hx >= 1 && hy >= 1 && hx < image.Width - 1 && hy < image.Height - 1)
                            {
                                var x1 = (int)hx;
                                var y1 = (int)hy;
                                c = (float)((hy - y1) * ((x1 + 1 - hx) * image[x1, y1 + 1] + (hx - x1) * image[x1 + 1, y1 + 1]) +
                                                (y1 + 1 - hy) * ((x1 + 1 - hx) * image[x1, y1] + (hx - x1) * image[x1 + 1, y1]));
                            }
                            resultImage[x, y] = c;
                        }
                    break;
            }
            return resultImage;
        }
    }
}