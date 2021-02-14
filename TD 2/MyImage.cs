using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

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
        /// <returns>Renvoi l'entier décimal correspondant au tableau de bytes</returns>
        /// <exception cref="ArgumentNullException">Retourne une erreur si le tableau est nul</exception>
        /// <exception cref="ArgumentOutOfRangeException">Retourne une erreur si le tailleTab + posMin dépasse la taille du tableau</exception>
        /// <exception cref="ArgumentException">Retourne une erreur si les entiers fournit ne sont pas positifs</exception>
        public static int Convertir_Endian_To_Int(byte[] tab, int tailleTab, int posMin)
        {
            if (tab == null)
            {
                // le tableau de bytes ne doit pas être null pour qu'on puisse le convertir, donc si il l'est on renvoi une erreur.
                throw new ArgumentNullException(nameof(tab));
            }

            if (posMin + tailleTab > tab.Length - 1)
            {
                // on sort du tableau, donc on renvoi une erreur.
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
        /// <returns>Renvoi le tableau de bytes correspondant à l'entier décimal</returns>
        public static byte[] Convertir_Int_To_Endian(int value, int tailleTab)
        {
            byte[] tab = new byte[tailleTab];
            for (int index = 0; index < tailleTab; index++)
            {
                tab[index] = (byte)(value % 256);
                int quotient = (value / 256);
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

            data = new Bgr[height, widthPlusPlus / (depth / 8)];

            for (int i = offset; i < offset + realImageSize; i = i + widthPlusPlus)
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
                        int x = (int)Math.Round(di / dd);
                        int y = (j - i) / (depth / 8);
                        data[x, y] = new Bgr(file[j], file[j + 1], file[j + 2]);

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
            // recreate the header (from 0 to 14)
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


            // DIB Header

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

            // Pixel Array
            int widthPlusPlus = 4 * (((width * (depth / 8)) + 4 / 2) / 4);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < widthPlusPlus - (depth / 8); x += (depth / 8))
                {
                    if (x / (depth / 8) < width)
                    {
                        byte[] pixel = { (byte)data[y, x / (depth / 8)].B, (byte)data[y, x / (depth / 8)].G, (byte)data[y, x / (depth / 8)].R };
                        file[offset + y * widthPlusPlus + x] = pixel[0];
                        file[offset + y * widthPlusPlus + x + 1] = pixel[1];
                        file[offset + y * widthPlusPlus + x + 2] = pixel[2];
                    }
                    else
                    {
                        // on est dans les pixels "fantômes" qui sont la juste pour que le nombre de byte dans la ligne soit divisible par 4
                        file[offset + y * widthPlusPlus + x] = 0;
                        file[offset + y * widthPlusPlus + x + 1] = 0;
                        file[offset + y * widthPlusPlus + x + 2] = 0;
                    }
                }
            }

            File.WriteAllBytes(output, file);
        }


        public static void From_Tableau_To_File(Bgr[,] tab, int width, int height, string output)
        {
            int size = 14 + 40 + tab.Length*3+1000;
            byte[] file = new byte[size];
            // recreate the header (from 0 to 14)
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

            byte[] bOffset = Convertir_Int_To_Endian(54, 4);
            for (int i = 0; i < 2; i++)
            {
                file[10 + i] = bOffset[i];
            }


            // DIB Header

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
            byte[] bDepth = Convertir_Int_To_Endian(24, 4);
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
            int taille = tab.Length * 3;
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

            // Pixel Array
            int widthPlusPlus = 4 * (((width * 3) + 4 / 2) / 4);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < widthPlusPlus - 3; x += 3)
                {
                    if (x / 3 < width)
                    {
                        byte[] pixel = { (byte)tab[y, x / 3].B, (byte)tab[y, x / 3].G, (byte)tab[y, x / 3].R };
                        file[54 + y * widthPlusPlus + x] = pixel[0];
                        file[54 + y * widthPlusPlus + x + 1] = pixel[1];
                        file[54 + y * widthPlusPlus + x + 2] = pixel[2];
                    }
                    else
                    {
                        // on est dans les pixels "fantômes" qui sont la juste pour que le nombre de byte dans la ligne soit divisible par 4
                        file[54 + y * widthPlusPlus + x] = 0;
                        file[54 + y * widthPlusPlus + x + 1] = 0;
                        file[54 + y * widthPlusPlus + x + 2] = 0;
                    }
                }
            }

            File.WriteAllBytes(output, file);
        }

        /// <summary>
        /// transforme l'image en nuance de gris
        /// </summary>
        public void passageNegatif()
        {
            int widthPlusPlus = 4 * (((width * (depth / 8)) + 4 / 2) / 4);
            for (int index = 0; index < height; index++)
            {
                for (int index1 = 0; index1 < widthPlusPlus/(depth / 8); index1++)
                {
                    if (index1 / (depth / 8) < width)
                    {
                        int avg = (data[index, index1].R + data[index, index1].G + data[index, index1].B) / 3;// on calcule
                        Bgr pixel = new Bgr(avg, avg, avg);
                        data[index, index1] = pixel;
                    }
                    else
                    {
                        Bgr pixel = new Bgr(0, 0, 0);
                        data[index, index1] = pixel;
                    }
                }
            }
        }

        public Bgr[,]RetrecissementBis(int ratio)
        {
            if (ratio < 1 || ratio > width || ratio > height)
            {
                throw new ArgumentException("Ce ratio de rétrécissement est incorrect");
            }

            int newWidth = width / ratio;
            int newHeigth = height / ratio;
            Bgr[,] output = new Bgr[newHeigth, newWidth];
            for(int index=0;index<width;index+=ratio)
            {
                for(int index1=0;index1<height;index1+=ratio)
                {
                    try
                    {
                        Bgr pixel = new Bgr(data[index1, index].B, data[index1, index].G, data[index1, index].R);
                        output[index1 / ratio, index / ratio] = pixel;
                    }
                    catch (Exception e)
                    {

                    }
                    
                }
            }
            AfficherImage(output);
            From_Tableau_To_File(output, newWidth, newHeigth, ".\\retrecie.bmp");

            return output;
        }
        public Bgr[,] Retrecissement(int ratio)
        {
            if(ratio<1 || ratio>width || ratio > height)
            {
                throw new ArgumentException("Ce ratio de rétrécissement est incorrect") ;
            }
            
            int newWidth = width / ratio;
            int newHeight = height/ ratio;
            int largeurDiv = width / newWidth;
            int hauteurDiv = height / newHeight;
            int newWidthPlusPlus = 4 * (((newWidth * 3) + 4 / 2) / 4);
            int widthPlusPlus = 4 * (((width * 3) + 4 / 2) / 4);
            Bgr[,] output = new Bgr[newHeight, newWidthPlusPlus];
            for(int index=0;index<newWidthPlusPlus;index++)
            {
                for(int index1=0;index1<newHeight;index1++)
                {

                    int avgR = 0;
                    int avgB = 0;
                    int avgG = 0;
                    int n = 0;
                    for (int i= largeurDiv*index1; i< largeurDiv * (index1+1) && i < widthPlusPlus; i+=largeurDiv)
                    {
                        for(int j= hauteurDiv * index; j< hauteurDiv * (index + 1) && j < height; j+=hauteurDiv)
                        {
                            
                                avgR += data[j, i].R;
                                avgB += data[j, i].B;
                                avgG += data[j, i].G;
                                n++;
                            

                        }
                    }

                    try
                    {
                        avgR = avgR / n;
                        avgG = avgG / n;
                        avgB = avgB / n;
                        output[index, index1] = new Bgr(avgB, avgG, avgR);
                    }
                    catch (Exception e)
                    {

                    }

                }
            }
            AfficherImage(output);
            From_Tableau_To_File(output,newWidth, newHeight, ".\\retrecie.bmp" );

            return output;
        }

        public Bgr[,] EffetMiroir()
        {
            Bgr[,] output = new Bgr[height, width];
            for (int index = 0; index < width; index++)
            {
                for (int index1 = 0; index1 < height; index1++)
                {
                    Bgr pixel = new Bgr(data[index1, index].B, data[index1, index].G, data[index1, index].R);
                    output[index1, width -1- index] = pixel;

                }
            }

            //AfficherImage(output);
            From_Tableau_To_File(output, width, height, ".\\Miroir.bmp");

            return output;
        }

        public Bgr[,] Rotation(int angle)
        {
            Bgr[,] output = new Bgr[height, width];
            double[] rotation =new double[2];
            rotation[0] = Math.Cos(angle / 180 * Math.PI);
            rotation[1] = Math.Sin(angle / 180 * Math.PI);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double[] pos = {x, y};
                    IEnumerable<double> dotProducts = pos.Zip(rotation, (a, b) => a * b);
                    double[] res = dotProducts.ToArray();
                    Console.WriteLine($"x = {res[0]} | y = {res[1]} ");
                    try
                    {
                        output[(int) res[1], (int) res[0]] = data[y, x];
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }
            From_Tableau_To_File(output, width, height, ".\\rotation.bmp");
            return output;
        }
        
        public static void AfficherImage(Bgr[,] tab)
        {
            for (int y = 0; y < tab.GetLength(0); y++)
            {
                for (int x = 0; x < tab.GetLength(1); x++)
                {
                    int avg = (tab[y, x].R + tab[y, x].G + tab[y, x].B) / 3;
                    if (avg < tab[y, x].B)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    else if (avg < tab[y, x].G)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (avg < tab[y, x].R)
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

                    Console.Write(avg);
                }
                Console.WriteLine();
            }
        }

        #endregion
    }
}