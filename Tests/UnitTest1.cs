using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSI;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CheckImportExportImages()
        {
            String filename = @".\fox.bmp";
            MyImage fox = new MyImage(filename);
            fox.From_Image_To_File(@".\foxOut.bmp");
            Assert.IsTrue(AssertCompareFiles(filename,@".\foxOut.bmp" ));
        }

        // Traitement d'image

        // Nuance de gris
        [TestMethod]
        public void CheckGrayscaleFilter()
        {
            String filename = @".\fox.bmp";
            MyImage fox = new MyImage(filename);
            fox = fox.ToGrayscale();
            fox.From_Image_To_File(@".\foxOut.bmp");
            Assert.IsTrue(AssertCompareFiles(@".\foxGray.bmp",@".\foxOut.bmp" ));
        }

        private bool AssertCompareFiles(string filename1, string filename2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Si le même fichier est entré deux fois ils sont forcément égaux.
            if (filename1 == filename2)
            {
                return true;
            }

            // On ouvre les deux fichiers
            fs1 = new FileStream(filename1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(filename2, FileMode.Open, FileAccess.Read);

            // On vérifie la taille des fichiers, s'ils n'ont pas la même taille, ils sont forcèments inégaux.
            if (fs1.Length != fs2.Length)
            {
                fs1.Close();
                fs2.Close();

                return false;
            }

            // On vérifie bit par bit s'ils sont égaux, dès qu'un des bits ne correspond pas c'est que les fichiers sont différents.
            // Peut être lent si on compare deux très gros fichiers.
            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            fs1.Close();
            fs2.Close();

            // file2byte est égal a file2byte seulement s'ils sont égaux
            return ((file1byte - file2byte) == 0);
        }
    }


}
