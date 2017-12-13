using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace XmlReflection
{
    public class XMLBase : INotifyPropertyChanged
    {

        public static readonly Assembly mCoreLib = Assembly.Load("mscorlib");
        public static readonly Assembly mCurrLib = Assembly.GetExecutingAssembly();

        const string IENUMERABLE = "ICollection";
        const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        private PropertyInfo[] mProperties = null;

        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<int, Action> mDictionary = new Dictionary<int, Action>();

        public XMLBase()
        {
            mProperties = GetType().GetProperties(PublicInstance);

            if (mProperties.Length > 0) _NotifyDataSetChanged(this);
        }

        public enum SupportEnum
        {
            STRING, PRIMARY, CLASS, STRUCT, ENUMERABLE, ARRAY, OTHER
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
            else if (t.IsArray)
            {
                return SupportEnum.ARRAY;
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

        private string HandleName(object _object)
        {
            if (_object is FieldInfo)
            {
                var field = _object as FieldInfo;

                var name = field.Name;

                if (name.Contains("__BackingField"))
                {
                    return name.Split('<')[1].Split('>')[0];
                }
                else if (name.Contains("<") || name.Contains(">"))
                {
                    return name.Replace("<", "_").Replace(">", "_");
                }
                else
                {
                    return name;
                }
            }

            var type = _object.GetType();

            var orgin_name = _object.GetType().Name;

            if (orgin_name == "KeyValuePair`2") return "keyValue";

            var result_name = orgin_name;

            if (_object is System.Collections.ICollection)
            {
                var count = (_object as System.Collections.ICollection).Count;

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

            if (type == SupportEnum.ENUMERABLE || type == SupportEnum.ARRAY)
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

                xml.Name = HandleName(_object);
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
                if (_object is Type) _object = Activator.CreateInstance(_object as Type);

                var mem = _object.GetType().GetFields(NonPublicInstance | PublicInstance);

                foreach (var item in mem)
                {
                    var item_value = item.GetValue(_object);

                    if (item_value == null) throw new NotSupportedException("Field must be initialized");

                    item.SetValue(_object, SetXML(item_value, _elem.Element(HandleName(item))));
                }
            }
            else if (type == SupportEnum.ARRAY)
            {
                var thisType = _object as Type ?? _object.GetType();

                Type ele_type = thisType.GetElementType() ?? thisType.GetGenericArguments()[0];

                var obj = Activator.CreateInstance(thisType, new object[] { _elem.Elements().Count() }) as System.Collections.IList;

                int index = 0;

                foreach (var ele in _elem.Elements())
                {
                    obj[index] = SetXML(ele_type, ele);

                    index++;
                }

                _object = obj;
            }
            else if (type == SupportEnum.ENUMERABLE)
            {
                var thisType = _object as Type ?? _object.GetType();

                Type ele_type = thisType.GetElementType() ?? thisType.GetGenericArguments()[0];

                var obj = Activator.CreateInstance(thisType);

                if (obj is System.Collections.IList)
                {
                    var ilist = obj as System.Collections.IList;

                    foreach (var ele in _elem.Elements())
                    {
                        ilist.Add(SetXML(ele_type, ele));
                    }
                }
                else if (obj is System.Collections.IDictionary)
                {
                    var typeKEY = thisType.GetGenericArguments()[0];

                    var typeVALUE = thisType.GetGenericArguments()[1];

                    var ilist = obj as System.Collections.IDictionary;

                    foreach (var ele in _elem.Elements())
                    {
                        ilist.Add(SetXML(typeKEY, ele.Element("key")), SetXML(typeVALUE, ele.Element("value")));
                    }
                }

                _object = obj;

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
                    XElement xe = new XElement(HandleName(item));

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
                    else if (item_type == SupportEnum.ENUMERABLE || item_type == SupportEnum.ARRAY)
                    {
                        xe = GetXMLIEnumerable(itemValue);
                    }
                    else
                    {
                        xe = GetXML(itemValue);
                    }

                    xe.Name = HandleName(item);

                    xml.Add(xe);
                }
            }

            return xml;
        }

        public void LoadXML(string filename = "")
        {
            XElement xml = XElement.Load((filename == "" ? (GetType().Name + ".xml") : filename));

            SetXML(this, xml);

            NotifyDataSetChanged();
        }

        public void SaveXML(string filename = "")
        {
            var temp = GetXML(this);

            temp.Save((filename == "" ? (GetType().Name + ".xml") : filename));
        }

        protected virtual void OnPropertyChanged(object _object, string name)
        {
            PropertyChanged?.Invoke(_object, new PropertyChangedEventArgs(name));
        }

        protected virtual void _NotifyDataSetChanged(object _object)
        {
            if (mProperties == null) return;

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
                        mDictionary.Add(item.MetadataToken, (() =>
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

}
