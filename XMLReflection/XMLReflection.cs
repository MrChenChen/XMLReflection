using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLReflection
{

    public class XMLBase
    {

        public static readonly Assembly mCoreLib = Assembly.Load("mscorlib");
        public static readonly Assembly mCurrLib = Assembly.GetExecutingAssembly();

        const int STRING = 0;
        const int PRIMARY = 1;
        const int EXTENTION = 2;

        public string mSavePath = string.Empty;

        public XMLBase()
        {
            mSavePath = "D:\\" + GetType().Name + ".xml";
        }

        public static int GetTypeFlag(Type t)
        {
            return (t == typeof(string)) ?
                STRING
                :
                (t.IsPrimitive || t == typeof(decimal))
                ?
                PRIMARY
                :
                EXTENTION;
        }

        public void LoadXML()
        {
            if (mSavePath == string.Empty) throw new Exception("XMLBase SavePath is Empty");

            XElement xml = XElement.Load(mSavePath);

            var member = GetType().GetFields();

            foreach (var item in member)
            {
                var elem = xml.Element(item.Name);

                MethodInfo parse = null;

                switch (GetTypeFlag(item.FieldType))
                {
                    case STRING:
                        {
                            item.SetValue(this, elem.Value);
                            break;
                        }
                    case PRIMARY:
                        {
                            parse = mCoreLib.CreateInstance("System." + item.FieldType.Name).GetType().GetMethod("Parse", new Type[] { typeof(string) });

                            item.SetValue(this, parse.Invoke(null, new object[] { elem.Value }));

                            break;
                        }
                    case EXTENTION:
                        {
                            parse = mCurrLib.GetTypes().Where(x => x.Name.Contains(item.FieldType.Name + "_Extention")).First().GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);

                            item.SetValue(this, parse.Invoke(null, new object[] { null, elem.Value }));
                            break;
                        }
                    default:
                        break;
                }

            }
        }

        public void SaveXML()
        {
            XElement xml = new XElement(GetType().Name);

            var mem = GetType().GetFields();

            foreach (var item in mem)
            {
                if (item.IsStatic) continue;

                XElement xe = new XElement(item.Name);

                xe.SetValue(item.GetValue(this).ToString());

                xml.Add(xe);

            }

            xml.Save("D:\\" + GetType().Name + ".xml");

        }

    }

    public class MySetting : XMLBase
    {
        //string

        public string name = string.Empty;


        //基元类型

        public bool flag = true;

        public byte age = 0;

        public int id = 0;

        public char character = '0';

        public long size = 0L;

        public float height = 0f;

        public double width = 0d;

        public decimal depth = 0m;


        //结构和类

        public Rectangle rect = new Rectangle();

        public Point pos = new Point();

        public DateTime date = DateTime.Now;


    }

}

namespace Extention
{


    public static class Rectangle_Extention
    {
        public static Rectangle Parse(this Rectangle obj, string text)
        {
            text = text.Replace("}", "");

            obj.X = int.Parse(text.Split(',')[0].Split('=')[1]);
            obj.Y = int.Parse(text.Split(',')[1].Split('=')[1]);
            obj.Width = int.Parse(text.Split(',')[2].Split('=')[1]);
            obj.Height = int.Parse(text.Split(',')[3].Split('=')[1]);

            return obj;
        }
    }

    public static class RectangleF_Extention
    {
        public static RectangleF Parse(this RectangleF obj, string text)
        {
            text = text.Replace("}", "");

            obj.X = float.Parse(text.Split(',')[0].Split('=')[1]);
            obj.Y = float.Parse(text.Split(',')[1].Split('=')[1]);
            obj.Width = float.Parse(text.Split(',')[2].Split('=')[1]);
            obj.Height = float.Parse(text.Split(',')[3].Split('=')[1]);

            return obj;
        }
    }

    public static class Point_Extention
    {
        public static Point Parse(this Point obj, string text)
        {
            text = text.Replace("}", "");

            obj.X = int.Parse(text.Split(',')[0].Split('=')[1]);
            obj.Y = int.Parse(text.Split(',')[1].Split('=')[1]);

            return obj;
        }
    }

    public static class PointF_Extention
    {
        public static PointF Parse(this PointF obj, string text)
        {
            text = text.Replace("}", "");

            obj.X = float.Parse(text.Split(',')[0].Split('=')[1]);
            obj.Y = float.Parse(text.Split(',')[1].Split('=')[1]);

            return obj;
        }
    }

    public static class DateTime_Extention
    {
        public static DateTime Parse(this DateTime obj, string text)
        {
            return Convert.ToDateTime(text);
        }
    }


}
