namespace TD_2
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
    readonly struct Bgr
    {
        /// <summary>
        /// Pixel de couleurs BGR (Blue Green Red)
        /// </summary>
        /// <param name="b">valeur de la couleur bleu [0-255]</param>
        /// <param name="g">valeur de la couleur verte [0-255]</param>
        /// <param name="r">valeur de la couleur rouge [0-255]</param>
        public Bgr(int b, int g, int r)
        {
            R = r;
            G = g;
            B = b;
        }
        
        /// <summary>
        /// valeur de la couleur rouge du pixel
        /// </summary>
        public int R { get; }
        /// <summary>
        /// valeur de la couleur verte du pixel
        /// </summary>
        public int G { get; }
        
        /// <summary>
        /// valeur de la couleur bleu du pixel
        /// </summary>
        public int B { get; }
    }
}
