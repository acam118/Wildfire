using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wildfire
{
    public partial class Form2 : Form
    {
        public Form2(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;
            numericUpDown1.Value = form1.fsr;
            numericUpDown2.Value = form1.fss;
            numericUpDown3.Value = form1.tlt;
            numericUpDown4.Value = form1.windX;
            numericUpDown5.Value = form1.windY;
        }
        Form1 form1;

        private void button1_Click(object sender, EventArgs e)
        {
            int fsr = (int)numericUpDown1.Value;
            int fss = (int)numericUpDown2.Value;
            int tlt = (int)numericUpDown3.Value;
            int windX = (int)numericUpDown4.Value;
            int windY = (int)numericUpDown5.Value;
            form1.ClosingOfForm2(fsr, fss, tlt, windX, windY);
            Close();
        }
    }
}
