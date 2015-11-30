using System;
using ImageReadCS.task1;

namespace ImageReadCS.task2
{
    public class ImageResolution
    {
        public static GrayscaleFloatImage Bilinear(GrayscaleFloatImage image, double n)
        {
            var width = (int)(image.Width * n);
            var height = (int)(image.Height * n);
            var result = new GrayscaleFloatImage(width, height);
            //
            for (var y = 0; y < height; y++)
            {
                var dy = y / n;
                for (var x = 0; x < width; x++)
                {
                    var dx = x / n;
                    if ((Math.Abs(dx % 1) <= 0) && (Math.Abs(dy % 1) <= 0))
                    {
                        result[x, y] = image[(int)dx, (int)dy];
                    }
                    else
                    {
                        var x1 = (int)dx;
                        var y1 = (int)dy;
                        result[x, y] = (float)((dy - y1) * ((x1 + 1 - dx) * image[x1, y1 + 1 >= image.Height ? image.Height - 1 : y1 + 1] +
                                                (dx - x1) * image[x1 + 1 >= image.Width ? image.Width - 1 : x1 + 1, y1 + 1 >= image.Height ? image.Height - 1 : y1 + 1]) +
                                                (y1 + 1 - dy) * ((x1 + 1 - dx) * image[x1, y1] +
                                                (dx - x1) * image[x1 + 1 >= image.Width ? image.Width - 1 : x1 + 1, y1]));
                    }
                }
            }
            return result;
        }

        public static GrayscaleFloatImage DownBilinear(GrayscaleFloatImage image, double n)
        {
            var width = (int)(image.Width / n);
            var height = (int)(image.Height / n);
            var result = new GrayscaleFloatImage(width, height);
            //
            var sigma = Math.Sqrt(n * n - 1);
            var data = new double[image.rawdata.Length];
            var template = new double[image.rawdata.Length];
            for (var i = 0; i < image.rawdata.Length; i++)
            {
                data[i] = Convert.ToDouble(image.rawdata[i]);
            }
            //
            ConvolutionGauss.GaussProcess(data, image.Width, image.Height, sigma: sigma, windowSize: (int)(sigma * 3), temp: template, dest: data);
            //
            for (var i = 0; i < image.rawdata.Length; i++)
            {
                image.rawdata[i] = Convert.ToSingle(data[i]);
            }
            for (var y = 0; y < height; y++)
            {
                var dy = y * n;
                for (var x = 0; x < width; x++)
                {
                    var dx = x * n;
                    if ((Math.Abs(dx % 1) <= 0) || (Math.Abs(dy % 1) <= 0))
                    {
                        result[x, y] = image[(int)dx, (int)dy];
                    }
                    else
                    {
                        var x1 = (int)dx;
                        var y1 = (int)dy;
                        result[x, y] = (float)((dy - y1) * ((x1 + 1 - dx) * image[x1, y1 + 1] + (dx - x1) * image[x1 + 1, y1 + 1]) +
                                        (y1 + 1 - dy) * ((x1 + 1 - dx) * image[x1, y1] + (dx - x1) * image[x1 + 1, y1]));
                    }
                }
            }
            return result;
        }

