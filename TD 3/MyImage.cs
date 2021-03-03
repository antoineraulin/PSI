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

namespace TD_3
{
    class MyImage
    {
        
        // todo
        // - faire la transformation en noir et blanc en plus du nuance de gris
        // - Rotation
        // - Miroir
        
        
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
        /// <exception cref="ArgumentException">Retourne une erreur si l"image </exception>
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
            depth = Convertir_Endian_To_Int(file, 2, 28);
            // on cherche l'entier divisible par 4 le plus proche du nombre de bytes dans la ligne
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            realImageSize = Convertir_Endian_To_Int(file, 4, 34);
            // Le format BMP autorise à mettre zero dans ce champ même si on a des données après l'image, on est obligé d'être sur du point d'arret des données de l'image.
            if(realImageSize == 0) realImageSize = widthPlusPlus * height;
            if (depth != 24)
            {
                throw new ArgumentException("la profondeur de l'image doit être de 24 bits ");
            }

            Console.WriteLine($"width : {width}");
            Console.WriteLine($"height : {height}");
            Console.WriteLine($"depth : {depth}");
            Console.WriteLine($"realImageSize : {realImageSize}");


            // L'image elle-même

            Console.WriteLine("bytes image:");

            

            data = new Bgr[height, widthPlusPlus / (depth / 8)];

            for (int i = offset; i < offset + realImageSize; i = i + widthPlusPlus)
            {
                for (int j = i; j < i + width * (depth / 8); j += depth / 8)
                {

                    if (depth / 8 == 3)
                    {

                        double di = i - offset;
                        double dd = (width * (depth / 8));
                        int x = (int)(di / dd);
                        int y = (j - i) / (depth / 8);
                        data[x, y] = new Bgr(file[j], file[j + 1], file[j + 2]);

                    }
                }

                
            }
            
        }

