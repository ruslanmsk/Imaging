using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageReadCS.task3
{
    class Canny
    {
        public static GrayscaleFloatImage ImageX, ImageY, ImageGrad, ImageGrad2;

        private int Width;
        public int Height;
        public GrayscaleFloatImage Image;
        public float Sigma = 1;
        //Canny Edge Detection Parameters
        public float MaxHysteresisThresh, MinHysteresisThresh;
        public int[] Dx = { 1, 0, -1, 0, 1, 1, -1, -1 };
        public int[] Dy = { 0, 1, 0, -1, 1, -1, 1, -1 };

        private static void NewGradient(GrayscaleFloatImage image, int r, float sig)
        {
            

            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                    ImageX[x, y] = ImageY[x, y] = 0;
            float s = 0, koef = 1 / (float)Math.Sqrt(2 * Math.PI * sig * sig);
            for (var j = -r; j <= r; j++)
                for (var i = -r; i <= r; i++)
                    s += Math.Abs((float)(koef * (-2) * i / (sig * sig) * Math.Exp(-((i * i + j * j) / (2*sig * sig)))));
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    for (var j = -r; j <= r; j++)
                        for (var i = -r; i <= r; i++)
                        {
                            ImageX[x, y] += (float)(koef * (-2) * i / (sig * sig) * Math.Exp(-((i * i + j * j) / (2*sig * sig)))) * image[x + i < 0 ? 0 : x + i >= image.Width ? image.Width - 1 : x + i, y + j < 0 ? 0 : y + j >= image.Height ? image.Height - 1 : y + j];
                        }
                    ImageX[x, y] = ImageX[x, y] / s;
                }
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    for (var j = -r; j <= r; j++)
                        for (var i = -r; i <= r; i++)
                        {
                            ImageY[x, y] += (float)(koef * (-2) * j / (sig * sig) * Math.Exp(-((i * i + j * j) / (2*sig * sig)))) * image[x + i < 0 ? 0 : x + i >= image.Width ? image.Width - 1 : x + i, y + j < 0 ? 0 : y + j >= image.Height ? image.Height - 1 : y + j];
                        }
                    ImageY[x, y] = ImageY[x, y] / s;
                }
            for (var x = 0; x < image.Width; x++)
                for (var y = 0; y < image.Height; y++)
                    ImageGrad[x, y] = (float)Math.Sqrt(ImageY[x, y] * ImageY[x, y] + ImageX[x, y] * ImageX[x, y]);
        }

        public Canny(GrayscaleFloatImage image, float sigma, float th, float tl)
        {
            Image = image;
            ImageX = new GrayscaleFloatImage(image.Width, image.Height);
            ImageY = new GrayscaleFloatImage(image.Width, image.Height);
            ImageGrad = new GrayscaleFloatImage(image.Width, image.Height);
            MaxHysteresisThresh = th;
            MinHysteresisThresh = tl;
            Sigma = sigma;
            NewGradient(Image, (int)(3 * Sigma), Sigma);
            ImageGrad2 = new GrayscaleFloatImage(ImageGrad.Width, ImageGrad.Height);
            for (var i = 1; i < Image.Width - 1; i++)
                for (var j = 1; j < Image.Height - 1; j++)
                {
                    var tang = Math.Round(Math.Atan2(ImageX[i, j], ImageY[i, j]) / (Math.PI / 4)) * (Math.PI / 4) - (Math.PI / 2);
                    var hi = Math.Sign(Math.Cos(tang));
                    var hj = -Math.Sign(Math.Sin(tang));
                    if (ImageGrad[i, j] < ImageGrad[i + hi, j + hj] || ImageGrad[i, j] < ImageGrad[i - hi, j - hj])
                    {
                        ImageGrad2[i, j] = 0;
                    }
                    else ImageGrad2[i, j] = ImageGrad[i, j];

                }
            ImageGrad = ImageGrad2;
            var max = ImageGrad.rawdata.Max();
            MinHysteresisThresh = max * MinHysteresisThresh;
            MaxHysteresisThresh = max * MaxHysteresisThresh;

            var stackX = new Stack<int>();
            var stackY = new Stack<int>();

            for (var i = 0; i < Image.Width; i++)
                for (var j = 0; j < Image.Height; j++)
                    if (ImageGrad[i, j] > MaxHysteresisThresh)
                    {
                        ImageGrad[i, j] = 255;
                        stackX.Push(i);
                        stackY.Push(j);
                    }
                    else
                        if (ImageGrad[i, j] > MinHysteresisThresh) ImageGrad[i, j] = 128;
                    else ImageGrad[i, j] = 0;

            while (stackX.Count != 0)
            {
                var x = stackX.Pop();
                var y = stackY.Pop();
                for (var i = 0; i < 8; i++)
                {
                    var nx = x + Dx[i];
                    var ny = y + Dy[i];
                    if ((nx < 0) || (ny < 0) || (nx >= Image.Width) || (ny >= Image.Height)) continue;
                    if (ImageGrad[nx, ny] != 128) continue;
                    ImageGrad[nx, ny] = 255;
                    stackX.Push(nx);
                    stackY.Push(ny);
                }
            }

            for (var i = 0; i < Image.Width; i++)
                for (var j = 0; j < Image.Height; j++)
                    if (ImageGrad[i, j] == 255) Image[i, j] = 255;
                    else Image[i, j] = 0;
        }


        public static GrayscaleFloatImage Process(GrayscaleFloatImage image, float sigma, float th, float tl)
        {
            var canny = new Canny(image, sigma, th, tl);
            var result = canny.Image;
            return result;
        }
    }
}