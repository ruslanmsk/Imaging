namespace ImageReadCS.task1
{
    public class Reflection
    {
        public static void FlipHorizontal(GrayscaleFloatImage image)
        {
            for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width / 2; x++)
                {
                    var p = image[x, y];
                    image[x, y] = image[image.Width - 1 - x, y];
                    image[image.Width - 1 - x, y] = p;
                }
        }

        public static void FlipVertical(GrayscaleFloatImage image)
        {
            for (var y = 0; y < image.Height / 2; y++)
                for (var x = 0; x < image.Width; x++)
                {
                    var p = image[x, y];
                    image[x, y] = image[x, image.Height - 1 - y];
                    image[x, image.Height - 1 - y] = p;
                }
        }

    }
}