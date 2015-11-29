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
            double dx, dy;
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
            for (var y = 0; y < height - (int)n; y++)
            {
                dy = y * n;
                for (var x = 0; x < width - (int)n; x++)
                {
                    dx = x * n;
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

        public static GrayscaleFloatImage UpSampleBicubic(GrayscaleFloatImage image, double n)
        {
            var oldWidth = image.Width;
            var oldHeight = image.Height;
            var oldImage = new GrayscaleFloatImage(oldWidth, oldHeight);
            //Экстраполируем и перевернем массив для удобства
            //Линейно экстраполируем крайние строки и столбцы, для того чтобы можно было интерполировать крайние квадраты
            //В случае заполнения нулями - граница будет выглядеть слишком грубо
            //
            for (var i = 0; i < oldWidth; i++)
            {
                for (var j = 0; j < oldHeight; j++)
                {
                    oldImage[i, j] = image[j, i];
                    //экстраполируем влево
                    if (i == 0)
                        oldImage[0, j] = image[j, i] - image[j, i + 1];
                    //эксраполируем вправо
                    if (i == (oldWidth - 1))
                        oldImage[oldWidth - 1, j] = image[j, i] - image[j, i - 1];
                    //экстраполируем вверх
                    if (j == 0)
                        oldImage[i, 0] = image[j, i] - image[j + 1, i];
                    //экстраполируем вниз
                    if (j == (oldHeight - 1))
                        oldImage[i, oldHeight - 1] = image[j, i] - image[j - 1, i];
                }
            }

            //экстраполяция углов
            oldImage[0, 0] = (oldImage[0, 1] + oldImage[1, 0]) / 2;
            oldImage[oldWidth - 1, 0] = (oldImage[oldWidth - 1, 1] + oldImage[oldWidth - 2, 0]) / 2;
            oldImage[0, oldHeight - 1] = (oldImage[1, oldHeight - 1] + oldImage[0, oldHeight - 2]) / 2;
            oldImage[oldWidth - 1, oldHeight - 1] = (oldImage[oldWidth - 1, oldHeight - 2] + oldImage[oldWidth - 2, oldHeight - 1]) / 2;

            //шаг сетки n
            //создаем результирующее изображение
            var width = (int)(image.Width * n);
            var height = (int)(image.Height * n);
            var result = new GrayscaleFloatImage(width, height);

            var b2I = new CachedBicubicInterpolator();
            //обход старой сетки
            for (var i = 1; i < oldWidth -2 ; i++)
            {
                for (var j = 1; j < oldHeight - 2; j++)
                {
                    var points = new List<List<double>>
                    {
                        new List<double>(4),
                        new List<double>(4),
                        new List<double>(4),
                        new List<double>(4)
                    };
                    //
                    points[0].Add(oldImage[i - 1, j - 1]); 
                    points[0].Add(oldImage[i, j - 1]);
                    points[0].Add(oldImage[i + 1, j - 1]);
                    points[0].Add(oldImage[i + 2, j - 1]);
                    // 
                    points[1].Add(oldImage[i - 1, j]);
                    points[1].Add(oldImage[i, j]);
                    points[1].Add(oldImage[i + 1, j]);
                    points[1].Add(oldImage[i + 2, j]);
                    //
                    points[2].Add(oldImage[i - 1, j + 1]);
                    points[2].Add(oldImage[i, j + 1]);
                    points[2].Add(oldImage[i + 1, j + 1]);
                    points[2].Add(oldImage[i + 2, j + 1]);
                    //
                    points[3].Add(oldImage[i - 1, j + 2]);
                    points[3].Add(oldImage[i, j + 2]);
                    points[3].Add(oldImage[i + 1, j + 2]);
                    points[3].Add(oldImage[i + 2, j + 2]);
                    //
                    b2I.UpdateCoefficients(points);
                    //обход результирующего обращения
                    for (var x = 0; x < n; x++)
                    {
                        for (var y = 0; y < n; y++)
                        {
                            //х пикселя на выходном изображении
                            var rx = (i - 1) * n + x;
                            //y пикселя на выходном изображении
                            var ry = (j - 1) * n + y;

                            //x - x0
                            var ax = x / n;
                            //y - y0
                            var ay = y / n;
                            //Получим значение
                            var value = b2I.GetValue(ax, ay);
                            //Записываем значение
                            result[(int) rx, (int) ry] = (float)value;
                        }
                    }
                }
            }
            return result;
        }

        public static double[,] Transpose(double[,] matrix, int row, int column)
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