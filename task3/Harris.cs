using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageReadCS.task3
{
   
    public class Harris
    {
        //Harris corner detection algorithm global variables

        //image matrix is structured as an array in which are concatenated all image ROWS; 
        protected byte[] rgb;                 //image array 

        protected static int Width;              //image width
        protected static int Height;             //image width



        protected static int[] corners;                        //corners vector
        protected static double k;                                //K coef for harris response       
        protected static int corner_treshold;
        protected static int nm_supress_window_size;

        protected static int[] gray;                //gray scale image vector

        protected static int[] grad_x;               //gradient on x axe
        protected static int[] grad_y;               //gradient on y axe


        //order 2 moment matrix (structured tensor)
        // M = | Mx  Mxy |
        //     | Mxy My  |
        //
        protected static int[] Mx;
        protected static int[] My;
        protected static int[] Mxy;

        //soebel operators
        //Dx  = soebel operator for x axe
        //Dy  = soebel operator for y axe
        //               -1 0 1
        //  soebel x =   -2 0 2
        //               -1 0 1
        //
        //               -1 -2 -1
        //  soebel y =    0  0  0
        //                1  2  1
        protected int dxdy_kernel_window_size = 3; //Dx and Dy window size
        protected static int[] Dx = new int[9] { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        protected static int[] Dy = new int[9] { -1, -2, -1, 0, 0, 0, 1, 2, 1 };

        //gaussian kernel
        //                    | 1 2 3 2 1 |
        //                    | 2 5 6 5 2 |
        // gaussian kernel  = | 3 6 8 6 3 |\84
        //                    | 2 5 6 5 2 | 
        //                    | 1 2 3 2 1 |
        //
        protected int gaussian_kernel_window_size = 5; //Dx and Dy window size
        protected static double[] GaussianKernel = new double[25] {
            ((double)1 / (double)84), ((double)2 / (double)84), ((double)3 / (double)84), ((double)2 / (double)84), ((double)1 / (double)84),
            ((double)2 / (double)84), ((double)5 / (double)84), ((double)6 / (double)84), ((double)5 / (double)84), ((double)2 / (double)84),
            ((double)3 / (double)84), ((double)6 / (double)84), ((double)8 / (double)84), ((double)6 / (double)84), ((double)3 / (double)84),
            ((double)2 / (double)84), ((double)5 / (double)84), ((double)6 / (double)84), ((double)5 / (double)84), ((double)2 / (double)84),
            ((double)1 / (double)84), ((double)2 / (double)84), ((double)3 / (double)84), ((double)2 / (double)84), ((double)1 / (double)84)
        };




        //--------------------------------------
        //HarrisCornersDetectionBase constructor
        //--------------------------------------
        public Harris(GrayscaleFloatImage image, double sigma)
        {
            //this.rgb = getRGB(bmp);  //get rgb bits;
            Width = image.Width;
            Height = image.Height;
            corners = new int[Width * Height];
            k = 0.05;
            corner_treshold = 9000000;
            nm_supress_window_size = 11;

            gray = new int[Width * Height];;
            grad_x = new int[Width * Height];
            grad_y = new int[Width * Height];
            Mx = new int[Width * Height];
            My = new int[Width * Height];
            Mxy = new int[Width * Height];
        }

        public static GrayscaleFloatImage Process(GrayscaleFloatImage image, double sigma)
        {
            var harris = new Harris(image, sigma);
            //RGB2GrayScale();            //compute image grayscale
            BuildGradient();            //compute image grayscale gradient
            BuildOrder2MomentMatrix();  //compute order 2 moment matrix;
            BuildHarrisResponse();      //compute harris reponse for corners
            ApplyNONMaxSupression();    //apply non-maximum supression algorithm to eliminate the weak corners
            return image;
        }

        //private static void RGB2GrayScale()
        //{

        //    for (int i = 0; i < ImgWidth * ImgHeight; i++)
        //    {
        //        // k = i / ImgWidth; //col number
        //        //l = i % ImgWidth; //row number
        //        gray[i] = (30 * rgb[i * 3] + 59 * rgb[i * 3 + 1] + 11 * rgb[i * 3 + 2]) / 100;
        //    }
        //}

        private static void BuildGradient()
        {
            int line_coord = 0;

            for (int i = Width + 1; i < (Height * Width) - Width - 1; i++)
            {
                //avoid to exceed image left and right margins and loss of information
                line_coord = i % Width;
                if (line_coord <= Width - 1 && line_coord >= 1)
                {
                    grad_x[i] = GradSpatialConvolution(gray, i, Dx, 3);
                    grad_y[i] = GradSpatialConvolution(gray, i, Dy, 3);
                }

            }
        }

        private static void BuildOrder2MomentMatrix()
        {
            int line_coord = 0;
            for (int i = (2 + 1) * Width + 2; i < (Height * Width) - (2 + 1) * Width - 2; i++)
            {
                //avoid to exceed image left and right margins and loss of information
                line_coord = i % Width;
                if (line_coord <= Width - 2 - 1 && line_coord >= 2 + 1)
                {
                    Mx[i] = MSpatialConvolution(grad_x, grad_x, i, GaussianKernel, 5);
                    My[i] = MSpatialConvolution(grad_y, grad_y, i, GaussianKernel, 5);
                    Mxy[i] = MSpatialConvolution(grad_x, grad_y, i, GaussianKernel, 5);
                }
            }
        }

        private static void BuildHarrisResponse()
        {
            for (int i = 0; i < Width * Height; i++)
            {
                corners[i] = (int)((double)(Mx[i] * My[i]) - (Mxy[i] * Mxy[i]) - (double)k * ((double)(Mx[i] + My[i]) * (Mx[i] + My[i])));

                //apply the treshold to diferentiate corners pixels from edge pixels or from flat regions pixels
                if (corners[i] < corner_treshold)
                {
                    corners[i] = 0;

                }

            }
        }

        private static void ApplyNONMaxSupression()
        {
            int line_coord = 0;

            for (int i = (nm_supress_window_size/2 + 1)*Width + (nm_supress_window_size/2);
                i < (Height*Width) - (nm_supress_window_size/2 + 1)*Width - (nm_supress_window_size/2);
                i++)
            {
                line_coord = i%Width;
                if (line_coord <= Width - (nm_supress_window_size/2 + 1) &&
                    line_coord >= (nm_supress_window_size/2 + 1))
                {

                    if (corners[i] != 0)
                    {
                        //verify i pixel suround based on nm_supress_window_size
                        for (int kk = -(nm_supress_window_size/2); kk <= (nm_supress_window_size/2); kk++)
                        {
                            for (int ll = -(nm_supress_window_size/2); ll <= (nm_supress_window_size/2); ll++)
                            {
                                if (corners[i + kk*Width + ll] > corners[i])
                                {
                                    corners[i] = 0;
                                    break;
                                }
                            }
                        }

                    }
                }
            }
        }


        //---------------------
        //return corners vector
        //---------------------
        public int[] GetCorners()
        {
            return corners;
        }

        //-------------------------------------------------------------
        //get rgb values from a bitmap in a faster way
        //source: http://msdn.microsoft.com/en-us/library/5ey6h79d.aspx
        //-------------------------------------------------------------
        protected byte[] getRGB(Bitmap bmp)
        {

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            //image matrix is structured as an array in which are concatenated all image ROWS; 
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);
            return rgbValues;

        }

        //---------------------------------------------------
        //Methood: WriteResultImage(string file_path)
        //
        //Description:
        //  - write the image result
        //  - add corners to the original image and write it
        //
        //Parameters:
        //  - file_path =>  destination imame file
        //---------------------------------------------------
        public void WriteResultImage(string file_path)
        {

            int kk = 0;
            int ll = 0;

            //for (int i = 0; i < ImgHeight * ImgWidth; i++)
            //{
            //    rgb[i * 3] = (byte)corners[i];
            //    rgb[i * 3 + 1] = (byte)corners[i];
            //    rgb[i * 3 + 2] = (byte)corners[i];
            //}


            for (int i = 2 * Width + 2; i < Width * Height - 2 * Width - 2; i++)
            {
                kk = i / Width; //col number
                ll = i % Width; //row number
                //rgb[i * 3] = (byte)corners[ll * ImgHeight + kk];
                //rgb[i * 3 + 1] = (byte)corners[ll * ImgHeight + kk];
                //rgb[i * 3 + 2] = (byte)corners[ll * ImgHeight + kk];
                if ((byte)corners[i] != 0)
                {
                    for (int ii = -2; ii <= 2; ii++)
                    {
                        for (int jj = -2; jj <= 2; jj++)
                        {
                            rgb[(i + ii * Width + jj) * 3] = 255;
                            rgb[(i + ii * Width + jj) * 3 + 1] = 0;
                            rgb[(i + ii * Width + jj) * 3 + 2] = 0;
                        }
                    }


                }
            }

            System.IO.MemoryStream ms = new System.IO.MemoryStream(rgb);
            Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(rgb, 0, bmpData.Scan0, rgb.Length);
            bmp.UnlockBits(bmpData);
            bmp.Save(file_path);

        }


        //----------------------------------------------------------------------------------------------------------
        //Methood: int MSpatialConvolution(int[] data1, int[] data2, int k, double[] kernel_filter, int kernelSize)
        //
        //Description:
        //  - spatial convolution for 1 element of the order 2 moment matrix
        //
        //Parameters:
        //  - data1 , data2     =>  arrays which are multiplied element by element
        //  - k                 =>  order coordinate for the center pixel
        //  - kernerl_filter    =>  kernel filter for convolution
        //  - kernelsize        =>  dimension of the kernel (ex if kernel is 5x5 then kernelsize = 5)
        //----------------------------------------------------------------------------------------------------------
        protected static int MSpatialConvolution(int[] data1, int[] data2, int k, double[] kernel_filter, int kernelSize)
        {
            int output = 0;
            int l = 0;
            for (int i = -(kernelSize / 2); i <= (kernelSize / 2); i++)
            {
                for (int j = -(kernelSize / 2); j <= (kernelSize / 2); j++)
                {
                    output += (int)(data1[k + i * Width + j] * data2[k + i * Width + j] * kernel_filter[l]);
                    l++;
                }
            }

            return output;

        }

        //--------------------------------------------------------------------------------------------
        //Methood: int GradSpatialConvolution(int[] data, int k, int[] kernel_filter, int kernelSize)
        //
        //Description:
        //  - spatial convolution for 1 matrix element for gradient construction 
        //  - build spatial convolution of data[x]
        //-------------------------------------------------------------------------------------------
        protected static int GradSpatialConvolution(int[] data, int k, int[] kernel_filter, int kernelSize)
        {
            int output = 0;
            int l = 0;
            for (int i = -(kernelSize / 2); i <= (kernelSize / 2); i++)
            {
                for (int j = -(kernelSize / 2); j <= (kernelSize / 2); j++)
                {
                    output += data[k + i * Width + j] * kernel_filter[l];
                    l++;
                }
            }

            return output;

        }
    }
}