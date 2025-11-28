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
    public partial class ViewDEV : Form
    {
        private readonly int devId;

        public ViewDEV()
        {
            InitializeComponent();
        }

        public ViewDEV(int devId) : this()
        {
            this.devId = devId;
            if (devId > 0)
            {
                // TODO: Load DEV details
            }
        }
    }
}
