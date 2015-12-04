using System;

namespace ImageReadCS.task3
{
    class Canny
    {
        public int Width, Height;
        public GrayscaleFloatImage Image;
        public int[,] GreyImage;
        public float[,] GreyImage2;
        //Gaussian Kernel Data
        int[,] GaussianKernel;
        int KernelWeight;
        int KernelSize = 5;
        float Sigma = 1;   // for N=2 Sigma =0.85  N=5 Sigma =1, N=9 Sigma = 2    2*Sigma = (int)N/2
        //Canny Edge Detection Parameters
        readonly float _maxHysteresisThresh;
        readonly float _minHysteresisThresh;
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

        public  static GrayscaleFloatImage Process(GrayscaleFloatImage image, float SigmaforGaussianKernel, float Th, float Tl )
        {
            var canny = new Canny(image, SigmaforGaussianKernel, Th, Tl);
            for (var i = 0; i < canny.Height; i++)
            {
                for (var j = 0; j < canny.Width; j++)
                {
                    image[j, i] = canny.GNH[j, i];
                }
            }
            var result = canny.Image;
            return result;
        }

        public Canny(GrayscaleFloatImage image, float SigmaforGaussianKernel, float Th, float Tl)
        {

            // Gaussian and Canny Parameters
            _maxHysteresisThresh = Th;
            _minHysteresisThresh = Tl;
            //KernelSize = GaussianMaskSize;
            Sigma = SigmaforGaussianKernel;
            Image = image;
            Width = image.Width;
            Height = image.Height;

            EdgeMap = new int[Width, Height];
            VisitedMap = new int[Width, Height];

            GreyImage2 = new float[Width,Height];
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

            var Sigma = S;
            float pi;
            pi = (float)Math.PI;
            int i, j;
            var sizeofKernel = N;

            var kernel = new float[N, N];
            GaussianKernel = new int[N, N];


            var d1 = 1 / (2 * pi * Sigma * Sigma);
            var d2 = 2 * Sigma * Sigma;

            float min = 1000;

            for (i = -sizeofKernel / 2; i <= sizeofKernel / 2; i++)
            {
                for (j = -sizeofKernel / 2; j <= sizeofKernel / 2; j++)
                {
                    kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] = ((1 / d1) * (float)Math.Exp(-(i * i + j * j) / d2));
                    if (kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] < min)
                        min = kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j];

                }
            }
            var mult = (int)(1 / min);
            var sum = 0;
            if ((min > 0) && (min < 1))
            {

                for (i = -sizeofKernel / 2; i <= sizeofKernel / 2; i++)
                {
                    for (j = -sizeofKernel / 2; j <= sizeofKernel / 2; j++)
                    {
                        kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] = (float)Math.Round(kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] * mult, 0);
                        GaussianKernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] = (int)kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j];
                        sum = sum + GaussianKernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j];
                    }

                }

            }
            else
            {
                sum = 0;
                for (i = -sizeofKernel / 2; i <= sizeofKernel / 2; i++)
                {
                    for (j = -sizeofKernel / 2; j <= sizeofKernel / 2; j++)
                    {
                        kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] = (float)Math.Round(kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j], 0);
                        GaussianKernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j] = (int)kernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j];
                        sum = sum + GaussianKernel[sizeofKernel / 2 + i, sizeofKernel / 2 + j];
                    }

                }

            }
            //Normalizing kernel Weight
            Weight = sum;
        }

        private float[,] GaussianFilter(float[,] Data)
        {
            GenerateGaussianKernel(KernelSize, Sigma, out KernelWeight);

            int i;
            var limit = KernelSize / 2;


            var output = Data;


            for (i = limit; i <= ((Width - 1) - limit); i++)
            {
                int j;
                for (j = limit; j <= ((Height - 1) - limit); j++)
                {
                    float sum = 0;
                    int k;
                    for (k = -limit; k <= limit; k++)
                    {
                        int l;
                        for (l = -limit; l <= limit; l++)
                        {
                            sum = sum + (Data[i + k, j + l] * GaussianKernel[limit + k, limit + l]);

                        }
                    }
                    output[i, j] = (float)(Math.Round(sum / KernelWeight));
                }
            }


            return output;
        }

        private float[,] Differentiate(float[,] Data, int[,] Filter)
        {
            int i;

            var fw = Filter.GetLength(0);
            var fh = Filter.GetLength(1);
            var output = new float[Width, Height];

            for (i = fw / 2; i <= (Width - fw / 2) - 1; i++)
            {
                int j;
                for (j = fh / 2; j <= (Height - fh / 2) - 1; j++)
                {
                    float sum = 0;
                    int k;
                    for (k = -fw / 2; k <= fw / 2; k++)
                    {
                        int l;
                        for (l = -fh / 2; l <= fh / 2; l++)
                        {
                            sum = sum + Data[i + k, j + l] * Filter[fw / 2 + k, fh / 2 + l];


                        }
                    }
                    output[i, j] = sum;

                }
            }
            return output;

        }

        private void DetectCannyEdges()
        {
            Gradient = new float[Width, Height];
            NonMax = new float[Width, Height];
            PostHysteresis = new int[Width, Height];

            DerivativeX = new float[Width, Height];
            DerivativeY = new float[Width, Height];
            FilteredImage = GaussianFilter(GreyImage2);
            
            //Sobel Masks
            int[,] dx = {{1,0,-1},
                         {1,0,-1},
                         {1,0,-1}};

            int[,] dy = {{1,1,1},
                         {0,0,0},
                         {-1,-1,-1}};


            DerivativeX = Differentiate(FilteredImage, dx);
            DerivativeY = Differentiate(FilteredImage, dy);

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

            for (i = 0; i <= (Width - 1); i++)
            {
                for (j = 0; j <= (Height - 1); j++)
                {
                    NonMax[i, j] = Gradient[i, j];
                }
            }

            var limit = KernelSize / 2;
            int r, c;


            for (i = limit; i <= (Width - limit) - 1; i++)
            {
                for (j = limit; j <= (Height - limit) - 1; j++)
                {
                    float tangent;
                    if (DerivativeX[i, j] == 0)
                        tangent = 90F;
                    else
                        tangent = (float)(Math.Atan(DerivativeY[i, j] / DerivativeX[i, j]) * 180 / Math.PI); //rad to degree



                    //Horizontal Edge
                    if (((-22.5 < tangent) && (tangent <= 22.5)) || ((157.5 < tangent) && (tangent <= -157.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i, j + 1]) || (Gradient[i, j] < Gradient[i, j - 1]))
                            NonMax[i, j] = 0;
                    }


                    //Vertical Edge
                    if (((-112.5 < tangent) && (tangent <= -67.5)) || ((67.5 < tangent) && (tangent <= 112.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j]) || (Gradient[i, j] < Gradient[i - 1, j]))
                            NonMax[i, j] = 0;
                    }

                    //+45 Degree Edge
                    if (((-67.5 < tangent) && (tangent <= -22.5)) || ((112.5 < tangent) && (tangent <= 157.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j - 1]) || (Gradient[i, j] < Gradient[i - 1, j + 1]))
                            NonMax[i, j] = 0;
                    }

                    //-45 Degree Edge
                    if (((-157.5 < tangent) && (tangent <= -112.5)) || ((67.5 < tangent) && (tangent <= 22.5)))
                    {
                        if ((Gradient[i, j] < Gradient[i + 1, j + 1]) || (Gradient[i, j] < Gradient[i - 1, j - 1]))
                            NonMax[i, j] = 0;
                    }

                }
            }


            //PostHysteresis = NonMax;
            for (r = limit; r <= (Width - limit) - 1; r++)
            {
                for (c = limit; c <= (Height - limit) - 1; c++)
                {

                    PostHysteresis[r, c] = (int)NonMax[r, c];
                }

            }

            //Find Max and Min in Post Hysterisis
            float min, max;
            min = 100;
            max = 0;
            for (r = limit; r <= (Width - limit) - 1; r++)
                for (c = limit; c <= (Height - limit) - 1; c++)
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

            for (r = limit; r <= (Width - limit) - 1; r++)
            {
                for (c = limit; c <= (Height - limit) - 1; c++)
                {
                    if (PostHysteresis[r, c] >= _maxHysteresisThresh)
                    {

                        EdgePoints[r, c] = 1;
                        GNH[r, c] = 255;
                    }
                    if ((PostHysteresis[r, c] < _maxHysteresisThresh) && (PostHysteresis[r, c] >= _minHysteresisThresh))
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