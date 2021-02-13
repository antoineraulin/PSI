using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;

namespace TD_2
{
    class MyImage
    {
        #region PRIVATE

        // définition des variables privées de l'objet
        private int size, offset, height, width, depth, realImageSize;

        /// <summary>
        /// matrice de pixels représentant l'image
        /// </summary>
        private Bgr[,] data;

        #endregion

        #region PUBLIC

        /// <summary>
        /// Conversion d'un tableau de bytes en ordre endian en un entier décimal
        /// </summary>
        /// <param name="tab">tableau de bytes en ordre endian</param>
        /// /// <param name="tailleTab">le nombre de bytes à considérer dans le tableau</param>
        /// <param name="posMin">position à partir de laquelle on débute la conversion dans le tableau</param>
        /// <returns>Renvoit l'entier décimal correspondant au tableau de bytes</returns>
        /// <exception cref="ArgumentNullException">Retourne une erreur si le tableau est nul</exception>
        /// <exception cref="ArgumentOutOfRangeException">Retourne une erreur si le tailleTab + posMin dépasse la taille du tableau</exception>
        /// <exception cref="ArgumentException">Retourne une erreur si les entiers fournit ne sont pas positifs</exception>
        public int Convertir_Endian_To_Int(byte[] tab, int tailleTab,  int posMin)
        {
            if (tab == null)
            {
                // le tableau de bytes ne doit pas être null pour qu'on puisse le convertir, donc si il l'est on renvoit une erreur.
                throw new ArgumentNullException(nameof(tab));
            }

            if (posMin + tailleTab > tab.Length - 1)
            {
                // on sort du tableau, donc on renvoit une erreur.
                throw new ArgumentOutOfRangeException(nameof(tailleTab));
            }

            if (tailleTab < 0 || posMin <= 0)
            {
                // on veut pas positions négatives => erreur.
                throw new ArgumentException("tailleTab doit être strictement positif et posMin doit être positif ou null");
            }
            int val = 0;
            for (int index = 0; index < tailleTab; index++)
            {
                val += tab[posMin + index] * (int)Math.Pow(256, index); //posMin correspond à la position du plus petit dans le file
            }

            return val;
        }

        /// <summary>
        /// Conversion d'un entier décimal en un tableau de bytes en ordre endian
        /// </summary>
        /// <param name="value">entier décimal à convertir</param>
        /// /// <param name="tailleTab">taille du tableau de sortie</param>
        /// <returns>Renvoit le tableau de bytes correspondant à l'entier décimal</returns>
        public byte[] Convertir_Int_To_Endian(int value, int tailleTab)
        {
            byte[] tab = new byte[tailleTab];
            int quotient;
            for (int index = 0; index < tailleTab; index++)
            {
                tab[index] = (byte) (value % 256);
                quotient = (value / 256);
                value = quotient;
            }
            return tab;
        }

