using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace XMLReflection
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        MySetting ms = new MySetting();

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            ms.flag = false;
            ms.age = 10;
            ms.id = 10;
            ms.size = 10;
            ms.height = 10.1f;
            ms.width = 10.2d;
            ms.depth = 10.3m;
            ms.character = 'a';
            ms.pos = new Point(14, 16);
            ms.rect = new Rectangle(10, 20, 30, 40);

            ms.SaveXML();
            ms.LoadXML();

            Console.WriteLine(ms.depth);

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

            ms.SaveXML();

        }


    }



}
