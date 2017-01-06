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
            ms._flag = false;
            ms._age = 10;
            ms._id = 10;
            ms._size = 10;
            ms._height = 10.1f;
            ms._width = 10.2d;
            ms._depth = 10.3m;
            ms._character = 'a';
            ms._pos = new Point(14, 16);
            ms._rect = new Rectangle(10, 20, 30, 40);

            ms.SaveXML();
            ms.LoadXML();

            Console.WriteLine(ms._depth);

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

            ms.SaveXML();

        }


    }



}
