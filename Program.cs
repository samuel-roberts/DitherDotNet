
using System.Drawing;
using System.IO;

namespace Dither
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return 1;
            }

            string inputFilepath = args[0];
            string outputFilepath = (args.Length > 1) ? args[1] : Path.ChangeExtension(inputFilepath, "dithered.png");

            if (string.IsNullOrWhiteSpace(inputFilepath))
            {
                return 1;
            }

            if (string.IsNullOrWhiteSpace(outputFilepath))
            {
                return 1;
            }

            // Read
            using var input = (Bitmap)Image.FromFile(inputFilepath);
            using var output = new Bitmap(input);
            var buffer = new float[input.Width, input.Height];

            // Fill buffer
            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    var pixel = input.GetPixel(x, y);
                    float greyscale = (((77 * pixel.R) + (150 * pixel.G) + (29 * pixel.B) + 128) >> 8) / 255.0f;
                    buffer[x, y] = greyscale;
                }
            }

            // Dither
            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    float oldPixel = buffer[x, y];
                    float newPixel = (oldPixel > 0.5) ? 1.0f : 0.0f;
                    float error = oldPixel - newPixel;

                    if (x + 1 < input.Width)
                    {
                        buffer[x + 1, y] += (error * 7 / 16);
                    }

                    if (x > 0 && y + 1 < input.Height)
                    {
                        buffer[x - 1, y + 1] += (error * 3 / 16);
                    }

                    if (y + 1 < input.Height)
                    {
                        buffer[x, y + 1] += (error * 5 / 16);
                    }

                    if (x + 1 < input.Width && y + 1 < input.Height)
                    {
                        buffer[x + 1, y + 1] += (error * 1 / 16);
                    }

                    output.SetPixel(x, y, (newPixel == 0.0) ? Color.Black : Color.White);
                }
            }

            // Write
            output.Save(outputFilepath);

            return 0;
        }
    }
}
