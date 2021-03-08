using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;


namespace TD_3
{
    class Program
    {
        static String[] imagesList = {"Test.bmp", "smoltriangle.bmp", "MARBLES.BMP", "Vd-Orig.bmp", "1.bmp", "lenna.bmp" };
        static void Main(string[] args)
        {
            while (true)
            {
                int imageIndex = 0;
                do
                {
                    Console.Write("Choisir une image :\n" +
                                  "-[1] Test.bmp\n" +
                                  "-[2] smoltriangle.bmp\n" +
                                  "-[3] MARBLES.BMP\n"+
                                  "-[4] Vd-Orig.bmp\n"+
                                  "-[5] 1.bmp\n" +
                                  "-[6] lenna.bmp\n"+
                                  "-[CTRL-C] Quitter\n" +
                                  "Numéro de l'image > ");
                } while (!int.TryParse(Console.ReadLine(), out imageIndex) || imageIndex <=0 || imageIndex > imagesList.Length+1 );

                String imagePath = imagesList[imageIndex - 1];
                MyImage myImage = new MyImage(imagePath);
                bool continuer = true;
                while (continuer)
                {
                    Console.WriteLine();
                    int cmdIndex = -1;
                    do
                    {
                        Console.Write("Choisir une action :\n" +
                                      "-[0] Afficher l'image d'origine\n"+
                                      "-[1] Noir et Blanc\n" +
                                      "-[2] Nuance de gris\n" +
                                      "-[3] Rotation (en radian)\n" +
                                      "-[4] Rotation (en degré)\n"+
                                      "-[5] Effet miroir\n"+
                                      "-[6] Redimensionner\n"+
                                      "-[7] Redimensionner (avec anti-aliasing)\n"+
                                      "-[8] Detection des bords\n"+
                                      "-[9] Flou Gaussien 3x3\n" +
                                      "-[10] Repoussage\n" +
                                      "-[11] Changer d'image\n" +
                                      "-[CTRL-C] Quitter\n" +
                                      "Numéro de l'action > ");
                    } while (!int.TryParse(Console.ReadLine(), out cmdIndex) || cmdIndex < 0 || cmdIndex > 11);

                    switch (cmdIndex)
                    {
                        case 0:
                            Process.Start(imagePath);
                            break;
                        case 1:
                            MyImage BW = myImage.ToBW();
                            BW.From_Image_To_File(@".\bw.bmp");
                            Process.Start(@".\bw.bmp");
                            break;
                        case 2:
                            MyImage grayscale = myImage.ToGrayscale();
                            grayscale.From_Image_To_File(@".\grayscale.bmp");
                            Process.Start(@".\grayscale.bmp");
                            break;
                        case 3:
                            double angleR = 0;

                            do
                            {
                                Console.Write("Saisissez l'angle de rotation (sens horaire et en radian) > ");
                            } while (!double.TryParse(Console.ReadLine(), out angleR));
                            MyImage rotateRadian = myImage.RotationV2(angleR);
                            rotateRadian.From_Image_To_File(@".\rotationRadian.bmp");
                            Process.Start(@".\rotationRadian.bmp");
                            break;
                        case 4:
                            double angle = 0;
                            
                            do
                            {
                                Console.Write("Saisissez l'angle de rotation (sens horaire et en degré) > ");
                            } while (!double.TryParse(Console.ReadLine(), out angle));
                            MyImage rotateDegre = myImage.RotationV2(angle, radian:false);
                            rotateDegre.From_Image_To_File(@".\rotationDegre.bmp");
                            Process.Start(@".\rotationDegre.bmp");
                            break;
                        case 5:
                            MyImage mirror = myImage.EffetMiroir();
                            mirror.From_Image_To_File(@".\mirror.bmp");
                            Process.Start(@".\mirror.bmp");
                            break;
                        case 6:
                            int nHeight = -1;
                            int nWidth = -1;
                            do
                            {
                                Console.Write("Saisissez la nouvelle hauteur > ");
                            } while (!int.TryParse(Console.ReadLine(), out nHeight) || nHeight < 0);
                            do
                            {
                                Console.Write("Saisissez la nouvelle largeur > ");
                            } while (!int.TryParse(Console.ReadLine(), out nWidth) || nWidth < 0);

                            MyImage resize = myImage.Resize(nWidth, nHeight);
                            resize.From_Image_To_File(@".\resize.bmp");
                            Process.Start(@".\resize.bmp");
                            break;
                        case 7:
                            int nHeight2 = -1;
                            int nWidth2 = -1;
                            do
                            {
                                Console.Write("Saisissez la nouvelle hauteur > ");
                            } while (!int.TryParse(Console.ReadLine(), out nHeight2) || nHeight2 < 0);
                            do
                            {
                                Console.Write("Saisissez la nouvelle largeur > ");
                            } while (!int.TryParse(Console.ReadLine(), out nWidth2) || nWidth2 < 0);

                            MyImage resize2 = myImage.ResizeAntialiasing(nWidth2, nHeight2);
                            resize2.From_Image_To_File(@".\resize2.bmp");
                            Process.Start(@".\resize2.bmp");
                            break;
                        case 8:
                            double[,] k2 = {{-1, -2, -1}, {0, 0, 0}, {1, 2, 1}};
                            double[,] kernel = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
                            MyImage grayscale2 = myImage.ToGrayscale();
                            MyImage bords = grayscale2.calculateKernel(kernel,k2);
                            bords.From_Image_To_File(@".\bords.bmp");
                            Process.Start(@".\bords.bmp");
                            break;
                        case 9:
                            double[,] k = myImage.MatriceKernel(2);
                            MyImage flou = myImage.calculateKernel(k);
                            flou.From_Image_To_File(@".\flou.bmp");
                            Process.Start(@".\flou.bmp");
                            break;
                        case 10:
                            double[,] kMasque = myImage.MatriceKernel(3);
                            MyImage masque = myImage.calculateKernel(kMasque);
                            masque.From_Image_To_File(@".\masque.bmp");
                            Process.Start(@".\masque.bmp");
                            break;
                        case 11:
                            continuer = false;
                            break;
                        default:
                            break;
                    }
                }
            }
        }


    }
}
