using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using ImageReadCS.task1;

namespace ImageReadCS.task2
{
    public class ImageResolution
    {
        public static GrayscaleFloatImage UpSampleBilinear(GrayscaleFloatImage image, double n)
        {
            var width = (int)(image.Width * n);
            var height = (int)(image.Height * n);
            var result = new GrayscaleFloatImage(width, height);
            for (var y = 0; y < height - (int)n; y++)
            {
                var dy = y / n;
                for (var x = 0; x < width - (int)n; x++)
                {
                    var dx = x / n;
                    if ((dx % 1 == 0) && (dy % 1 == 0))
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

        public static GrayscaleFloatImage DownSampleBilinear(GrayscaleFloatImage image, double n)
        {
            var width = (int)(image.Width / n);
            var height = (int)(image.Height / n);
            var result = new GrayscaleFloatImage(width, height);
            var sigma = Math.Sqrt(n * n - 1);
            var data = new double[image.rawdata.Length];
            var template = new double[image.rawdata.Length];
            for (var i = 0; i < image.rawdata.Length; i++)
            {
                data[i] = Convert.ToDouble(image.rawdata[i]);
            }
            ConvolutionGauss.GaussProcess(data, image.Width, image.Height, sigma: sigma, windowSize: (int)(sigma * 3), temp: template, dest: data);
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
                    if ((dx % 1 == 0) || (dy % 1 == 0))
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

        public static GrayscaleFloatImage UpSampleBicubic(GrayscaleFloatImage image, double n)
        {
            var width = (int)(image.Width * n);
            var height = (int)(image.Height * n);
            var result = new GrayscaleFloatImage(width, height);
            for (var y = 0; y < height - (int)n; y++)
            {
                var dy = y / n;
                for (var x = 0; x < width - (int)n; x++)
                {
                    var dx = x / n;
                    if ((dx % 1 == 0) && (dy % 1 == 0))
                    {
                        result[x, y] = image[(int)dx, (int)dy];
                    }
                    else
                    {
                        var x1 = (int)dx;
                        var y1 = (int)dy;
                        if (x1 <= 0) x1 = 1;
                        if (y1 <= 0) y1 = 1;
                        if (x1 >= image.Width - 2) x1--;
                        if (y1 >= image.Height - 2) y1--; //rewrite

                        result[x, y] =
                            (float)
                                ((x1 - dx)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1)*(dy - y1 - 1)*(dy - y1 - 2)/36*
                                 image[x1 - 1, y1 - 1] -
                                 (x1 - dx + 1)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1)*(dy - y1 - 1)*(dy - y1 - 2)/12*
                                 image[x1, y1 - 1] +
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 2)*(dy - y1)*(dy - y1 - 1)*(dy - y1 - 2)/12*
                                 image[x1 + 1, y1 - 1] -
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 1)*(dy - y1)*(dy - y1 - 1)*(dy - y1 - 2)/36*
                                 image[x1 + 2, y1 - 1] -
                                 (x1 - dx)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1 - 1)*(dy - y1 - 2)/12*
                                 image[x1 - 1, y1] +
                                 (x1 - dx + 1)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1 - 1)*(dy - y1 - 2)/4*
                                 image[x1, y1] -
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1 - 1)*(dy - y1 - 2)/4*
                                 image[x1 + 1, y1] +
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 1)*(dy - y1 + 1)*(dy - y1 - 1)*(dy - y1 - 2)/12*
                                 image[x1 + 2, y1] +
                                 (x1 - dx)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 2)/12*
                                 image[x1 - 1, y1 + 1] -
                                 (x1 - dx + 1)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 2)/4*
                                 image[x1, y1 + 1] +
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 2)/4*
                                 image[x1 + 1, y1 + 1] -
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 1)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 2)/12*
                                 image[x1 + 2, y1 + 1] -
                                 (x1 - dx)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 1)/36*
                                 image[x1 - 1, y1 + 2] +
                                 (x1 - dx + 1)*(x1 - dx - 1)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1) * (dy - y1 - 1) / 12 *
                                 image[x1, y1 + 2] -
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 2)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 1)/12*
                                 image[x1 + 1, y1 + 2] +
                                 (x1 - dx + 1)*(x1 - dx)*(x1 - dx - 1)*(dy - y1 + 1)*(dy - y1)*(dy - y1 - 1)/36*
                                 image[x1 + 2, y1 + 2]);

                    }
                }
            }
            return result;
        }

        public static
            double[,] Transpose(double[,] matrix, int row, int column)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            var result = new double[column, row];
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < column; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }
            return result;
        }

        public static double[,] Divide(double[,] matrix, int row, int column, int n)
        {
            for (var i = 0; i < row; i++)
            {
                for (var j = 0; j < column; j++)
                    matrix[i, j] = matrix[i, j] / n;
            }
            return matrix;
        }

        public static double[,] Multiply(double[,] a, int rowA, int columnA, double[,] b, int rowB, int columnB)
        {
            if (columnA != rowB)
            {
                Console.WriteLine("Ошибка размера матриц");
                return a;
            }
            var result = new double[rowA, columnB];
            for (var row = 0; row < rowA; row++)
            {
                for (var col = 0; col < columnB; col++)
                {
                    for (var inner = 0; inner < columnA; inner++)
                    {
                        result[row, col] += a[row, inner] * b[inner, col];
                    }
                }
            }
            return result;
        }

        public static double[,] Multiply(double[,] a, int rowA, int columnA, float[,] b, int rowB, int columnB)
        {
            var newB = new double[rowB, columnB];
            for (var i = 0; i < rowB; i++)
                for (var j = 0; j < columnB; j++)
                    newB[i, j] = Convert.ToDouble(b[i, j]);
            var result = Multiply(a, rowA, columnA, newB, rowB, columnB);
            return result;
        }
    }
}