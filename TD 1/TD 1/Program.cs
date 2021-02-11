using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TD_1
{
    internal class Program
    {
        #region Fait avec Hortense 
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
        
        #endregion

        static void Exo4()
        {
            byte[] myfile = File.ReadAllBytes("./Images/Test.bmp");
            byte[] newfile = new byte[myfile.Length];
            //myfile est un vecteur composé d'octets représentant les métadonnées et les données de l'image
           
            //Métadonnées du fichier
            Console.WriteLine("\n Header \n");
            for (int i = 0; i < 14; i++)
            {
                Console.Write(myfile[i] + " ");
                newfile[i] = myfile[i];
            }
                
            //Métadonnées de l'image
            Console.WriteLine("\n HEADER INFO \n");
            for (int i = 14; i< 54; i++)
            {
                Console.Write(myfile[i] + " ");
                newfile[i] = myfile[i];
            }
                

            //L'image elle-même
            Console.WriteLine("\n IMAGE \n");
            for (int i = 54; i < myfile.Length; i = i + 60)
            {
                for (int j = i; j < i + 60; j++)
                {
                    Console.Write(myfile[j] + " ");

                }
                Console.WriteLine();
            }
            //copie de l'image avec inversion des couleurs et rotation
            for(int i = 54, j=0; i< myfile.Length && j < myfile.Length - 54; i++,j++)
            {
                newfile[i] = (byte)Math.Abs(255 - myfile[myfile.Length - 1 - j]);
            }

            Console.WriteLine("\n IMAGE \n");
            for (int i = 54; i < newfile.Length; i = i + 60)
            {
                for (int j = i; j < i + 60; j++)
                {
                    Console.Write(newfile[j] + " ");

                }
                Console.WriteLine();
            }

            File.WriteAllBytes("./Images/Sortie.bmp", newfile);
        }

        public static void Main(string[] args)
        {
            while (true)
            {
                uint choice = 0;
                do
                {
                    Console.WriteLine("TD 1 - PSI\n");
                    Console.WriteLine("1) Exercice 1 : Rotation image\n" +
                                      "2) Exercice 2 : Rogner image\n" +
                                      "3) Exercice 3 : Inverser couleurs\n"+
                                      "4) Exercice 4 : Rotation et inversion couleurs sans librairie\n"+
                                      "CTRL-C : Quitter");
                    Console.Write("Choisissez un exercice > ");
                } while (!uint.TryParse(Console.ReadLine(), out choice) || choice == 0 || choice > 4);

                switch (choice)
                {
                    case 1:
                        Exo1();
                        break;
                    case 2:
                        Exo2();
                        break;
                    case 3:
                        Exo3();
                        break;
                    case 4:
                        Exo4();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}