        public static GrayscaleFloatImage Bicubic(GrayscaleFloatImage image, double n)
        {
            int oldWidth = image.Width, oldHeight = image.Height;
            var newWidth = (int)Math.Round(image.Width * n);
            var newHeight = (int)Math.Round(image.Height * n);
            var result = new GrayscaleFloatImage(newWidth, newHeight);
            //
            var arr = new float[4];
            var temp = new float[4][];
            for (var i = 0; i < 4; i++)
                temp[i] = new float[4];
            //
            for (var y = 0; y < newHeight; y++)
                for (var x = 0; x < newWidth; x++)
                {

                    var x0 = (int)(x / n) - 1;
                    var y0 = (int)(y / n) - 1;
                    var x1 = (int)(x / n);
                    var y1 = (int)(y / n);
                    var x2 = (int)(x / n) + 1;
                    var y2 = (int)(y / n) + 1;
                    var x3 = (int)(x / n) + 2;
                    var y3 = (int)(y / n) + 2;
                    //
                    temp[0][0] = image[x0 < 0 ? 0 : x0 >= oldWidth ? oldWidth - 1 : x0, y0 < 0 ? 0 : y0 >= oldHeight ? oldHeight - 1 : y0];
                    temp[0][1] = image[x0 < 0 ? 0 : x0 >= oldWidth ? oldWidth - 1 : x0, y1 < 0 ? 0 : y1 >= oldHeight ? oldHeight - 1 : y1];
                    temp[0][2] = image[x0 < 0 ? 0 : x0 >= oldWidth ? oldWidth - 1 : x0, y2 < 0 ? 0 : y2 >= oldHeight ? oldHeight - 1 : y2];
                    temp[0][3] = image[x0 < 0 ? 0 : x0 >= oldWidth ? oldWidth - 1 : x0, y3 < 0 ? 0 : y3 >= oldHeight ? oldHeight - 1 : y3];
                    temp[1][0] = image[x1 < 0 ? 0 : x1 >= oldWidth ? oldWidth - 1 : x1, y0 < 0 ? 0 : y0 >= oldHeight ? oldHeight - 1 : y0];
                    temp[1][1] = image[x1 < 0 ? 0 : x1 >= oldWidth ? oldWidth - 1 : x1, y1 < 0 ? 0 : y1 >= oldHeight ? oldHeight - 1 : y1];
                    temp[1][2] = image[x1 < 0 ? 0 : x1 >= oldWidth ? oldWidth - 1 : x1, y2 < 0 ? 0 : y2 >= oldHeight ? oldHeight - 1 : y2];
                    temp[1][3] = image[x1 < 0 ? 0 : x1 >= oldWidth ? oldWidth - 1 : x1, y3 < 0 ? 0 : y3 >= oldHeight ? oldHeight - 1 : y3];
                    temp[2][0] = image[x2 < 0 ? 0 : x2 >= oldWidth ? oldWidth - 1 : x2, y0 < 0 ? 0 : y0 >= oldHeight ? oldHeight - 1 : y0];
                    temp[2][1] = image[x2 < 0 ? 0 : x2 >= oldWidth ? oldWidth - 1 : x2, y1 < 0 ? 0 : y1 >= oldHeight ? oldHeight - 1 : y1];
                    temp[2][2] = image[x2 < 0 ? 0 : x2 >= oldWidth ? oldWidth - 1 : x2, y2 < 0 ? 0 : y2 >= oldHeight ? oldHeight - 1 : y2];
                    temp[2][3] = image[x2 < 0 ? 0 : x2 >= oldWidth ? oldWidth - 1 : x2, y3 < 0 ? 0 : y3 >= oldHeight ? oldHeight - 1 : y3];
                    temp[3][0] = image[x3 < 0 ? 0 : x3 >= oldWidth ? oldWidth - 1 : x3, y0 < 0 ? 0 : y0 >= oldHeight ? oldHeight - 1 : y0];
                    temp[3][1] = image[x3 < 0 ? 0 : x3 >= oldWidth ? oldWidth - 1 : x3, y1 < 0 ? 0 : y1 >= oldHeight ? oldHeight - 1 : y1];
                    temp[3][2] = image[x3 < 0 ? 0 : x3 >= oldWidth ? oldWidth - 1 : x3, y2 < 0 ? 0 : y2 >= oldHeight ? oldHeight - 1 : y2];
                    temp[3][3] = image[x3 < 0 ? 0 : x3 >= oldWidth ? oldWidth - 1 : x3, y3 < 0 ? 0 : y3 >= oldHeight ? oldHeight - 1 : y3];
                    //
                    arr[0] = (float)(temp[0][1] + 0.5 * (float)(y / n - y1) * (temp[0][2] - temp[0][0] + (float)(y / n - y1) * (2.0 * temp[0][0] - 5.0 * temp[0][1] + 4.0 * temp[0][2] - temp[0][3] + (float)(y / n - y1) * (3.0 * (temp[0][1] - temp[0][2]) + temp[0][3] - temp[0][0]))));
                    arr[1] = (float)(temp[1][1] + 0.5 * (float)(y / n - y1) * (temp[1][2] - temp[1][0] + (float)(y / n - y1) * (2.0 * temp[1][0] - 5.0 * temp[1][1] + 4.0 * temp[1][2] - temp[1][3] + (float)(y / n - y1) * (3.0 * (temp[1][1] - temp[1][2]) + temp[1][3] - temp[1][0]))));
                    arr[2] = (float)(temp[2][1] + 0.5 * (float)(y / n - y1) * (temp[2][2] - temp[2][0] + (float)(y / n - y1) * (2.0 * temp[2][0] - 5.0 * temp[2][1] + 4.0 * temp[2][2] - temp[2][3] + (float)(y / n - y1) * (3.0 * (temp[2][1] - temp[2][2]) + temp[2][3] - temp[2][0]))));
                    arr[3] = (float)(temp[3][1] + 0.5 * (float)(y / n - y1) * (temp[3][2] - temp[3][0] + (float)(y / n - y1) * (2.0 * temp[3][0] - 5.0 * temp[3][1] + 4.0 * temp[3][2] - temp[3][3] + (float)(y / n - y1) * (3.0 * (temp[3][1] - temp[3][2]) + temp[3][3] - temp[3][0]))));
                    result[x, y] = (float)(arr[1] + 0.5 * (float)(x / n - x1) * (arr[2] - arr[0] + (float)(x / n - x1) * (2.0 * arr[0] - 5.0 * arr[1] + 4.0 * arr[2] - arr[3] + (float)(x / n - x1) * (3.0 * (arr[1] - arr[2]) + arr[3] - arr[0]))));
                }
            return result;
        }
    }
}