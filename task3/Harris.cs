using System;

namespace ImageReadCS.task3
{

    public class Harris
    {
        protected static int Width;            
        protected static int Height;             
        //Corners vector
        protected static int[] Corners;
        //K coef for harris response                     
        protected static double K;                                    
        protected static int CornerTreshold;
        protected static int NmSupressWindowSize;

        protected static float[] Gray;                

        protected static float[] GradX;           
        protected static float[] GradY;

        protected static int[] Mx;
        protected static int[] My;
        protected static int[] Mxy;

        protected int DxdyKernelWindowSize = 3;
        protected static int[] Dx = new int[9] { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        protected static int[] Dy = new int[9] { -1, -2, -1, 0, 0, 0, 1, 2, 1 };

        protected int GaussianKernelWindowSize = 5;
        protected static double[] GaussianKernel = new double[25] {
            (1 / (double)84), (2 / (double)84), (3 / (double)84), (2 / (double)84), (1 / (double)84),
            ((double)2 / 84), (5 / (double)84), (6 / (double)84), (5 / (double)84), (2 / (double)84),
            (3 / (double)84), (6 / (double)84), (8 / (double)84), (6 / (double)84), (3 / (double)84),
            (2 / (double)84), (5 / (double)84), (6 / (double)84), (5 / (double)84), (2 / (double)84),
            (1 / (double)84), (2 / (double)84), (3 / (double)84), (2 / (double)84), (1 / (double)84)
        };

        public Harris(GrayscaleFloatImage image, double sigma)
        {
            Width = image.Width;
            Height = image.Height;
            Corners = new int[Width * Height];
            K = 0.05;
            CornerTreshold = 9000000;
            NmSupressWindowSize = 11;

            Gray = new float[Width * Height];
            Gray = image.rawdata;
            GradX = new float[Width * Height];
            GradY = new float[Width * Height];
            Mx = new int[Width * Height];
            My = new int[Width * Height];
            Mxy = new int[Width * Height];
        }

        public static GrayscaleFloatImage Process(GrayscaleFloatImage image, double sigma)
        {
            var harris = new Harris(image, sigma);
            BuildGradient();            //compute image grayscale gradient
            BuildOrder2MomentMatrix();  //compute order 2 moment matrix;
            BuildHarrisResponse();      //compute harris reponse for Corners
            ApplyNonMaxSupression();    //apply non-maximum supression algorithm to eliminate the weak Corners
            var result = WriteResultImage(image);

            return result;
        }

        private static void BuildGradient()
        {
            for (var i = Width + 1; i < (Height * Width) - Width - 1; i++)
            {
                //avoid to exceed image left and right margins and loss of information
                var lineCoord = i % Width;
                if (lineCoord <= Width - 1 && lineCoord >= 1)
                {
                    GradX[i] = GradSpatialConvolution(Gray, i, Dx, 3);
                    GradY[i] = GradSpatialConvolution(Gray, i, Dy, 3);
                }
            }
        }

        private static void BuildOrder2MomentMatrix()
        {
            for (var i = (2 + 1) * Width + 2; i < (Height * Width) - (2 + 1) * Width - 2; i++)
            {
                //avoid to exceed image left and right margins and loss of information
                var lineCoord = i % Width;
                if (lineCoord <= Width - 2 - 1 && lineCoord >= 2 + 1)
                {
                    Mx[i] = MSpatialConvolution(GradX, GradX, i, GaussianKernel, 5);
                    My[i] = MSpatialConvolution(GradY, GradY, i, GaussianKernel, 5);
                    Mxy[i] = MSpatialConvolution(GradX, GradY, i, GaussianKernel, 5);
                }
            }
        }

        private static void BuildHarrisResponse()
        {
            for (var i = 0; i < Width * Height; i++)
            {
                Corners[i] = (int)((double)(Mx[i] * My[i]) - (Mxy[i] * Mxy[i]) - K * ((double)(Mx[i] + My[i]) * (Mx[i] + My[i])));

                //apply the treshold to diferentiate Corners pixels from edge pixels or from flat regions pixels
                if (Corners[i] < CornerTreshold)
                {
                    Corners[i] = 0;
                }

            }
        }

        private static void ApplyNonMaxSupression()
        {
            for (var i = (NmSupressWindowSize / 2 + 1) * Width + (NmSupressWindowSize / 2);
                i < (Height * Width) - (NmSupressWindowSize / 2 + 1) * Width - (NmSupressWindowSize / 2);
                i++)
            {
                var lineCoord = i % Width;
                if (lineCoord <= Width - (NmSupressWindowSize / 2 + 1) &&
                    lineCoord >= (NmSupressWindowSize / 2 + 1))
                {

                    if (Corners[i] != 0)
                    {
                        //verify i pixel suround based on NmSupressWindowSize
                        for (var kk = -(NmSupressWindowSize / 2); kk <= (NmSupressWindowSize / 2); kk++)
                        {
                            for (var ll = -(NmSupressWindowSize / 2); ll <= (NmSupressWindowSize / 2); ll++)
                            {
                                if (Corners[i + kk*Width + ll] <= Corners[i]) continue;
                                Corners[i] = 0;
                                break;
                            }
                        }

                    }
                }
            }
        }

        public static GrayscaleFloatImage WriteResultImage(GrayscaleFloatImage image)
        {
            for (var i = 2 * Width + 2; i < Width * Height - 2 * Width - 2; i++)
            {
                if ((byte)Corners[i] != 0)
                {
                    for (var ii = -2; ii <= 2; ii++)
                    {
                        for (var jj = -2; jj <= 2; jj++)
                        {
                            image.rawdata[(i + ii * Width + jj)] = 1;
                        }
                    }


                }
            }
            return image;
        }

        protected static int MSpatialConvolution(float[] Data1, float[] Data2, int k, double[] KernelFilter, int KernelSize)
        {
            if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));
            var output = 0;
            var l = 0;
            for (var i = -(KernelSize / 2); i <= (KernelSize / 2); i++)
            {
                for (var j = -(KernelSize / 2); j <= (KernelSize / 2); j++)
                {
                    output += (int)(Data1[k + i * Width + j] * Data2[k + i * Width + j] * KernelFilter[l]);
                    l++;
                }
            }

            return output;

        }

        protected static float GradSpatialConvolution(float[] Data, int k, int[] KernelFilter, int KernelSize)
        {
            if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));
            float output = 0;
            var l = 0;
            for (var i = -(KernelSize / 2); i <= (KernelSize / 2); i++)
            {
                for (var j = -(KernelSize / 2); j <= (KernelSize / 2); j++)
                {
                    output += Data[k + i * Width + j] * KernelFilter[l];
                    l++;
                }
            }

            return output;

        }
    }
}