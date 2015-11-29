namespace ImageReadCS.task1
{
    public static class Filters
    {
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

        public static float[] RobertsMainDiag()
        {
            var result = new float[4];
            result[1] = result[2] = 0;
            result[0] = 1;
            result[3] = -1;
            return result;
        }

        public static float[] RobertsAdditionalDiag()
        {
            var result = new float[4];
            result[0] = result[3] = 0;
            result[1] = 1;
            result[2] = -1; return result;
        }
    }
}