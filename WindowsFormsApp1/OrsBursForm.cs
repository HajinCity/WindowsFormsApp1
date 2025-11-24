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
    public partial class OrsBursForm : Form
    {
        public OrsBursForm()
        {
            InitializeComponent();
            addEntryBtn.Click += AddEntryBtn_Click;
        }

        private void AddEntryBtn_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddORSBURS())
            {
                addForm.ShowDialog(this);
            }
        }
    }
}
