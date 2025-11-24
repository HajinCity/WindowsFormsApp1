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
    public partial class IARForm : Form
    {
        public IARForm()
        {
            InitializeComponent();
            addIARbtn.Click += AddIARbtn_Click;
        }

        private void AddIARbtn_Click(object sender, EventArgs e)
        {
            AddIARForm addIARForm = new AddIARForm();
            addIARForm.ShowDialog();
        }
    }
}
