using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Schema;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Wildfire
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            scale = ClientRectangle.Width / 960;
            CamPosX = 0; 
            CamPosY = 0; 
            chunks[n] = new Chunk(-64, -64);
            chunks[n++].generate();
            Refresh();
            timer1.Start();
            hScrollBar1.Location = new Point(108, ClientRectangle.Height - 96 - 20);
            label1.Location = new Point(0, ClientRectangle.Height - 96 - 20);
            button1.Location = new Point(0, ClientRectangle.Height - 96 - 20 - 25);
            MouseWheel += Form1_MouseWheel;
        }

        #region defines
        float scale;
        Chunk[] chunks = new Chunk[1024];
        int n = 0;
        float CamPosX, CamPosY;
        int v = 50;
        float brushRange = 25;
        float mouseX = 720, mouseY = 405;

        Thread MainThread = Thread.CurrentThread;
        Thread OtherThread;

        Image[] tools_images =
        {
            Image.FromFile("Tools.png"), Image.FromFile("Tools_Tree.png"), Image.FromFile("Tools_Destroy.png"),
            Image.FromFile("Tools_Fire.png"), Image.FromFile("Tools_Flush.png")
        };
        int tin = 0; //tools images number
        #endregion

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Green);
            for (int i = 0; i < n; i++)
            {
                chunks[i].Draw(e.Graphics, scale, CamPosX, CamPosY);
            }
            //e.Graphics.DrawEllipse(Pens.Black, ClientRectangle.Width * 0.475f, ClientRectangle.Height * 0.475f, ClientRectangle.Width * 0.05f, ClientRectangle.Height * 0.05f);
            e.Graphics.DrawImage(tools_images[tin], 0, ClientRectangle.Height - 96, 256, 96);
            //e.Graphics.DrawEllipse(Pens.Black, mouseX - brushRange, mouseY - brushRange, brushRange * 2, brushRange * 2);
        }

        #region gameplay
        public int fsr = 2; //fire spread range
        public int fss = 6; //fire spread speed
        public int tlt = 15; //tree life time
        public int windX = 0; //wind in X-axis
        public int windY = 0; //wind in Y-axis
        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < chunks[k].n; i++)
                {
                    for (int j = 0; j < chunks[k].n; j++)
                    {
                        switch(chunks[k].tiles[i, j].fire)
                        {
                            case int n when (n > 0 && n < fss) || (n > fss && n < tlt):
                                chunks[k].tiles[i, j].fire++; 
                                break;
                            case int n when n == fss:
                                chunks[k].tiles[i, j].fire++;
                                for (int i1 = Math.Min(-fsr + windX, 0); i1 <= Math.Max(fsr + windX, 0); i1++)
                                {
                                    for (int j1 = Math.Min(-fsr + windY, 0); j1 <= Math.Max(fsr + windY, 0); j1++)
                                    {
                                        Chunk c = chunks[k];
                                        int diffx = 0;
                                        int diffy = 0;

                                        if (i + i1 < 0)
                                        {
                                            c = c.links[2];
                                            if (c == null) { c.generate(); Chunk.Link(ref c, chunks, n); }
                                            diffx = c.n;
                                        }
                                        else if (i + i1 >= c.n)
                                        {
                                            diffx = -c.n;
                                            c = c.links[0];
                                            if (c == null) { c.generate(); Chunk.Link(ref c, chunks, n); }
                                        }
                                        if (j + j1 < 0)
                                        {
                                            c = c.links[1];
                                            if (c == null) { c.generate(); Chunk.Link(ref c, chunks, n); }
                                            diffy = c.n;
                                        }
                                        else if(j + j1 >= c.n)
                                        {
                                            diffy = -c.n;
                                            c = c.links[3];
                                            if (c == null) { c.generate(); Chunk.Link(ref c, chunks, n); }
                                        }
                                        if (c.tiles[i + i1 + diffx, j + j1 + diffy].tree && c.tiles[i + i1 + diffx, j + j1 + diffy].fire == 0)
                                            c.tiles[i + i1 + diffx, j + j1 + diffy].fire = tlt + 1;
                                    }
                                }
                                break;
                            case int n when n == tlt:
                                chunks[k].tiles[i, j].tree = false;
                                chunks[k].tiles[i, j].fire = 0;
                                break;
                            case int n when n == tlt + 1: chunks[k].tiles[i, j].fire = 1; break;
                            default: break;
                        }
                    }
                }
            }
            Refresh();
        }
        #endregion

        private void Form1_Resize(object sender, EventArgs e)
        {
            Tile.XpicMax = ClientRectangle.Width;
            Tile.YpicMax = ClientRectangle.Height;

            hScrollBar1.Location = new Point(108, ClientRectangle.Height - 96 - 20);
            label1.Location = new Point(0, ClientRectangle.Height - 96 - 20);
            button1.Location = new Point(0, ClientRectangle.Height - 96 - 20 - 25);
        }

        public void LoadNecessaryChunks()
        {
            Chunk origin = null;
            for (int i = 0; i < n; i++)
            {
                Chunk c = chunks[i];
                Tile t1 = c.tiles[0, 0];
                float t1x = t1.X(scale, CamPosX);
                float t1y = t1.Y(scale, CamPosY);
                Tile tn = c.tiles[c.n - 1, c.n - 1];
                float tnx = tn.X(scale, CamPosX) + Tile.size * scale;
                float tny = tn.Y(scale, CamPosY) + Tile.size * scale;
                int cw05 = ClientRectangle.Width / 2;
                int ch05 = ClientRectangle.Height / 2;
                if (t1x < cw05 && tnx > cw05 && t1y < ch05 && tny > ch05) origin = chunks[i];
            }
            float chunks_visible = ClientRectangle.Width / (chunks[0].tiles[chunks[0].n - 1, 0].X(scale, CamPosX) + Tile.size * scale - chunks[0].tiles[0, 0].X(scale, CamPosX));
            int cv = (int)Math.Ceiling(chunks_visible);
            origin.Load_chunks(cv, cv, new List<Chunk>(), chunks, ref n);
        }

        #region keys
        bool speedMoving = false;
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left: CamPosX -= v / scale; break;
                case Keys.Right: CamPosX += v / scale; break;
                case Keys.Up: CamPosY -= v / scale; break;
                case Keys.Down: CamPosY += v / scale; break;
                case Keys.Add:
                    CamPosX += 0.05f * ClientRectangle.Width / scale;
                    CamPosY += 0.05f * ClientRectangle.Height / scale;
                    scale *= 10f / 9;
                    break;
                case Keys.Subtract:
                    CamPosX -= 0.05f * ClientRectangle.Width / scale;
                    CamPosY -= 0.05f * ClientRectangle.Height / scale;
                    scale *= 10f / 11;
                    break;
                case Keys.ControlKey: speedMoving = true; v *= 10; break;
                case Keys.Escape: tin = 0; break;
                default:
                    MessageBox.Show(e.KeyCode.ToString());
                    break;
            }

            LoadNecessaryChunks();

            Refresh();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey: speedMoving = false; v /= 10; break;
                default:
                    break;
            }
        }
        #endregion

        #region mouse
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                CamPosX += 0.05f * ClientRectangle.Width / scale;
                CamPosY += 0.05f * ClientRectangle.Height / scale;
                scale *= 10f / 9;
            }
            else
            {
                CamPosX -= 0.05f * ClientRectangle.Width / scale;
                CamPosY -= 0.05f * ClientRectangle.Height / scale;
                scale *= 10f / 11;
            }

            LoadNecessaryChunks();

            Refresh();
        }

        bool lmbid = false; //left mouse button is down
        bool rmbid = false; //right mouse button is down
        bool mmrn = false; //mouse move Refresh() needed
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    lmbid = true;
                    OtherThread = new Thread(() => mouseEffect(e.X, e.Y));
                    OtherThread.Start();
                    if (mmrn) { Refresh(); mmrn = false; }
                    break;
                case MouseButtons.Right:
                    rmbid = true;
                    break;
                default: 
                    break;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (rmbid)
            {
                CamPosX += (mouseX - e.X) / scale;
                CamPosY += (mouseY - e.Y) / scale;
                LoadNecessaryChunks();
                Refresh();
            }
            mouseX = e.X;
            mouseY = e.Y;
            //Refresh();
            if(lmbid) 
            { 
                OtherThread = new Thread(() => mouseEffect(e.X, e.Y)); 
                OtherThread.Start();
                if (mmrn) { Refresh(); mmrn = false; }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    lmbid = false;
                    break;
                case MouseButtons.Right:
                    rmbid = false;
                    break;
                default:
                    break;
            }
        }

        public void mouseEffect(float X, float Y)
        {
            if (X < 256 && Y > ClientRectangle.Height - 96)
            {
                if (X < 64) if (tin != 1) tin = 1; else tin = 0;
                else if (X < 128) if (tin != 2) tin = 2; else tin = 0;
                else if (X < 196) if (tin != 3) tin = 3; else tin = 0;
                else { if (tin != 4) tin = 4; else tin = 0; }
                mmrn = true;
            }
            else
            {
                //Graphics g = CreateGraphics();
                float s = Tile.size * scale;
                for (int k = 0; k < n; k++)
                {
                    for (int i = 0; i < chunks[k].n; i++)
                    {
                        for (int j = 0; j < chunks[k].n; j++)
                        {
                            float xx = chunks[k].tiles[i, j].X(scale, CamPosX);
                            float yy = chunks[k].tiles[i, j].Y(scale, CamPosY);
                            //g.DrawRectangle(Pens.Red, xx, yy, s, s);
                            if (xx < X && xx + s > X && yy < Y && yy + s > Y)
                            {
                                int ibr = (int)Math.Ceiling(brushRange / Tile.size / scale); //int brushRange
                                for (int i1 = -ibr; i1 <= ibr; i1++)
                                {
                                    for (int j1 = -ibr; j1 <= ibr; j1++)
                                    {
                                        if (i1 * i1 + j1 * j1 <= ibr * ibr)
                                        {
                                            Chunk c = chunks[k];
                                            int diffx = 0;
                                            int diffy = 0;

                                            if (i + i1 < 0)
                                            {
                                                c = c.links[2];
                                                diffx = c.n;
                                            }
                                            else if (i + i1 >= c.n)
                                            {
                                                diffx = -c.n;
                                                c = c.links[0];
                                            }
                                            if (j + j1 < 0)
                                            {
                                                c = c.links[1];
                                                diffy = c.n;
                                            }
                                            else if (j + j1 >= c.n)
                                            {
                                                diffy = -c.n;
                                                c = c.links[3];
                                            }

                                            switch (tin)
                                            {
                                                case 1:
                                                    c.tiles[i + i1 + diffx, j + j1 + diffy].tree = true;
                                                    break;
                                                case 2:
                                                    if (c.tiles[i + i1 + diffx, j + j1 + diffy].tree) c.tiles[i + i1 + diffx, j + j1 + diffy].tree = false;
                                                    break;
                                                case 3:
                                                    if (c.tiles[i + i1 + diffx, j + j1 + diffy].tree && c.tiles[i + i1 + diffx, j + j1 + diffy].fire == 0) c.tiles[i + i1 + diffx, j + j1 + diffy].fire = 1;
                                                    break;
                                                case 4:
                                                    if (c.tiles[i + i1 + diffx, j + j1 + diffy].fire != 0) c.tiles[i + i1 + diffx, j + j1 + diffy].fire = 0;
                                                    break;
                                            }
                                            //g.DrawRectangle(Pens.Blue, xx, yy, s, s);
                                        }
                                    }
                                }
                                mmrn = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            brushRange = hScrollBar1.Value;
        }

        #region Form2
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            new Form2(this).Show();
        }
        public void ClosingOfForm2(int fsr, int fss, int tlt, int windX, int windY)
        {
            timer1.Start();
            this.fsr = fsr;
            this.fss = fss;
            this.tlt = tlt;
            this.windX = windX;
            this.windY = windY;
        }
        #endregion
    }
}