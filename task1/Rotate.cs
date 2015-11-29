namespace ImageReadCS.task1
{
    public static class Rotate
    {

        public static GrayscaleFloatImage ClockWiseRotate(GrayscaleFloatImage image, int angle)
        {
            GrayscaleFloatImage resultImage;
            switch (angle)
            {
                case 90:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for ( var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Height - 1 - y, x] = image[x, y];
                        }
                    break;
                case 180:
                    resultImage = new GrayscaleFloatImage(image.Width, image.Height);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Width - 1 - x, image.Height - 1 - y] = image[x, y];
                        }
                    break;
                case 270:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[y, image.Width - 1 - x] = image[x, y];
                        }
                    break;
                default:
                    return null;
            }
            return resultImage;
        }

        public static GrayscaleFloatImage CounterClockWiseRotate(GrayscaleFloatImage image, int angle)
        {
            GrayscaleFloatImage resultImage;
            switch (angle)
            {
                case 90:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[y, image.Width - 1 - x] = image[x, y];
                        }
                    break;
                case 180:
                    resultImage = new GrayscaleFloatImage(image.Width, image.Height);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Width - 1 - x, image.Height - 1 - y] = image[x, y];
                        }
                    break;
                case 270:
                    resultImage = new GrayscaleFloatImage(image.Height, image.Width);
                    for (var y = 0; y < image.Height; y++)
                        for (var x = 0; x < image.Width; x++)
                        {
                            resultImage[image.Height - 1 - y, x] = image[x, y];
                        }
                    break;
                default:
                    return null;
            }
            return resultImage;
        }
    }
}