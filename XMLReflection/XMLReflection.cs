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

        const string IENUMERABLE = "IList";
        const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public enum SupportEnum
        {
            STRING, PRIMARY, CLASS, STRUCT, ENUMERABLE, OTHER
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private PropertyInfo[] mProperties = null;

        private Dictionary<int, Action> mDictionary = new Dictionary<int, Action>();

        public XMLBase()
        {
            mProperties = GetType().GetProperties(PublicInstance);

            if (mProperties.Length > 0) _NotifyDataSetChanged(this);
        }

        private static SupportEnum GetTypeFlag(Type t)
        {
            if (t == typeof(string))
            {
                return SupportEnum.STRING;
            }
            else if (t.IsPrimitive || t == typeof(decimal))
            {
                return SupportEnum.PRIMARY;
            }
            else if (t.IsSubclassOf(typeof(XMLBase)))
            {
                return SupportEnum.CLASS;
            }
            else if (t.IsSubclassOf(typeof(ValueType)))
            {
                return SupportEnum.STRUCT;
            }
            else if (t.GetInterface(IENUMERABLE) != null)
            {
                return SupportEnum.ENUMERABLE;
            }
            else
            {
                return SupportEnum.OTHER;
            }
        }


        string HandleName(object _object)
        {
            var type = _object.GetType();

            var orgin_name = _object.GetType().Name;

            var result_name = orgin_name;

            if (_object is System.Collections.IList)
            {
                var count = (_object as System.Collections.IList).Count;

                if (orgin_name.Contains("`"))
                {
                    result_name = orgin_name.Split('`')[0] + "_" + count;
                }
                else if (orgin_name.Contains("["))
                {
                    result_name = orgin_name.Split('[')[0] + "_" + count;
                }
            }

            return result_name;
        }

        protected XElement GetXMLIEnumerable(object _object)
        {

            XElement xml = new XElement(HandleName(_object));

            var type = GetTypeFlag(_object.GetType());

            if (type == SupportEnum.ENUMERABLE)
            {
                var list = _object as System.Collections.IEnumerable;

                foreach (var temp in list)
                {
                    xml.Add(GetXMLIEnumerable(temp));
                }
            }
            else
            {
                xml = GetXML(_object);
                xml.Name = _object.GetType().Name;
            }

            return xml;
        }

        protected object SetXML(object _object, XElement _elem)
        {
            if (_object == null) return null;

            if (_elem == null) return _object;

            var type = GetTypeFlag((_object as Type) ?? _object.GetType());

            if (type == SupportEnum.STRING)
            {
                _object = _elem.Value;
            }
            else if (type == SupportEnum.PRIMARY)
            {
                MethodInfo parse;

                if (_object is Type)
                {
                    parse = mCoreLib.CreateInstance((_object as Type).ToString()).GetType().GetMethod("Parse", new[] { typeof(string) });
                }
                else
                {
                    parse = mCoreLib.CreateInstance(_object.GetType().ToString()).GetType().GetMethod("Parse", new[] { typeof(string) });
                }

                _object = parse.Invoke(null, new object[] { _elem.Value });
            }
            else if (type == SupportEnum.CLASS || type == SupportEnum.STRUCT || type == SupportEnum.OTHER)
            {
                var mem = _object.GetType().GetFields(NonPublicInstance | PublicInstance);

                foreach (var item in mem)
                {
                    var item_value = item.GetValue(_object);

                    item.SetValue(_object, SetXML(item_value, _elem.Element(item.Name)));
                }
            }
            else if (type == SupportEnum.ENUMERABLE)
            {
                var list = _object as System.Collections.IList;

                Type list_type = _object.GetType().GetElementType();

                var ele_type = GetTypeFlag(list_type);

                list.Clear();

                int index = 0;

                foreach (var ele in _elem.Elements())
                {
                    object obj = null;

                    if (ele_type == SupportEnum.ENUMERABLE)
                    {
                        obj = Activator.CreateInstance(list_type);
                    }
                    else if (ele_type == SupportEnum.STRING)
                    {
                        obj = string.Empty;
                    }
                    else if (list is Array)
                    {
                        var rank = (list as Array).Rank;

                        if (rank == 1)
                        {
                            obj = Activator.CreateInstance(list_type);

                            list[index++] = SetXML(obj, ele);
                        }
                        else
                        {
                            throw new NotImplementedException("Did not implement multi-dimensional array");
                        }
                    }
                    else
                    {
                        obj = Activator.CreateInstance(list_type);

                        list.Add(SetXML(obj, ele));
                    }
                }

                _object = list;
            }

            return _object;
        }

        protected XElement GetXML(object _object)
        {
            XElement xml = new XElement(GetType().Name);

            var type = GetTypeFlag(_object.GetType());

            if (type == SupportEnum.STRING || type == SupportEnum.PRIMARY)
            {
                xml.SetValue(_object.ToString());
            }
            else
            {
                var mem = _object.GetType().GetFields(NonPublicInstance | PublicInstance);

                foreach (var item in mem)
                {
                    XElement xe = new XElement(item.Name);

                    var itemValue = item.GetValue(_object);

                    if (itemValue == null) continue;

                    var item_type = GetTypeFlag(itemValue.GetType());

                    if (item_type == SupportEnum.CLASS)
                    {
                        xe = (itemValue as XMLBase).GetXML(itemValue);
                    }
                    else if (item_type == SupportEnum.STRUCT)
                    {
                        xe = GetXML(itemValue);
                    }
                    else if (item_type == SupportEnum.ENUMERABLE)
                    {
                        xe = GetXMLIEnumerable(itemValue);
                    }
                    else
                    {
                        xe = GetXML(itemValue);
                    }

                    xe.Name = item.Name;

                    xml.Add(xe);
                }
            }

            return xml;
        }

        public void LoadXML(string filename = "")
        {
            XElement xml = XElement.Load((filename == "" ? GetType().Name : filename) + ".xml");

            SetXML(this, xml);

            NotifyDataSetChanged();
        }

        public void SaveXML(string filename = "")
        {
            var temp = GetXML(this);

            temp.Save((filename == "" ? GetType().Name : filename) + ".xml");
        }

        protected virtual void OnPropertyChanged(object _object, string name)
        {
            PropertyChanged?.Invoke(_object, new PropertyChangedEventArgs(name));
        }

        protected virtual void _NotifyDataSetChanged(object _object)
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
                            var temp = (item.GetValue(_object, null) as XMLBase);

                            temp?._NotifyDataSetChanged(temp);
                        });
                    }
                    else
                    {
                        mDictionary.Add(item.MetadataToken, (Action)(() =>
                        {
                            OnPropertyChanged(_object, item.Name);
                        }));
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
