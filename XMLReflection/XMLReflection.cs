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

        const string STRING = "S";
        const string PRIMARY = "P";
        const string EXTENTION = "E";

        public void LoadXML()
        {
            XElement xml = XElement.Load("D:\\" + GetType().Name + ".xml");

            var member = GetType().GetFields();

            foreach (var item in member)
            {
                var elem = xml.Element(item.Name);

                if (elem != null)
                {
                    var attrValue = elem.Attribute("Type").Value;

                    var attrType = attrValue.Split('_')[0];

                    if (attrValue != null)
                    {
                        MethodInfo parse = null;

                        if (attrValue.EndsWith(STRING))
                        {
                            item.SetValue(this, elem.Value);
                        }
                        else if (attrValue.EndsWith(EXTENTION))
                        {
                            parse = mCurrLib.GetTypes().Where(x => x.Name.Contains(attrType + "_Extention")).First().GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);

                            item.SetValue(this, parse.Invoke(null, new object[] { null, elem.Value }));
                        }
                        else if (attrValue.EndsWith(PRIMARY))
                        {
                            parse = mCoreLib.CreateInstance("System." + attrType).GetType().GetMethod("Parse", new Type[] { typeof(string) });

                            item.SetValue(this, parse.Invoke(null, new object[] { elem.Value }));
                        }
                    }
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


                if (item.FieldType.IsPrimitive || item.FieldType == typeof(decimal))
                {
                    xe.SetAttributeValue("Type", item.FieldType.Name + "_" + PRIMARY);
                }
                else if (item.FieldType == typeof(string))
                {
                    xe.SetAttributeValue("Type", item.FieldType.Name + "_" + STRING);
                }
                else
                {
                    xe.SetAttributeValue("Type", item.FieldType.Name + "_" + EXTENTION);
                }

                xml.Add(xe);

            }

            xml.Save("D:\\" + GetType().Name + ".xml");

        }

    }

    public class MySetting : XMLBase
    {
        //基元类型
        public bool _flag = true;

        public byte _age = 0;

        public int _id = 0;

        public char _character = '0';

        public long _size = 0L;

        public float _height = 0f;

        public double _width = 0d;


        //结构和类
        public decimal _depth = 0m;

        public string _name = string.Empty;

        public Rectangle _rect = new Rectangle();

        public Point _pos = new Point();

        public DateTime _date = DateTime.Now;

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