        public MyImage(Bgr[,] tab, int width, int height)
        {
            data = tab;
            this.width = width;
            int widthPlusPlus = (width*3 + 3) & ~0x03;
            this.height = height;
            size = 14 + 40 + widthPlusPlus* height;
            Console.WriteLine(size);
            offset = 54; //offset de base
            realImageSize = widthPlusPlus* height;
            depth = 24;

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
            //int width2 = width;
            int widthPlusPlus = (width*3 + 3) & ~0x03;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x ++)
                {
                    if (x / (depth / 8) < width)
                    {
                        int pos = offset + y * widthPlusPlus + x * 3;
                        //int pos = offset + y * width + x * 3;
                        file[pos] = (byte)data[y, x].B;
                        file[pos + 1] = (byte)data[y, x].G;
                        file[pos + 2] = (byte)data[y, x].R;
                    }
                    /*else
                    {
                        // on est dans les pixels "fantômes" qui sont la juste pour que le nombre de byte dans la ligne soit divisible par 4
                        file[offset + y * widthPlusPlus + x*3] = 0;
                        file[offset + y * widthPlusPlus + x*3 + 1] = 0;
                        file[offset + y * widthPlusPlus + x*3 + 2] = 0;
                    }*/
                }
            }

            File.WriteAllBytes(output, file);
        }


        public static void From_Tableau_To_File(Bgr[,] tab, string output)
        {
            int width = tab.GetLength(1);
            int height = tab.GetLength(0);
            int size = 14 + 40 + tab.Length*3;
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
        public MyImage ToGrayscale()
        {
            Bgr[,] output = new Bgr[height, width];
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            for (int index = 0; index < height; index++)
            {
                for (int index1 = 0; index1 < widthPlusPlus/(depth / 8); index1++)
                {
                    if (index1 / (depth / 8) < width)
                    {
                        int avg = (data[index, index1].R + data[index, index1].G + data[index, index1].B) / 3;// on calcule
                        Bgr pixel = new Bgr(avg, avg, avg);
                        output[index, index1] = pixel;
                    }
                    else
                    {
                        Bgr pixel = new Bgr(0, 0, 0);
                        output[index, index1] = pixel;
                    }
                }
            }

            return new MyImage(output, width, height);
        }

        /// <summary>
        /// transforme l'image en noir et blanc
        /// </summary>
        public MyImage ToBW()
        {
            Bgr[,] output = new Bgr[height, width];
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            for (int index = 0; index < height; index++)
            {
                for (int index1 = 0; index1 < widthPlusPlus / (depth / 8); index1++)
                {
                    if (index1 / (depth / 8) < width)
                    {
                        int avg = (data[index, index1].R + data[index, index1].G + data[index, index1].B) / 3;// on calcule
                        Bgr pixel = pixel = new Bgr(255, 255, 255);
                        if (avg < 128)
                        {
                            pixel = new Bgr(0, 0, 0);
                        }
                        
                        output[index, index1] = pixel;
                    }
                    else
                    {
                        Bgr pixel = new Bgr(0, 0, 0);
                        output[index, index1] = pixel;
                    }
                }
            }
            return new MyImage(output, width, height);
        }



        public MyImage Resize(int newWidth, int newHeight)
        {
            Bgr[,] output = new Bgr[newHeight, newWidth];
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // on fait une bête règle de trois pour savoir à quel pixel de l'image originale notre pixel correspond.
                    int yInOriginal = (y * height) / newHeight; // le fait de calculer seulement avec des int permet d'arrondir strictement à l'inférieur, et donc on ne sors pas de l'image originale.
                    int xInOriginal = (x * width) / newWidth;
                    output[y, x] = data[yInOriginal, xInOriginal];
                }
            }

            return new MyImage(output, newWidth, newHeight);
        }

        public MyImage ResizeAntialiasing(int newWidth, int newHeight)
        {
            Bgr[,] output = new Bgr[newHeight, newWidth];
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    
                    double yInOriginal = (((double)y * (double)height) / (double)newHeight);
                    double xInOriginal = (((double)x * (double)width) / (double)newWidth);
                    int strInfY = (int) yInOriginal;
                    int strInfX = (int) xInOriginal;
                    double yRemainder = yInOriginal - (double) strInfY;
                    double xRemainder = xInOriginal - (double)strInfX;
                    double avg = (yRemainder + xRemainder) / 2;
                    int blue, red, green;
                    if(strInfY + 1 < height && strInfX + 1 < width)
                    {
                        blue = (int) ((data[strInfY, strInfX].B + avg * data[strInfY + 1, strInfX + 1].B) /
                                          (1.0 + avg));
                        red = (int) ((data[strInfY, strInfX].R + avg * data[strInfY + 1, strInfX + 1].R) /
                                         (1.0 + avg)); 
                        green = (int) ((data[strInfY, strInfX].G + avg * data[strInfY + 1, strInfX + 1].G) /
                                                                      (1.0 + avg));
                    }
                    else
                    {
                        blue = data[strInfY, strInfX].B;
                        red = data[strInfY, strInfX].R;
                        green = data[strInfY, strInfX].G;
                    }

                    Bgr pixel = new Bgr(blue, green, red);
                    output[y, x] = pixel;
                }
            }

            return new MyImage(output, newWidth, newHeight);
        }

        public MyImage EffetMiroir()
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
            From_Tableau_To_File(output, ".\\Miroir.bmp");

            return new MyImage(output, width, height);
        }

        public double[] ToPolarCoordinates(double y, double x,double theta = 0)
        {
            double radius = Math.Sqrt((x * x) + (y * y));
            double angle = Math.Atan2(y, x);
            double[] res = {angle+theta, radius};
            return res;
        }

        public double[] ToCartesianCoordinates(double[] polar)
        {
            double[] res = {polar[1] * Math.Sin(polar[0]), polar[1] * Math.Cos(polar[0]) };
            return res;
        }

        /// <summary>
        /// Permet d'appliquer un angle de rotation sur l'image
        /// </summary>
        /// <param name="angle">angle de rotation, dans le sens horaire, en radian si le paramètre radian est vrai (par défaut), en degré sinon</param>
        /// <param name="radian">définit si l'angle donné est en radian ou en degré</param>
        /// <returns>Retourne l'image après rotation</returns>
        public MyImage RotationV2(double angle, bool radian=true)
        {
            // conversion degré vers radian
            if (!radian) angle = angle * Math.PI / 180;

            // définissons le centre de rotation au centre de l'image d'origine
            double y0 = height / 2.0;
            double x0 = width / 2.0;

            // trouvons la taille de l'image après rotation pour que l'image soit contenu entièrement dans le cadre
            double[] topLeftCorner = ToCartesianCoordinates(ToPolarCoordinates(0, 0, angle));
            double[] topRightCorner = ToCartesianCoordinates(ToPolarCoordinates(0, width - 1, angle));
            double[] bottomRightCorner = ToCartesianCoordinates(ToPolarCoordinates(height - 1, width - 1, angle));
            double[] bottomLeftCorner = ToCartesianCoordinates(ToPolarCoordinates(height - 1, 0, angle));
            double[][] corners = { topLeftCorner, topRightCorner, bottomLeftCorner, bottomRightCorner };
            double maxY = -9999999.0;
            double minY = 9999999.0;
            double maxX = -9999999.0;
            double minX = 9999999.0;
            foreach (double[] corner in corners)
            {
                if (corner[0] > maxY) maxY = corner[0];
                if (corner[1] > maxX) maxX = corner[1];
                if (corner[0] < minY) minY = corner[0];
                if (corner[1] < minX) minX = corner[1];
            }
            int newHeight = (int)(maxY - minY)+1;
            int newWidth = (int) (maxX - minX)+1;
            Console.WriteLine($"newHeight : {newHeight} | newWidth : {newWidth}");

            // définissons le centre de rotation au centre de l'image d'arrivée
            double y0prime = newHeight / 2.0;
            double x0prime = newWidth / 2.0;

            Bgr[,] output = new Bgr[newHeight, newWidth];
            for (double y1 = 0.0; y1 < output.GetLength(0); y1 += 1)
            {
                for (double x1 = 0.0; x1 < output.GetLength(1); x1 += 1)
                {
                    // on choisit que les pixels vide seront remplis de noir :
                    Bgr color = new Bgr(0, 0, 0);
                    double[] polar = ToPolarCoordinates(y1 - y0prime, x1 - x0prime, -angle);
                    double[] pixel = ToCartesianCoordinates(polar);
                    int intX2 = (int)Math.Round(pixel[1] + x0, MidpointRounding.AwayFromZero);
                    int intY2 = (int)Math.Round(pixel[0] + y0, MidpointRounding.AwayFromZero);
                    if (intX2 >= 0 && intX2 < width && intY2 >= 0 && intY2 < height)
                    {
                        color = data[intY2, intX2];
                    }

                    output[(int) y1, (int) x1] = color;
                }
            }

            return new MyImage(output, newWidth, newHeight);
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