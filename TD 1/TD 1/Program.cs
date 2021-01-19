using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace TD_1
{
    internal class Program
    {
        static void Exo1()
        {
            Bitmap image = new Bitmap("fox.bmp");
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            image.Save("foxRotate.bmp");
            Process.Start("foxRotate.bmp");
        }
        
        static void Exo2()
        {
            Bitmap image = new Bitmap("fox.bmp");
            Rectangle rectangle = new Rectangle(48,29,10,20);
            Bitmap foxmini = image.Clone(rectangle,PixelFormat.DontCare);
            foxmini.Save("papatte.bmp");
            Process.Start("papatte.bmp");
        }

        static void Exo3()
        {
            Bitmap image = new Bitmap("fox.bmp");
            for(int i = 0; i< image.Height; i++)
            {
                for(int j = 0; j < image.Width; j++)
                {
                    Color color = image.GetPixel(j, i);
                    Color newColor = Color.FromArgb(255-color.R, 255-color.G, 255-color.B);
                    image.SetPixel(j, i, newColor);


                }
            }
            image.Save("CoulInverse.bmp");
            Process.Start("CoulInverse.bmp");
        }

        public static void Main(string[] args)
        {
            Exo3();
        }
    }
}