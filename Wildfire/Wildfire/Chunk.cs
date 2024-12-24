using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildfire
{
    public class Chunk
    {
        static int size = 128;
        int x, y;
        public Tile[,] tiles;
        public int n;
        public Chunk[] links = new Chunk[4];
        public Chunk(int x, int y)
        {
            this.x = x;
            this.y = y;
            tiles = new Tile[size,size];
            n = size;
            links = new Chunk[4];
        }
        public void generate()
        {
            Random R = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    tiles[i,j] = new Tile(x + i, y + j);
                }
            }
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double r = R.NextDouble();
                    double sot = 0.1; //scarcity of trees
                    double fc = 0.5; // forests coefficient
                    if ((i > 0 && tiles[i - 1, j].tree) ||
                        (i < size - 1 && tiles[i + 1, j].tree) ||
                        (j > 0 && tiles[i, j - 1].tree) ||
                        (j < size - 1 && tiles[i, j + 1].tree))
                    {
                        sot = fc;
                    }
                    if (r < sot) tiles[i, j].tree = true;

                    /*
                    double sot = 0.99; //scarcity of trees
                    double fc = 2.5; // forests coefficient
                    if (i > 0 && tiles[i - 1, j].tree) sot /= fc;
                    if (i < size - 1 && tiles[i + 1, j].tree) sot /= fc;
                    if (j > 0 && tiles[i, j - 1].tree) sot /= fc;
                    if (j < size - 1 && tiles[i, j + 1].tree) sot /= fc;
                    if (r > sot) tiles[i, j].tree = true;
                    */

                    //------------------------------------------------ chunk splitting not aknowleged
                }
            }
        }
        public void Load_chunks(int numx, int numy, List<Chunk> a, Chunk[] chunks, ref int chunks_n)
        {
            foreach (Chunk c in a)
            {
                if (this == c) return;
            }
            a.Add(this);
            if(numx > 0)
            {
                for(int i = 0;i <= 2; i += 2)
                {
                    if (links[i] != null) links[i].Load_chunks(numx - 1, numy, a, chunks, ref chunks_n);
                    else
                    {
                        if(i == 0) links[i] = new Chunk(x + size, y);
                        else links[i] = new Chunk(x - size, y);
                        links[i].generate();
                        links[i].links[2 - i] = this;
                        Link(ref links[i], chunks, chunks_n);
                        chunks[chunks_n++] = links[i];
                        links[i].Load_chunks(numx - 1, numy, a, chunks, ref chunks_n);
                    }
                }
            }
            if(numy > 0)
            {
                for (int i = 1; i <= 3; i += 2)
                {
                    if (links[i] != null) links[i].Load_chunks(numx, numy - 1, a, chunks, ref chunks_n);
                    else
                    {
                        if (i == 1) links[i] = new Chunk(x, y - size);
                        else links[i] = new Chunk(x, y + size);
                        links[i].generate();
                        links[i].links[2 - (i - 1) + 1] = this;
                        Link(ref links[i], chunks, chunks_n);
                        chunks[chunks_n++] = links[i];
                        links[i].Load_chunks(numx, numy - 1, a, chunks, ref chunks_n);
                    }
                }
            }
        }
        public static void Link(ref Chunk c, Chunk[] chunks, int chunks_n)
        {
            for (int i = 0; i < chunks_n; i++)
            {
                if (chunks[i].x == c.x + size && chunks[i].y == c.y) { if (c.links[0] == null) { c.links[0] = chunks[i]; chunks[i].links[2] = c; } }
                if (chunks[i].x == c.x && chunks[i].y == c.y - size) { if (c.links[1] == null) { c.links[1] = chunks[i]; chunks[i].links[3] = c; } }
                if (chunks[i].x == c.x - size && chunks[i].y == c.y) { if (c.links[2] == null) { c.links[2] = chunks[i]; chunks[i].links[0] = c; } }
                if (chunks[i].x == c.x && chunks[i].y == c.y + size) { if (c.links[3] == null) { c.links[3] = chunks[i]; chunks[i].links[1] = c; } }
            }
        }
        public void Draw(Graphics g, float scale, float dx, float dy)
        {
            /*
            float side = tiles[n - 1, n - 1].X(scale, dx) + Tile.size * scale - tiles[0, 0].X(scale, dx);
            g.DrawRectangle(Pens.Black, tiles[0, 0].X(scale, dx), tiles[0, 0].Y(scale, dy), side, side);
            */
            for(int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    tiles[i,j].Draw(g, scale, dx, dy);
                }
            }
        }
    }
}
