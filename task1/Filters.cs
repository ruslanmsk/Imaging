using System;

namespace ImageReadCS.task1
{
    public static class Filters
    {
        //фильтр Собеля
        public static float[] SobelVertical()
        {
            var result = new float[9];
            result[3] = result[4] = result[5] = 0;
            result[0] = result[2] = -1;
            result[6] = result[8] = 1;
            result[1] = -2;
            result[7] = 2;
            return result;
        }

        public static float[] SobelHorizontal()
        {
            var result = new float[9];
            result[1] = result[4] = result[7] = 0;
            result[0] = result[6] = -1;
            result[2] = result[8] = 1;
            result[3] = -2;
            result[5] = 2;
            return result;
        }
        //фильтр Превитта
        public static float[] PrewittVertical()
        {
            var result = new float[9];
            result[0] = result[1] = result[2] = -1;
            result[3] = result[4] = result[5] = 0;
            result[6] = result[7] = result[8] = 1;
            return result;
        }

        public static float[] PrewittHorizontal()
        {
            var result = new float[9];
            result[0] = result[3] = result[6] = -1;
            result[1] = result[4] = result[7] = 0;
            result[2] = result[5] = result[8] = 1;
            return result;
        }
        //фильтр Робертса
        public static float[] RobertsMainDiagonal()
        {
            var result = new float[4];
            result[1] = result[2] = 0;
            result[0] = 1;
            result[3] = -1;
            return result;
        }

        public static float[] RobertsAdditionalDiagonal()
        {
            var result = new float[4];
            result[0] = result[3] = 0;
            result[1] = 1;
            result[2] = -1; return result;
        }
        //габор
        private static double FuncGabor(double x, double y, double sigma, double lambda, double angle, double gamma, double phi)
        {
            angle = angle * 0.0174533;
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);
            return Math.Exp(-0.5 * (Math.Pow((x * cos + y * sin), 2) + Math.Pow((-x * sin + y * cos) * gamma, 2)) / Math.Pow(sigma, 2)) * Math.Cos(2 * Math.PI * lambda * (x * cos + y * sin) + phi);
        }

        public static float[] Gabor(int size, double sigma, double lambda, double angle, double gamma, double phi)
        {
            var result = new float[size * size];
            var n = (size - 1) / 2;
            for (var j = 0; j < size; j++)
                for (var i = 0; i < size; i++)
                    result[i + j * size] = (float)FuncGabor(i - n, j - n, sigma, lambda, angle, gamma, phi);
            return result;
        }
    }
}