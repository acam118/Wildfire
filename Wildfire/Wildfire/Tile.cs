using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wildfire
{
    public class Tile
    {
        public int x, y;
        public static int size = 32;
        public bool tree;
        public int fire;
        static Image[] pictures = {
            Image.FromFile("Tree.png"),
            Image.FromFile("Tree Fire.png"),
            Image.FromFile("Tree Fire 2.png"),
            Image.FromFile("Tree Fire 3.png")
        };
        static public int XpicMax = 1440, YpicMax = 810;

        public Tile(int x, int y, bool tree = false, int fire = 0)
        {
            this.x = x;
            this.y = y;
            this.tree = tree;
            this.fire = fire;
        }
        public float X(float scale, float dx)
        {
            return (x * size - dx) * scale;
        }
        public float Y(float scale, float dy)
        {
            return (y * size - dy) * scale;
        }
        public float Y_pic(float scale, float dy)
        {
            return (y * size - dy + size - 64) * scale;
        }
        public void Draw(Graphics G, float scale, float dx, float dy)
        {
            if (tree)
            {
                float Xpic = X(scale, dx);
                if (Xpic < -scale * 64 || Xpic > XpicMax) return;
                float Ypic = Y_pic(scale, dy);
                if (Ypic < -scale * 64 || Ypic > YpicMax) return; 
                Image image;
                switch (fire)
                {
                    case 0: image = pictures[0]; break;
                    case int n when n % 3 == 1: image = pictures[1]; break;
                    case int n when n % 3 == 2: image = pictures[2]; break;
                    case int n when n % 3 == 0: image = pictures[3]; break;
                    default: image = pictures[0]; break;
                }
                G.DrawImage(image, Xpic, Ypic, scale * 64, scale * 64);
                //float Ypic1 = Y(scale, dy);
                //if (Ypic1 < -scale * 64 || Ypic1 > 960) return;
                //G.DrawRectangle(Pens.Black, Xpic, Ypic1, scale * 64 / 2, scale * 64 / 2);

                //--------------------------------------------------------------------when scale is too small draw simplified images
                //--------------------------------------------------------------------this may not even work really, I need GPU bruh
                if (scale > 0.125)
                {
                    //normal
                }
                else
                {
                    //idk how to make simplified images
                }
            }
        }
    }
}