        /// <summary>
        /// Constructeur de la classe MyImage, permet de convertir un fichier BMP en une instance de MyImage
        /// </summary>
        /// <param name="myfile">emplacement du BMP</param>
        /// <exception cref="ArgumentException">Retourne une erreur si le fichier fournit n'est pas un Bitmap</exception>
        public MyImage(string myfile)
        {
            byte[] file = File.ReadAllBytes(myfile);

            byte[] header = new byte[14];


            //Métadonnées du fichier
            for (int i = 0; i < 14; i++)
            {
                header[i] = file[i];
            }

            Console.WriteLine(header[0]);
            Console.WriteLine(header[1]);
            if (header[0] != 66 || header[1] != 77) //on vérifie que le fichier est bien un format bitmap
            {
                throw new ArgumentException("Le fichier fournit n'est pas un Bitmap");
            }

            Console.WriteLine("C'est un bitmap");

            size = Convertir_Endian_To_Int(file, 4, 2);

            offset = Convertir_Endian_To_Int(file, 4, 10);
            Console.WriteLine($"size : {size}");
            Console.WriteLine($"offset : {offset}");


            width = Convertir_Endian_To_Int(file, 4, 18);
            height = Convertir_Endian_To_Int(file, 4, 22);
            depth = Convertir_Endian_To_Int(file, 3, 28);
            realImageSize = Convertir_Endian_To_Int(file, 4, 34);


            Console.WriteLine($"width : {width}");
            Console.WriteLine($"height : {height}");
            Console.WriteLine($"depth : {depth}");
            Console.WriteLine($"realImageSize : {realImageSize}");


            // L'image elle-même
            
            Console.WriteLine("bytes image:");

            // on cherche l'entier divisible par 4 le plus proche du nombre de bytes dans la ligne
            int widthPlusPlus = 4 * (((width * (depth / 8)) + 4 / 2) / 4);
            
            data = new Bgr[height, widthPlusPlus/(depth / 8)];

            for (int i = offset; i < offset+realImageSize; i = i + widthPlusPlus)
            {
                for (int j = i; j < i + width * (depth / 8); j += depth / 8)
                {
                    // Console.Write(file[j] + " ");
                    // Console.Write(file[j + 1] + " ");
                    // Console.Write(file[j + 2] + " ");

                    if (depth / 8 == 3)
                    {
                        //RGB
                        double di = i - offset;
                        double dd = (width * (depth / 8));
                        int x = (int) Math.Round(di / dd);
                        int y = (j - i) / (depth / 8);
                        data[x, y] = new Bgr(file[j], file[j + 1], file[j + 2]);
                        
                        // a décommenter si on veut afficher l'image dans la console
                        /* int avg = (file[j] + file[j + 1] + file[j + 2]) / (depth / 8);
                         if (avg < file[j])
                         {
                             Console.ForegroundColor = ConsoleColor.Blue;
                         }
                         else if (avg < file[j + 1])
                         {
                             Console.ForegroundColor = ConsoleColor.Green;
                         }
                         else if (avg < file[j + 2])
                         {
                             Console.ForegroundColor = ConsoleColor.Red;
                         }
                         else if (avg < 128)
                         {
                             Console.ForegroundColor = ConsoleColor.Black;
                         }
                         else
                         {
                             Console.ForegroundColor = ConsoleColor.White;
                         }
 
                         Console.WriteLine(avg);*/
                    }
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Convertit l'instance MyImage en un fichier BMP
        /// </summary>
        /// <param name="output">emplacement du fichier de sortie</param>
        public void From_Image_To_File(string output)
        {
            byte[] file = new byte[size];
            /// recreate the header (from 0 to 14)
            // type : BMP
            file[0] = 66; // B
            file[1] = 77; // M

            // size of the bitmap
            byte[] bSize = Convertir_Int_To_Endian(size, 4);
            for (int i = 0; i < 4; i++)
            {
                file[2 + i] = bSize[i];
            }

            // application that creates the image, we use 666 because why not
            byte[] bAppId = Convertir_Int_To_Endian(666, 2);
            for (int i = 0; i < 2; i++)
            {
                file[6 + i] = bAppId[i];
            }

            // same for the next two bytes
            for (int i = 0; i < 2; i++)
            {
                file[8 + i] = bAppId[i];
            }

            // The offset, i.e. starting address, of the byte where the bitmap image data (pixel array) can be found.

            byte[] bOffset = Convertir_Int_To_Endian(offset, 4);
            for (int i = 0; i < 2; i++)
            {
                file[10 + i] = bOffset[i];
            }


            /// DIB Header

            // taille du header
            byte[] bSizeHeader = Convertir_Int_To_Endian(40, 4);
            for (int index = 0; index < 4; index++)
            {
                file[14 + index] = bSizeHeader[index];
            }

            // largeur de l'image
            byte[] bWidth = Convertir_Int_To_Endian(width, 4);
            for (int index = 0; index < 4; index++)
            {
                file[18 + index] = bWidth[index];
            }

            // hauteur de l'image
            byte[] bHeight = Convertir_Int_To_Endian(height, 4);
            for (int index = 0; index < 4; index++)
            {
                file[22 + index] = bHeight[index];
            }

            //profondeur de l image
            byte[] bDepth = Convertir_Int_To_Endian(depth, 4);
            for (int index = 0; index < 2; index++)
            {
                file[28 + index] = bDepth[index];
            }

            //nbr plans
            file[26] = 1;
            file[27] = 0;

            //compression utilisée mise à 0 par défaut
            for (int index = 0; index < 4; index++)
            {
                file[30 + index] = 0;
                file[42 + index] = 0;
                file[46 + index] = 0;
            }

            //taille image
            int taille = data.Length * (depth / 8);
            byte[] imageSize = Convertir_Int_To_Endian(taille, 4);
            for (int index = 0; index < 4; index++)
            {
                file[index + 34] = imageSize[index];
            }

            //resolution mise à 12000 because ca correspond à 300 ppi
            byte[] imageRes = Convertir_Int_To_Endian(12000, 4);
            for (int index = 0; index < 4; index++)
            {
                file[index + 38] = imageRes[index];
            }

            /// Pixel Array
            int widthPlusPlus = 4 * (((width * (depth / 8)) + 4 / 2) / 4);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < widthPlusPlus - 3; x += 3)
                {
                    if (x / (depth / 8) < width)
                    {
                        byte[] pixel = {(byte) data[y, x / 3].B, (byte) data[y, x / 3].G, (byte) data[y, x / 3].R};
                        file[offset + y * widthPlusPlus + x] = pixel[0];
                        file[offset + y * widthPlusPlus + x + 1] = pixel[1];
                        file[offset + y * widthPlusPlus + x + 2] = pixel[2];
                    }
                    else
                    {
                        // on est dans les pixels "fantomes" qui sont la juste pour que le nombre de byte dans la ligne soit divisible par 4
                        file[offset + y * widthPlusPlus + x] = 0;
                        file[offset + y * widthPlusPlus + x + 1] = 0;
                        file[offset + y * widthPlusPlus + x + 2] = 0;
                    }
                }
            }

            File.WriteAllBytes(output, file);
        }

        #endregion
    }
}