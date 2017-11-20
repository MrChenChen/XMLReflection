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

    public class XMLBase : INotifyPropertyChanged
    {

        public static readonly Assembly mCoreLib = Assembly.Load("mscorlib");
        public static readonly Assembly mCurrLib = Assembly.GetExecutingAssembly();

        const int STRING = 0;
        const int PRIMARY = 1;
        const int CLASS = 2;
        const int EXTENTION = 3;

        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyInfo[] mProperties = null;

        private Dictionary<int, Action> mDictionary = new Dictionary<int, Action>();

        public XMLBase()
        {
            Init_NotifyChanged();
        }

        private void Init_NotifyChanged()
        {
            mProperties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            _NotifyDataSetChanged(this);
        }

        public static int GetTypeFlag(Type t)
        {
            if (t == typeof(string))
            {
                return STRING;
            }
            else if (t.IsPrimitive || t == typeof(decimal))
            {
                return PRIMARY;
            }
            else if (t.IsSubclassOf(typeof(XMLBase)))
            {
                return CLASS;
            }
            else
            {
                return EXTENTION;
            }
        }

        public void SetXML(object _object, XElement _elem)
        {
            var member = _object.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var item in member)
            {
                var elem = _elem.Element(item.Name);

                MethodInfo parse = null;

                switch (GetTypeFlag(item.FieldType))
                {
                    case STRING:
                        {
                            item.SetValue(_object, elem.Value);
                            break;
                        }
                    case PRIMARY:
                        {
                            parse = mCoreLib.CreateInstance("System." + item.FieldType.Name)
                                .GetType().GetMethod("Parse", new[] { typeof(string) });

                            item.SetValue(_object, parse.Invoke(null, new object[] { elem.Value }));

                            break;
                        }
                    case CLASS:
                        {
                            var currField = item.GetValue(_object);

                            SetXML(currField, elem);

                            break;
                        }
                    case EXTENTION:
                        {
                            parse = mCurrLib.GetTypes()
                                .Where(x => x.Name.Contains(item.FieldType.Name + "_Extention"))
                                .First().GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);

                            if (parse == null) return;

                            item.SetValue(_object, parse.Invoke(null, new object[] { null, elem.Value }));

                            break;
                        }
                    default:
                        break;
                }
            }
        }

        public XElement GetXML()
        {
            XElement xml = new XElement(GetType().Name);

            var mem = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var item in mem)
            {
                XElement xe = new XElement(item.Name);

                if (item.IsStatic) continue;

                var itemValue = item.GetValue(this);

                if (itemValue is XMLBase)
                {
                    xe = (itemValue as XMLBase).GetXML();

                    xe.Name = item.Name;
                }
                else
                {
                    xe.SetValue(itemValue.ToString());
                }

                xml.Add(xe);
            }

            return xml;
        }

        public void LoadXML()
        {
            XElement xml = XElement.Load(GetType().Name + ".xml");

            SetXML(this, xml);

            NotifyDataSetChanged();
        }

        public void SaveXML()
        {
            var temp = GetXML();

            temp.Save(GetType().Name + ".xml");
        }

        private void _NotifyDataSetChanged(object _this)
        {
            foreach (var item in mProperties)
            {
                if (mDictionary.ContainsKey(item.MetadataToken))
                {
                    mDictionary[item.MetadataToken].Invoke();
                }
                else
                {
                    if (item.PropertyType.IsSubclassOf(typeof(XMLBase)))
                    {
                        mDictionary.Add(item.MetadataToken, () =>
                        {
                            var temp = (item.GetValue(_this, null) as XMLBase);

                            temp?._NotifyDataSetChanged(temp);
                        });
                    }
                    else
                    {
                        mDictionary.Add(item.MetadataToken, () =>
                        {
                            PropertyChanged?.Invoke(_this, new PropertyChangedEventArgs(item.Name));
                        });
                    }
                }
            }
        }

        public void NotifyDataSetChanged()
        {
            _NotifyDataSetChanged(this);
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
