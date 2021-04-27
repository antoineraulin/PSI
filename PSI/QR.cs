using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI
{
    class QR
    {
        #region private




        #endregion

        #region public

        public void encodage(String valeur)
        {
            Encoding ascii = Encoding.ASCII;
            byte[] res = ascii.GetBytes(valeur);
            
            /*valeur = valeur.ToUpper();
            byte[] encode = new byte[valeur.Length];
            for (int i = 0; i < valeur.Length; i++)
            {
                if (Char.IsDigit(valeur[i]))//on teste si le caractère est un int
                {
                    encode[i] = (byte)int.Parse(valeur[i].ToString());//on convertit le char en string, puis en int et enfin en byte

                }
                else if (Char.IsLetter(valeur[i]))
                {
                    encode[i] = (byte)((int)(Convert.ToByte(valeur[i])) - 55);//car si on convertit direct, on a A=65 et on veut A=10
                                                                              //on le convertit en int pour pouvoir lui soustraire 55
                    Console.WriteLine(encode[i]);
                }
                else
                  if (valeur[i] == '$')
                {
                    encode[i] = 37;
                }
                else if(valeur[i]==' ')
                {
                    encode[i]  =  36;
                }
                else if (valeur[i] == '%')
                {
                    encode[i] = 38;
                }
                else if (valeur[i] == ':')
                {
                    encode[i] = 44;
                }
                else if (valeur[i] == '*')
                {
                    encode[i] = 39;
                }
                else if (valeur[i] == '+')
                {
                    encode[i] = 40;
                }
                else if (valeur[i] == '-')
                {
                    encode[i] = 41;
                }
                else if (valeur[i] == '.')
                {
                    encode[i] = 42;
                }
                else if (valeur[i] == '/')
                {
                    encode[i] = 43;
                }
                else
                {
                    throw new ArgumentException("La phrase n'est pas traduisible", "valeur");
                }*/
                //Autres caractères spéciaux
            }
        }
        /*public void correction(byte[] encodage)
        {
            int taille  =  encodage.Length;
            taille  =  (byte)taille;

        }*/

        #endregion
    
}
