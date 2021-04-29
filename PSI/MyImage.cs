using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace PSI
{
    class MyImage
    {

        // todo
        // - faire la transformation en noir et blanc en plus du nuance de gris
        // - Rotation
        // - Miroir


        #region PRIVATE

        public int height { get; }

        public int width { get; }

        // définition des variables privées de l'objet
        private int size, offset, depth, realImageSize;

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

            Console.WriteLine(@"C'est un bitmap");

            size = Convertir_Endian_To_Int(file, 4, 2);

            offset = Convertir_Endian_To_Int(file, 4, 10);
            Console.WriteLine($@"size : {size}");
            Console.WriteLine($@"offset : {offset}");


            width = Convertir_Endian_To_Int(file, 4, 18);
            height = Convertir_Endian_To_Int(file, 4, 22);
            depth = Convertir_Endian_To_Int(file, 2, 28);
            // on cherche l'entier divisible par 4 le plus proche du nombre de bytes dans la ligne
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            realImageSize = Convertir_Endian_To_Int(file, 4, 34);
            // Le format BMP autorise à mettre zero dans ce champ même si on a des données après l'image, on est obligé d'être sur du point d'arret des données de l'image.
            if (realImageSize == 0) realImageSize = widthPlusPlus * height;
            if (depth != 24)
            {
                throw new ArgumentException("la profondeur de l'image doit être de 24 bits ");
            }

            Console.WriteLine($@"width : {width}");
            Console.WriteLine($@"height : {height}");
            Console.WriteLine($@"depth : {depth}");
            Console.WriteLine($@"realImageSize : {realImageSize}");


            // L'image elle-même

            Console.WriteLine(@"bytes image:");



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
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            this.height = height;
            size = 14 + 40 + widthPlusPlus * height;
            Console.WriteLine(size);
            offset = 54; //offset de base
            realImageSize = widthPlusPlus * height;
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
            byte[] bheight = Convertir_Int_To_Endian(height, 4);
            for (int index = 0; index < 4; index++)
            {
                file[22 + index] = bheight[index];
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
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
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


        /// <summary>
        /// transforme l'image en nuance de gris
        /// </summary>
        public MyImage ToGrayscale()
        {
            Bgr[,] output = new Bgr[height, width];
            int widthPlusPlus = (width * 3 + 3) & ~0x03;
            for (int index = 0; index < height; index++)
            {
                for (int index1 = 0; index1 < width; index1++)
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
                for (int index1 = 0; index1 < width; index1++)
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



        public MyImage Resize(int newWidth, int newheight)
        {
            Bgr[,] output = new Bgr[newheight, newWidth];
            for (int y = 0; y < newheight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    // on fait une bête règle de trois pour savoir à quel pixel de l'image originale notre pixel correspond.
                    int yInOriginal = (y * height) / newheight; // le fait de calculer seulement avec des int permet d'arrondir strictement à l'inférieur, et donc on ne sors pas de l'image originale.
                    int xInOriginal = (x * width) / newWidth;
                    output[y, x] = data[yInOriginal, xInOriginal];
                }
            }

            return new MyImage(output, newWidth, newheight);
        }

        public MyImage ResizeAntialiasing(int newWidth, int newheight)
        {
            Bgr[,] output = new Bgr[newheight, newWidth];
            for (int y = 0; y < newheight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {

                    double yInOriginal = (((double)y * (double)height) / (double)newheight);
                    double xInOriginal = (((double)x * (double)width) / (double)newWidth);
                    int strInfY = (int)yInOriginal;
                    int strInfX = (int)xInOriginal;
                    double yRemainder = yInOriginal - (double)strInfY;
                    double xRemainder = xInOriginal - (double)strInfX;
                    double avg = (yRemainder + xRemainder) / 2;
                    int blue, red, green;
                    if (strInfY + 1 < height && strInfX + 1 < width)
                    {
                        blue = (int)((data[strInfY, strInfX].B + avg * data[strInfY + 1, strInfX + 1].B) /
                                          (1.0 + avg));
                        red = (int)((data[strInfY, strInfX].R + avg * data[strInfY + 1, strInfX + 1].R) /
                                         (1.0 + avg));
                        green = (int)((data[strInfY, strInfX].G + avg * data[strInfY + 1, strInfX + 1].G) /
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

            return new MyImage(output, newWidth, newheight);
        }

        public MyImage EffetMiroir()
        {
            Bgr[,] output = new Bgr[height, width];
            for (int index = 0; index < width; index++)
            {
                for (int index1 = 0; index1 < height; index1++)
                {
                    Bgr pixel = new Bgr(data[index1, index].B, data[index1, index].G, data[index1, index].R);
                    output[index1, width - 1 - index] = pixel;

                }
            }

            return new MyImage(output, width, height);
        }

        public double[] ToPolarCoordinates(double y, double x, double theta = 0)
        {
            double radius = Math.Sqrt((x * x) + (y * y));
            double angle = Math.Atan2(y, x);
            double[] res = { angle + theta, radius };
            return res;
        }

        public double[] ToCartesianCoordinates(double[] polar)
        {
            double[] res = { polar[1] * Math.Sin(polar[0]), polar[1] * Math.Cos(polar[0]) };
            return res;
        }

        /// <summary>
        /// Permet d'appliquer un angle de rotation sur l'image
        /// </summary>
        /// <param name="angle">angle de rotation, dans le sens horaire, en radian si le paramètre radian est vrai (par défaut), en degré sinon</param>
        /// <param name="radian">définit si l'angle donné est en radian ou en degré</param>
        /// <returns>Retourne l'image après rotation</returns>
        public MyImage RotationV2(double angle, bool radian = true)
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
            int newheight = (int)(maxY - minY) + 1;
            int newWidth = (int)(maxX - minX) + 1;
            Console.WriteLine($@"newheight : {newheight} | newWidth : {newWidth}");

            // définissons le centre de rotation au centre de l'image d'arrivée
            double y0prime = newheight / 2.0;
            double x0prime = newWidth / 2.0;

            Bgr[,] output = new Bgr[newheight, newWidth];
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

                    output[(int)y1, (int)x1] = color;
                }
            }

            return new MyImage(output, newWidth, newheight);
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

        public MyImage calculateKernel(double[,] kernel, double[,] kernel2 = null)
        {
            Bgr[,] output = new Bgr[height, width];
            int kernelWidth = kernel.GetLength(1);
            int kernelheight = kernel.GetLength(0);
            if (kernel2 != null)
            {
                // on a 2 kernels
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int[] pos = new int[2] { y, x };
                        int[,,] theNeighbors = Getneighbors(pos, kernelWidth, kernelheight);
                        double magnitudeX = 0;
                        double magnitudeY = 0;
                        for (int i = 0; i < kernelheight; i++)
                        {
                            for (int j = 0; j < kernelWidth; j++)
                            {
                                Bgr pix = data[theNeighbors[i, j, 0], theNeighbors[i, j, 1]];
                                magnitudeX += pix.B * kernel[i, j];
                                magnitudeY += pix.B * kernel2[i, j];
                            }
                        }
                        int color = (int)(Math.Sqrt(magnitudeX * magnitudeX + magnitudeY * magnitudeY));
                        output[y, x] = new Bgr(color, color, color);
                    }
                }
            }
            else
            {
                // on a 1 kernel
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double blue = 0;
                        double green = 0;
                        double red = 0;
                        int[] pos = new int[2] { y, x };
                        double n = 0;
                        int[,,] theNeighbors = Getneighbors(pos, kernelWidth, kernelheight);
                        for (int i = 0; i < kernelheight; i++)
                        {
                            for (int j = 0; j < kernelWidth; j++)
                            {
                                Bgr pix = data[theNeighbors[i, j, 0], theNeighbors[i, j, 1]];
                                double b = (double)pix.B;
                                double g = (double)pix.G;
                                double r = (double)pix.R;
                                n += kernel[i, j];
                                blue += b * kernel[i, j];
                                green += g * kernel[i, j];
                                red += r * kernel[i, j];
                            }
                        }

                        if (green > 255)
                        {
                            green = 255;
                        }

                        if (blue > 255)
                        {
                            blue = 255;
                        }

                        if (red > 255)
                        {
                            red = 255;
                        }
                        output[y, x] = new Bgr((int)Math.Round(blue), (int)Math.Round(green), (int)Math.Round(red));

                    }
                }
            }

            return new MyImage(output, width, height);
        }

        public int[,,] Getneighbors(int[] pos, int kernelWidth, int kernelheight)
        {
            int[,,] neighbors = new int[kernelheight, kernelWidth, 2];
            int x = pos[1];
            int y = pos[0];
            for (int i = -kernelheight / 2; i < kernelheight / 2 + 1; i++)
                for (int j = -kernelWidth / 2; j < kernelWidth / 2 + 1; j++)
                {

                    int rx = x + i;
                    int ry = y + j;

                    if (rx >= 0 && ry >= 0 && rx < width && ry < height)
                    {
                        neighbors[i + kernelheight / 2, j + kernelWidth / 2, 0] = ry;
                        neighbors[i + kernelheight / 2, j + kernelWidth / 2, 1] = rx;
                    }
                    else
                    {
                        neighbors[i + kernelheight / 2, j + kernelWidth / 2, 0] = y;
                        neighbors[i + kernelheight / 2, j + kernelWidth / 2, 1] = x;
                    }


                }

            return neighbors;
        }

        public double[,] MatriceKernel(int type)
        {
            double[,] Kernel = new double[3, 3];


            if (type == 2)//Flou
            {
                Kernel = new double[3, 3];
                Kernel[0, 0] = 1.0 / 16.0;
                Kernel[0, 2] = Kernel[0, 0];
                Kernel[2, 0] = Kernel[0, 0];
                Kernel[2, 2] = Kernel[0, 0];
                Kernel[0, 1] = 2.0 / 16.0;
                Kernel[1, 0] = Kernel[0, 1];
                Kernel[1, 2] = Kernel[0, 1];
                Kernel[2, 1] = Kernel[0, 1];
                Kernel[1, 1] = 4.0 / 16.0;
            }

            else if (type == 3)// Sharp
            {
                Kernel = new double[3, 3];
                Kernel[0, 0] = 0;
                Kernel[0, 2] = Kernel[0, 0];
                Kernel[2, 0] = Kernel[0, 0];
                Kernel[2, 2] = Kernel[0, 0];
                Kernel[0, 1] = -1;
                Kernel[1, 0] = Kernel[0, 1];
                Kernel[1, 2] = Kernel[0, 1];
                Kernel[2, 1] = Kernel[0, 1];
                Kernel[1, 1] = 5;
            }

            else if (type == 4)//Repoussage
            {

                Kernel = new double[5, 5] { { -1.0 / 256.0, -4.0 / 256.0, -6.0 / 256.0, -14.0 / 256.0, -1.0 / 256.0 }, { -4.0 / 256.0, -16.0 / 256.0, -24.0 / 256.0, -16.0 / 256.0, -4.0 / 256.0 }, { -6.0 / 256.0, -24.0 / 256.0, 476.0 / 256.0, -24.0 / 256.0, -6.0 / 256.0 }, { -4.0 / 256.0, -16.0 / 256.0, -24.0 / 256.0, -16.0 / 256.0, -4.0 / 256.0 }, { -1.0 / 256.0, -4.0 / 256.0, -6.0 / 256.0, -4.0 / 256.0, -1.0 / 256.0 } };



                /*Kernel[0, 0] = -1.0/ 256.0;
                Kernel[0,  4]  =  Kernel[0,  0];
                Kernel[4, 0] = Kernel[0, 0];
                Kernel[4, 4] = Kernel[0, 0];

                Kernel[0,  1]  =  -4.0  /  256.0;
                Kernel[0,  3]=  Kernel[0,  1];
                Kernel[1, 0] = Kernel[0, 1];
                Kernel[1, 4] = Kernel[0, 1];
                Kernel[3, 0] = Kernel[0, 1];
                Kernel[4, 1] = Kernel[0, 1];
                Kernel[4, 3] = Kernel[0, 1];
                Kernel[3, 4] = Kernel[0, 1];

                Kernel[0,  2]  =  -6.0  /  256.0;
                Kernel[2,  0]  =  Kernel[0,  2];
                Kernel[2, 4] = Kernel[0, 2];
                Kernel[4, 2] = Kernel[0, 2];

                Kernel[1, 1] = -16.0 / 256.0;
                Kernel[1, 3] = Kernel[1, 1];
                Kernel[3, 1] = Kernel[1, 1];
                Kernel[3, 3] = Kernel[1, 1];

                Kernel[1, 2] = -24.0 / 256.0;
                Kernel[2, 1] = Kernel[1, 2];
                Kernel[2, 3] = Kernel[1, 2];
                Kernel[3, 2] = Kernel[1, 2];

                Kernel[2,  2]  =  -476.0  / 256.0;*/

            }
            return Kernel;
        }

        public static MyImage MandelBrot(int iteration_max, int image_x, int image_y)
        {
            //on définit la taille de la fractale
            double x1 = 0.22;
            double x2 = 0.32;
            double y1 = 0.10;
            double y2 = -0.10;

            Bgr[,] output = new Bgr[image_y, image_x];

            //on calcule la taille de l'image
            double zoom_x = image_x / (x2 - x1);
            double zoom_y = image_y / (y2 - y1);

            for (double x = 0; x < image_x; x++)
            {
                for (double y = 0; y < image_y; y++)
                {
                    double c_r = x / zoom_x + x1;
                    double c_i = y / zoom_y + y1;
                    double z_r = 0;
                    double z_i = 0;
                    int i = 0;

                    do
                    {
                        double tmp = z_r;
                        z_r = z_r * z_r - z_i * z_i + c_r;
                        z_i = 2 * z_i * tmp + c_i;
                        i++;
                    } while (z_r * z_r + z_i * z_i < 4 && i < iteration_max);

                    if (i == iteration_max)
                    {
                        output[(int)y, (int)x] = new Bgr(0, 0, 0);
                    }
                    else
                    {
                        output[(int)y, (int)x] = new Bgr(0, 0, (int)(i * 255.0 / iteration_max));
                    }
                }
            }

            return new MyImage(output, image_x, image_y);
        }

        public static MyImage Julia(int iteration_max, int image_x, int image_y)
        {
            double x1 = -1;
            double x2 = 1;
            double y1 = -1.2;
            double y2 = 1.2;


            //on calcule la taille de l'image
            double zoom_x = image_x / (x2 - x1);
            double zoom_y = image_y / (y2 - y1);

            Bgr[,] output = new Bgr[image_y, image_x];

            for (int x = 0; x < image_x; x++)
            {
                for (int y = 0; y < image_y; y++)
                {
                    double c_r = 0.285;
                    double c_i = 0.01;
                    double z_r = x / zoom_x + x1;
                    double z_i = y / zoom_y + y1;
                    double i = 0;

                    do
                    {
                        double tmp = z_r;
                        z_r = z_r * z_r - z_i * z_i + c_r;
                        z_i = 2 * z_i * tmp + c_i;
                        i++;

                    } while (z_r * z_r + z_i * z_i < 4 && i < iteration_max);


                    if (i == iteration_max)
                    {
                        output[(int)y, (int)x] = new Bgr(0, 0, 0);
                    }
                    else
                    {
                        output[(int)y, (int)x] = new Bgr(0, 0, (int)(i * 255.0 / iteration_max));
                    }
                }


            }

            return new MyImage(output, image_x, image_y);

        }

        public MyImage Histogramme()
        {

            int[] blueRepartition = new int[256];
            int[] greenRepartition = new int[256];
            int[] redRepartition = new int[256];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    blueRepartition[data[y, x].B]++;
                    greenRepartition[data[y, x].G]++;
                    redRepartition[data[y, x].R]++;
                }
            }

            int[] maxs = { blueRepartition.Max(), redRepartition.Max(), greenRepartition.Max() };
            int max = maxs.Max();
            Bgr[,] output = new Bgr[max, 255];
            for (int y = 0; y < max; y++)
            {
                for (int x = 0; x < 255; x++)
                {
                    output[y, x] = new Bgr(blueRepartition[x] >= y ? 255 : 0, greenRepartition[x] >= y ? 255 : 0, redRepartition[x] >= y ? 255 : 0);
                }
            }
            MyImage res = new MyImage(output, 255, max);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return res;
        }

        public MyImage SteganographyEncode(MyImage imageToHide)
        {
            MyImage rescaled = Resize(imageToHide.width, imageToHide.height);
            rescaled.From_Image_To_File(@".\stegano.bmp");
            imageToHide.From_Image_To_File(@".\toHide.bmp");
            byte[] hideFile = File.ReadAllBytes(@".\toHide.bmp");
            byte[] originalFile = File.ReadAllBytes(@".\stegano.bmp");
            byte[] hideImageBytes = hideFile.Skip(imageToHide.offset).Take(imageToHide.realImageSize).ToArray();
            for (int i = offset; i < offset + realImageSize; i++)
            {
                int j = i - offset;
                if (j < hideImageBytes.Length)
                {
                    byte newLow = (byte)(hideImageBytes[j] & 0xF0);
                    originalFile[i] = (byte)(originalFile[i] & 0xF0 | (newLow >> 4));
                }
            }
            File.WriteAllBytes(@".\stegano.bmp", originalFile);
            return new MyImage(@".\stegano.bmp");
        }

        public MyImage SteganographyDecode()
        {
            From_Image_To_File(@".\stegano.bmp");
            byte[] originalFile = File.ReadAllBytes(@".\stegano.bmp");
            byte[] finalFile = originalFile;
            for (int i = offset; i < offset + realImageSize; i++)
            {
                byte low = (byte)(originalFile[i] & 0x0F);
                originalFile[i] = (byte)(originalFile[i] & 0x0F | (low << 4));
            }
            File.WriteAllBytes(@".\steganoRes.bmp", originalFile);
            return new MyImage(@".\steganoRes.bmp");
        }

        #region QRCODE

        public static int FromCharToInt(char c)
        {
            int res;
            if (Char.IsDigit(c))
            {
                res = Convert.ToInt32(new string(c, 1));
            }
            else if (Char.IsLetter(c))
            {
                res = c;
                res -= 55; //car code Ascii
            }
            else
            {
                switch (c)
                {
                    case ' ':
                        res = 36;
                        break;
                    case '$':
                        res = 37;
                        break;
                    case '%':
                        res = 38;
                        break;
                    case '*':
                        res = 39;
                        break;
                    case '+':
                        res = 40;
                        break;
                    case '-':
                        res = 41;
                        break;
                    case '.':
                        res = 42;
                        break;
                    case '/':
                        res = 43;
                        break;
                    case ':':
                        res = 44;
                        break;
                    default:
                        throw new ArgumentException("Caractère non reconnu");
                }
            }
            return res;

        }

        public static void GenerateQRCode(String input)
        {
            // Encodages préliminaires
            int inputLength = input.Length;
            int qrVersion = 1;
            String typeInfo = "0010";
            String binInputLenght = Convert.ToString(inputLength, 2).PadLeft(9, '0'); // version 1
            if (inputLength > 24)
            {
                qrVersion = 2;
                
            }

            

            // Analyse et Encodage des données 
            int[] inputInt = new int[input.Length];
            int[] inputPair = new int[input.Length / 2 + 1];
            for (int i = 0; i < input.Length; i++)
            {
                inputInt[i] = FromCharToInt(input[i]);
                Console.WriteLine(inputInt[i]);
            }

            String bitString = typeInfo + binInputLenght;
            Console.WriteLine("Create data segment :");
            for (int i = 0; i < input.Length; i += 2)
            {
                if (i + 1 < input.Length)
                {
                    inputPair[i / 2] = 45 * inputInt[i] + inputInt[i + 1];
                    bitString += Convert.ToString(inputPair[i / 2], 2).PadLeft(11, '0');
                    Console.WriteLine(inputPair[i / 2]);
                }
                else
                {
                    inputPair[i / 2] = inputInt[i];
                    bitString += Convert.ToString(inputPair[i / 2], 2).PadLeft(6, '0');
                    Console.WriteLine(inputPair[i / 2]);

                }
            }

            Console.WriteLine(bitString);

            int bitStringLength = bitString.Length;

            Console.WriteLine(bitStringLength);
            int requiredSize = qrVersion == 1 ? 152 : 272;
            int difference = requiredSize - bitStringLength;
            if (difference >= 4)
            {
                bitString += "0000";
            }
            else
            {
                bitString += new String('0', difference);
            }

            Console.WriteLine(bitString);

            int tailleModulo8 = bitString.Length % 8;
            if(tailleModulo8 != 0) bitString += new String('0', 8 - tailleModulo8);
            Console.WriteLine(bitString);

            Console.WriteLine($@"bitString.Length : {bitString.Length}");

            difference = requiredSize - bitString.Length;

            Console.WriteLine($@"difference : {difference}");
            int repeat = difference / 8;
            Console.WriteLine($@"repeat : {repeat}");
            bool kind = false;
            while (repeat != 0)
            {
                if (!kind)
                {
                    bitString += "11101100";
                }
                else
                {
                    bitString += "00010001";
                }

                repeat--;

                kind = !kind;
            }

            Console.WriteLine(bitString);


            //Etape 2 : Correction reed-solomon
            int numOfBytes = bitString.Length / 8;
            byte[] bytes = new byte[numOfBytes];
            for (int i = 0; i < numOfBytes; ++i)
            {
                bytes[i] = Convert.ToByte(bitString.Substring(8 * i, 8), 2);
                Console.WriteLine($@"{i} : {bytes[i]}");
            }

            Console.WriteLine(@"reed solomon : ");

            byte[] correction = ReedSolomonAlgorithm.Encode(bytes, qrVersion == 1?7:10, ErrorCorrectionCodeType.QRCode);
            for (int i = 0; i < correction.Length; i++)
            {
                Console.WriteLine($@"{i+35} : {correction[i]}");
                bitString += Convert.ToString(correction[i], 2).PadLeft(8, '0');
            }
            Console.WriteLine(bitString);

            int matrixSize = qrVersion == 1 ? 21 : 25;

            // init qr matrix
            Bgr[,] qrmatrix = new Bgr[matrixSize, matrixSize];
            // on rempli tout les pixels de rouge pour pouvoir voir plus facilement ceux qui n'ont pas encore été rempli

            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    qrmatrix[i, j] = new Bgr(0, 0, 255);
                }
            }
            // Génération des timers
            ApplyTimer(qrmatrix);

            // On génère les 3 marqueurs 
            // les offsets permettent de positionner correctement les 4 marqueurs
            ApplyQrMarkers(qrmatrix, 0, 0);
            ApplyQrMarkers(qrmatrix, 0, matrixSize-7);
            ApplyQrMarkers(qrmatrix, matrixSize - 7, matrixSize - 7);

            // module sombre obligatoire
            qrmatrix[matrixSize-1-((4 * qrVersion) + 9), 8] = new Bgr(0, 0, 0);

            ApplyEccAndVersionCode(qrmatrix, "111011111000100");
            if (qrVersion == 2)
            {
                ApplyAlignementMarker(qrmatrix);
            }
            
            WriteData(qrmatrix, bitString);
            ApplyMask(qrmatrix);

            MyImage qrFinal = new MyImage(qrmatrix, matrixSize, matrixSize);
            qrFinal.From_Image_To_File("./qr.bmp");
            Process.Start(@"C:\Users\antoi\AppData\Local\Programs\Microsoft VS Code\bin\code", @"C:\Users\antoi\Documents\PSI\PSI\bin\Debug\qr.bmp");


        }

        /// <summary>
        /// Génère un marqueur de positionnement pour QR code
        /// </summary>
        /// <param name="matrix">matrice de pixel</param>
        /// <param name="offsetx">décalage du marqueur en x</param>
        /// <param name="offsety">décalage du marqueur en y</param>
        private static void ApplyQrMarkers(Bgr[,] matrix, int offsetx, int offsety)
        {
            int matrixSize = matrix.GetLength(0);
            // ici rien de très intéressant, juste des boucles et des if pour générer deux carrés imbriqués.
            for (int y = -1 + offsety; y < 8 + offsety; y++)
            {
                for (int x = -1 + offsetx; x < 8 + offsetx; x++)
                {
                    if (x < 0 || y < 0 || x > matrixSize-1 || y > matrixSize - 1) continue;
                    
                    if (x > 6 + offsetx || y > 6 + offsety || x < 0 + offsetx || y < 0 + offsety)
                    {
                        matrix[y, x] = new Bgr(255, 255, 255);
                    }
                    else if ((x > 1 + offsetx && x < 5 + offsetx && y > 1 + offsety && y < 5 + offsety) || (y == 0 + offsety || y == 6 + offsety || x == 0 + offsetx || x == 6 + offsetx))
                    {
                        matrix[y, x] = new Bgr(0, 0, 0);
                    }
                    else
                    {
                        matrix[y, x] = new Bgr(255, 255, 255);
                    }
                }
            }
        }

        /// <summary>
        /// Génère les pixels de timer du QR code
        /// </summary>
        /// <param name="qrmatrix">matrice de pixel</param>
        private static void ApplyTimer(Bgr[,] qrmatrix)
        {
            int matrixSize = qrmatrix.GetLength(0);
            for (int i = 0; i < matrixSize; i++)
            {
                int color = i % 2 == 0 ? 0 : 255;
                Bgr pixel = new Bgr(color, color, color);
                qrmatrix[matrixSize-7, i] = pixel;
                qrmatrix[i, 6] = pixel;
            }
        }

        private static void ApplyEccAndVersionCode(Bgr[,] qrmatrix, String data)
        {
            int matrixSize = qrmatrix.GetLength(0);
            int color;
            Bgr pixel;
            for (int i = 0; i < 7; i++)
            {
                color = data[i] == '0' ? 255 : 0;
                pixel = new Bgr(color, color, color);
                qrmatrix[i, 8] = pixel;
                if (i < 6) qrmatrix[matrixSize - 9, i] = pixel;
            }
            for (int i = 14; i > 5; i--)
            {
                color = data[i] == '0' ? 255 : 0;
                pixel = new Bgr(color, color, color);
                if (i > 6) qrmatrix[matrixSize - 9, matrixSize - 15 + i] = pixel;
                if (i > 8) qrmatrix[matrixSize - 15 + i, 8] = pixel;
                else switch (i)
                {
                    case 8:
                        qrmatrix[matrixSize - 8, 8] = pixel;
                        break;
                    case 7:
                        qrmatrix[matrixSize - 9, 8] = pixel;
                        break;
                    case 6:
                        qrmatrix[matrixSize - 9, 7] = pixel;
                        break;
                }
            }
        }

        private static void ApplyAlignementMarker(Bgr[,] qrmatrix)
        {
            for (int y = 4; y < 9; y++)
            {
                for (int x =20; x > 15; x--)
                {

                    if (x == 20 || x == 16 || y == 4 || y == 8)
                    {
                        qrmatrix[y, x] = new Bgr(0, 0, 0);
                    }
                    else if (x == 18 && y == 6)
                    {
                        qrmatrix[y, x] = new Bgr(0, 0, 0);
                    }
                    else
                    {
                        qrmatrix[y, x] = new Bgr(255, 255, 255);
                    }
                    
                }
            }
        }

        private static void WriteData(Bgr[,] qrmatrix, String data)
        {
            int matrixSize = qrmatrix.GetLength(0);
            bool bottomToTop = true;
            int offsetx = 0;
            int n = 0;
            int topMax = matrixSize - 9;
            int bottomMin = 0;
            int rightMax = matrixSize - 1;
            int rows = (matrixSize -1)/2;
            for (int i = 0; i < rows; i++)
            {
                for (int y = bottomToTop ? bottomMin : topMax-1; bottomToTop ? y < topMax : y >= bottomMin; y += bottomToTop ? 1 : -1)
                {
                    for (int x = rightMax - offsetx;
                         x > rightMax - 2 - offsetx;
                        x += -1)
                    {
                        int color;
                        try { color = (data[n] == '0') ? 255 : 0;}
                        catch (Exception e)
                        {
                            color = 255;
                        }
                        Bgr pixel = new Bgr(color, color, color);
                        if (i == 4 && matrixSize == 25)
                        {
                            if (y >= 4 && y < 9 && x== rightMax - 1 - offsetx)
                            {
                                qrmatrix[y, x] = pixel;
                                n++;
                            }
                            else if (y < 4 || y >=9)
                            {
                                if (y > matrixSize - 8)
                                {
                                    qrmatrix[y+1, x] = pixel;
                                }
                                else
                                {
                                    qrmatrix[y, x] = pixel;
                                }
                                n++;
                            }
                        }
                        else if (y >= 4 && x<=20 && x>15 && matrixSize == 25)
                        {
                            if(y+5 >= topMax) continue;
                            qrmatrix[y + 5, x] = pixel;
                            n++;
                        }
                        else if (y > matrixSize - 8)
                        {
                            qrmatrix[y+1, x] = pixel;
                            n++;
                        }
                        else if (x < 7)
                        {
                            qrmatrix[y, x-1] = pixel;
                            n++;
                        }
                        else
                        {
                            qrmatrix[y, x] = pixel;
                            n++;
                        }
                    }
                }
                bottomToTop = !bottomToTop;
                offsetx += 2;
                if(matrixSize == 25)
                {
                    if (i >= 7)
                    {
                        topMax = matrixSize - 9;
                        bottomMin = 8;
                    }
                    else if (i >= 3)
                    {
                        topMax = matrixSize - 1;
                    }
                }
                else
                {
                    if (i > 4)
                    {
                        topMax = 12;
                        bottomMin = 8;
                    }
                    else if (i > 2) topMax = 20;
                }
                
            }

        }

        private static void ApplyMask(Bgr[,] qrmatrix)
        {
            int matrixsize = qrmatrix.GetLength(0);
            for (int y = 0; y < matrixsize; y++)
            {
                for (int x = 0; x < matrixsize; x++)
                {
                    if (matrixsize == 25 && y > 3 && y < 9 && x > 15 && x < 21 ||
                        x < 9 && (y < 8 || y > matrixsize - 10) || (y == matrixsize - 7 || x == 6) ||
                        x > matrixsize - 9 && y > matrixsize - 10) continue;
                    int color = (qrmatrix[y, x].R == 255) ^ ((y + x) % 2 == 0) ? 255 : 0;
                    Bgr pixel = new Bgr(color, color, color);
                    qrmatrix[y, x] = pixel;

                }
            }
        }


        #endregion

        #endregion
    }
}
