using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp5
{
    public partial class PrintEDP : Form
    {
        public PrintEDP()
        {
            InitializeComponent();
        }

        private void btnOpenCondition_Click(object sender, EventArgs e)
        {
            using (var form = new Conditional())
            {
                form.ShowDialog(this);
            }
        }
    }
}
