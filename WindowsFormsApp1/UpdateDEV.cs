using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class UpdateDEV : Form
    {
        private readonly int devId;

        public UpdateDEV()
        {
            InitializeComponent();
        }

        public UpdateDEV(int devId) : this()
        {
            this.devId = devId;
            if (devId > 0)
            {
                // TODO: Load DEV details
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
