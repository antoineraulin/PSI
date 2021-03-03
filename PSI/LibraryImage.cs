using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI
{
    /// <summary>
    /// Pixel de couleurs BGR (Blue Green Red)
    /// </summary>
    /// <remarks>
    /// On utilise ici une struct plutôt qu'une class parce qu'on a pas besoin de méthodes ici,
    /// et surtout parce qu'une classe est passé par référence alors qu'un struct est passé par une copie ce qui peut éviter certains problèmes
    /// </remarks>
    /// <remarks>
    /// Notre pixel est de type BGR et non pas RGB parce qu'en BMP les pixels sont codés de cette manière
    /// </remarks>
    readonly struct LibraryImage
    {
       
        public LibraryImage(String imageUri, String name, String size)
        {
            ImageUri = imageUri;
            Name = name;
            Size = size;
        }

        public String ImageUri { get; }
 
        public String Name { get; }

        public String Size { get; }
    }
}
