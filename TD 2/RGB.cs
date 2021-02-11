using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TD_2
{
    struct RGB
    {
        private int r, g, b;

        public RGB(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public int R
        {
            get { return r; }
        }


        public int G
        {
            get { return g; }
        }

        public int B
        {
            get { return b; }
        }
    }
}
