using System;

namespace ImageReadCS.task3
{
    class Canny
    {
        public int Width, Height;
        public GrayscaleFloatImage image;
        public int[,] GreyImage;
        public float[,] GreyImage2;
        int[,] GaussianKernel;
        int KernelWeight;
        int KernelSize = 5;
        float Sigma = 1;   // for N=2 Sigma =0.85  N=5 Sigma =1, N=9 Sigma = 2    2*Sigma = (int)N/2
        //Canny Edge Detection Parameters
        float MaxHysteresisThresh, MinHysteresisThresh;
        public float[,] DerivativeX;
        public float[,] DerivativeY;
        public float[,] FilteredImage;
        public float[,] Gradient;
        public float[,] NonMax;
        public int[,] PostHysteresis;
        int[,] EdgePoints;
        public float[,] GNH;
        public float[,] GNL;
        public int[,] EdgeMap;
        public int[,] VisitedMap;

        //new gradient
        public static GrayscaleFloatImage Grad(GrayscaleFloatImage image, int r, float sig)
        {
            var image_x = new GrayscaleFloatImage(image.Width, image.Height);
            var image_y = new GrayscaleFloatImage(image.Width, image.Height);
            var image_grad = new GrayscaleFloatImage(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    image_x[x, y] = image_y[x, y] = 0;

            float s = 0, koef = 1 / (float)Math.Sqrt(2 * Math.PI * sig * sig);
            for (int j = -r; j <= r; j++)
                for (int i = -r; i <= r; i++)
                    s += Math.Abs((float)(koef * (-2) * i / (sig * sig) * Math.Exp(-((i * i + j * j) / (sig * sig)))));

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    for (int j = -r; j <= r; j++)
                        for (int i = -r; i <= r; i++)
                        {
                            image_x[x, y] += (float)(koef * (-2) * i / (sig * sig) * Math.Exp(-((i * i + j * j) / (sig * sig)))) * image[x + i < 0 ? 0 : x + i >= image.Width ? image.Width - 1 : x + i, y + j < 0 ? 0 : y + j >= image.Height ? image.Height - 1 : y + j];
                        }
                    image_x[x, y] = image_x[x, y] / s;
                }
            //nonmax
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    for (int j = -r; j <= r; j++)
                        for (int i = -r; i <= r; i++)
                        {
                            image_y[x, y] += (float)(koef * (-2) * j / (sig * sig) * Math.Exp(-((i * i + j * j) / (sig * sig)))) * image[x + i < 0 ? 0 : x + i >= image.Width ? image.Width - 1 : x + i, y + j < 0 ? 0 : y + j >= image.Height ? image.Height - 1 : y + j];
                        }
                    image_y[x, y] = image_y[x, y] / s;
                }

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                    image_grad[x, y] = (float)Math.Sqrt(image_y[x, y] * image_y[x, y] + image_x[x, y] * image_x[x, y]);
            for (int i = 1; i < image.Width - 1; i++)
                for (int j = 1; j < image.Height - 1; j++)
                {
                    double th = Math.Round(Math.Atan2(image_x[i, j], image_y[i, j]) / (Math.PI / 4)) * (Math.PI / 4) - (Math.PI / 2);
                    int di = Math.Sign(Math.Cos(th));
                    int dj = -Math.Sign(Math.Sin(th));
                    if (image_grad[i + di, j + dj] <= image_grad[i, j])
                        image_grad[i + di, j + dj] = 0;
                    if (image_grad[i - di, j - dj] <= image_grad[i, j])
                        image_grad[i - di, j - dj] = 0;
                }
            return image_grad;
        }

        public static GrayscaleFloatImage Process(GrayscaleFloatImage image, float SigmaforGaussianKernel, float Th, float Tl)
        {
            var canny = new Canny(image, SigmaforGaussianKernel, Th, Tl);
            for (var i = 0; i < canny.Height; i++)
            {
                for (var j = 0; j < canny.Width; j++)
                {
                    image[j, i] = canny.GNH[j, i];
                }
            }
            var result = canny.image;
            return result;
        }

