namespace ImageReadCS.task1
{
    public class Inversion
    {
        public static void InversionProcess(GrayscaleFloatImage image)
        {
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    image[x, y] = 255 - image[x, y];
                }
        }
    }
}