        public Canny(GrayscaleFloatImage image, float SigmaforGaussianKernel, float Th, float Tl)
        {

            // Gaussian and Canny Parameters
            MaxHysteresisThresh = Th;
            MinHysteresisThresh = Tl;
            Sigma = SigmaforGaussianKernel;
            this.image = image;
            Width = image.Width;
            Height = image.Height;

            EdgeMap = new int[Width, Height];
            VisitedMap = new int[Width, Height];

            GreyImage2 = new float[Width, Height];
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    GreyImage2[j, i] = image[j, i];
                }
            }

            DetectCannyEdges();
        }

        private void GenerateGaussianKernel(int N, float S, out int Weight)
        {

            float Sigma = S;
            float pi;
            pi = (float)Math.PI;
            int i, j;
            int SizeofKernel = N;

            float[,] Kernel = new float[N, N];
            GaussianKernel = new int[N, N];
            float[,] OP = new float[N, N];
            float D1, D2;


            D1 = 1 / (2 * pi * Sigma * Sigma);
            D2 = 2 * Sigma * Sigma;

            float min = 1000;

            for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
            {
                for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                {
                    Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = ((1 / D1) * (float)Math.Exp(-(i * i + j * j) / D2));
                    if (Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] < min)
                        min = Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];

                }
            }
            int mult = (int)(1 / min);
            int sum = 0;
            if ((min > 0) && (min < 1))
            {

                for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
                {
                    for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                    {
                        Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (float)Math.Round(Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] * mult, 0);
                        GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (int)Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                        sum = sum + GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                    }

                }

            }
            else
            {
                sum = 0;
                for (i = -SizeofKernel / 2; i <= SizeofKernel / 2; i++)
                {
                    for (j = -SizeofKernel / 2; j <= SizeofKernel / 2; j++)
                    {
                        Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (float)Math.Round(Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j], 0);
                        GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j] = (int)Kernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                        sum = sum + GaussianKernel[SizeofKernel / 2 + i, SizeofKernel / 2 + j];
                    }

                }

            }
            //Normalizing kernel Weight
            Weight = sum;

            return;
        }

        private float[,] GaussianFilter(float[,] Data)
        {
            GenerateGaussianKernel(KernelSize, Sigma, out KernelWeight);

            var Output = new float[Width, Height];
            int i, j, k, l;
            int Limit = KernelSize / 2;

            float Sum = 0;


            Output = Data; // Removes Unwanted Data Omission due to kernel bias while convolution


            for (i = Limit; i <= ((Width - 1) - Limit); i++)
            {
                for (j = Limit; j <= ((Height - 1) - Limit); j++)
                {
                    Sum = 0;
                    for (k = -Limit; k <= Limit; k++)
                    {

                        for (l = -Limit; l <= Limit; l++)
                        {
                            Sum = Sum + ((float)Data[i + k, j + l] * GaussianKernel[Limit + k, Limit + l]);

                        }
                    }
                    Output[i, j] = (float)(Math.Round(Sum / (float)KernelWeight));
                }

            }


            return Output;
        }

        private float[,] Differentiate(float[,] Data, int[,] Filter)
        {
            int i, j, k, l, Fh, Fw;

            Fw = Filter.GetLength(0);
            Fh = Filter.GetLength(1);
            float sum = 0;
            float[,] Output = new float[Width, Height];

            for (i = Fw / 2; i <= (Width - Fw / 2) - 1; i++)
            {
                for (j = Fh / 2; j <= (Height - Fh / 2) - 1; j++)
                {
                    sum = 0;
                    for (k = -Fw / 2; k <= Fw / 2; k++)
                    {
                        for (l = -Fh / 2; l <= Fh / 2; l++)
                        {
                            sum = sum + Data[i + k, j + l] * Filter[Fw / 2 + k, Fh / 2 + l];


                        }
                    }
                    Output[i, j] = sum;

                }

            }
            return Output;

        }

        private void DetectCannyEdges()
        {
            Gradient = new float[Width, Height];
            NonMax = new float[Width, Height];
            PostHysteresis = new int[Width, Height];

            DerivativeX = new float[Width, Height];
            DerivativeY = new float[Width, Height];

            //Gaussian Filter Input Image 
            ////test
            //var data = new double[image.rawdata.Length];
            //var template = new double[image.rawdata.Length];
            //for (var k = 0; k < image.rawdata.Length; k++)
            //{
            //    data[k] = Convert.ToDouble(image.rawdata[k]);
            //}
            //ConvolutionGauss.GaussProcess(data, image.Width, image.Height, 2, windowSize: 9, temp: template, dest: data);
            //for (var z = 0; z < image.rawdata.Length; z++)
            //{
            //    image.rawdata[z] = Convert.ToSingle(data[z]);
            //}
            //FilteredImage = new float[image.Width,image.Height];
            //for (var o = 0; o < Height; o++) 
            //{
            //    for (var h = 0; h < Width; h++)
            //    {
            //        FilteredImage[h, o] = image[h, o];
            //    }
            //}
            ////end test
            FilteredImage = GaussianFilter(GreyImage2);

            //Sobel Masks
            int[,] Dx = {{1,0,-1},
                         {1,0,-1},
                         {1,0,-1}};

            int[,] Dy = {{1,1,1},
                         {0,0,0},
                         {-1,-1,-1}};


            DerivativeX = Differentiate(FilteredImage, Dx);
            DerivativeY = Differentiate(FilteredImage, Dy);

            int i, j;

            //Compute the gradient magnitude based on derivatives in x and y:
            for (i = 0; i <= (Width - 1); i++)
            {
                for (j = 0; j <= (Height - 1); j++)
                {
                    Gradient[i, j] = (float)Math.Sqrt((DerivativeX[i, j] * DerivativeX[i, j]) + (DerivativeY[i, j] * DerivativeY[i, j]));

                }

            }
            // Perform Non maximum suppression:
            // NonMax = Gradient;



            int Limit = KernelSize / 2;
            int r, c;
            float Tangent;

            var ima = image;
            ima = Grad(ima, (int)Sigma * 3, Sigma);


            for (int y = 0; y < ima.Height; y++)
            {
                for (int x = 0; x < ima.Width; x++)
                {
                    NonMax[x, y] = ima[x, y];
                }
            }

            //PostHysteresis = NonMax;
            for (r = Limit; r <= (Width - Limit) - 1; r++)
            {
                for (c = Limit; c <= (Height - Limit) - 1; c++)
                {

                    PostHysteresis[r, c] = (int)NonMax[r, c];
                }

            }

            //Find Max and Min in Post Hysterisis
            float min, max;
            min = 100;
            max = 0;
            for (r = Limit; r <= (Width - Limit) - 1; r++)
                for (c = Limit; c <= (Height - Limit) - 1; c++)
                {
                    if (PostHysteresis[r, c] > max)
                    {
                        max = PostHysteresis[r, c];
                    }

                    if ((PostHysteresis[r, c] < min) && (PostHysteresis[r, c] > 0))
                    {
                        min = PostHysteresis[r, c];
                    }
                }

            GNH = new float[Width, Height];
            GNL = new float[Width, Height]; ;
            EdgePoints = new int[Width, Height];

            for (r = Limit; r <= (Width - Limit) - 1; r++)
            {
                for (c = Limit; c <= (Height - Limit) - 1; c++)
                {
                    if (PostHysteresis[r, c] >= MaxHysteresisThresh)
                    {

                        EdgePoints[r, c] = 1;
                        GNH[r, c] = 255;
                    }
                    if ((PostHysteresis[r, c] < MaxHysteresisThresh) && (PostHysteresis[r, c] >= MinHysteresisThresh))
                    {

                        EdgePoints[r, c] = 2;
                        GNL[r, c] = 255;

                    }

                }

            }

            HysterisisThresholding(EdgePoints);

            for (i = 0; i <= (Width - 1); i++)
                for (j = 0; j <= (Height - 1); j++)
                {
                    EdgeMap[i, j] = EdgeMap[i, j] * 255;
                }

            return;

        }

        private void HysterisisThresholding(int[,] Edges)
        {

            int i, j;
            int Limit = KernelSize / 2;


            for (i = Limit; i <= (Width - 1) - Limit; i++)
                for (j = Limit; j <= (Height - 1) - Limit; j++)
                {
                    if (Edges[i, j] == 1)
                    {
                        EdgeMap[i, j] = 1;

                    }

                }

            for (i = Limit; i <= (Width - 1) - Limit; i++)
            {
                for (j = Limit; j <= (Height - 1) - Limit; j++)
                {
                    if (Edges[i, j] == 1)
                    {
                        EdgeMap[i, j] = 1;
                        Travers(i, j);
                        VisitedMap[i, j] = 1;
                    }
                }
            }




            return;
        }

        private void Travers(int X, int Y)
        {


            if (VisitedMap[X, Y] == 1)
            {
                return;
            }

            //1
            if (EdgePoints[X + 1, Y] == 2)
            {
                EdgeMap[X + 1, Y] = 1;
                VisitedMap[X + 1, Y] = 1;
                Travers(X + 1, Y);
                return;
            }
            //2
            if (EdgePoints[X + 1, Y - 1] == 2)
            {
                EdgeMap[X + 1, Y - 1] = 1;
                VisitedMap[X + 1, Y - 1] = 1;
                Travers(X + 1, Y - 1);
                return;
            }

            //3

            if (EdgePoints[X, Y - 1] == 2)
            {
                EdgeMap[X, Y - 1] = 1;
                VisitedMap[X, Y - 1] = 1;
                Travers(X, Y - 1);
                return;
            }

            //4

            if (EdgePoints[X - 1, Y - 1] == 2)
            {
                EdgeMap[X - 1, Y - 1] = 1;
                VisitedMap[X - 1, Y - 1] = 1;
                Travers(X - 1, Y - 1);
                return;
            }
            //5
            if (EdgePoints[X - 1, Y] == 2)
            {
                EdgeMap[X - 1, Y] = 1;
                VisitedMap[X - 1, Y] = 1;
                Travers(X - 1, Y);
                return;
            }
            //6
            if (EdgePoints[X - 1, Y + 1] == 2)
            {
                EdgeMap[X - 1, Y + 1] = 1;
                VisitedMap[X - 1, Y + 1] = 1;
                Travers(X - 1, Y + 1);
                return;
            }
            //7
            if (EdgePoints[X, Y + 1] == 2)
            {
                EdgeMap[X, Y + 1] = 1;
                VisitedMap[X, Y + 1] = 1;
                Travers(X, Y + 1);
                return;
            }
            //8

            if (EdgePoints[X + 1, Y + 1] == 2)
            {
                EdgeMap[X + 1, Y + 1] = 1;
                VisitedMap[X + 1, Y + 1] = 1;
                Travers(X + 1, Y + 1);
                return;
            }


            //VisitedMap[X, Y] = 1;
            return;
        }
    }
}