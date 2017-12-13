using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlReflection;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace XMLReflection_UnitTest
{

    [TestClass()]
    public class UnitTest
    {


        [TestMethod()]
        public void Test_BaseType()
        {
            XML_BaseType mXML_Base = new XML_BaseType();

            mXML_Base.Age = 101;
            mXML_Base.character = 'g';
            mXML_Base.id = 102;
            mXML_Base.height = 103;
            mXML_Base.name = "mytest";
            mXML_Base.size = 104L;
            mXML_Base.width = 105.1d;
            mXML_Base.depth = 106.2M;
            mXML_Base.flag = false;
            mXML_Base.date = DateTime.Now;

            var map1 = new Dictionary<decimal, string>();
            var map2 = new Dictionary<decimal, string>();

            map1.Add(3.2M, "3.2M");
            map1.Add(4.99992M, "4.99992M");
            map2.Add(100.01M, "100.01M");

            mXML_Base.dictionary.Add(1, map1);
            mXML_Base.dictionary.Add(100, map2);


            var filename = "xml_basetype.xml";
            mXML_Base.SaveXML(filename);

            XML_BaseType mXML_Base_copy = new XML_BaseType();
            mXML_Base_copy.LoadXML(filename);


            Assert.AreEqual(mXML_Base.Age, mXML_Base_copy.Age);
            Assert.AreEqual(mXML_Base.character, mXML_Base_copy.character);
            Assert.AreEqual(mXML_Base.id, mXML_Base_copy.id);
            Assert.AreEqual(mXML_Base.height, mXML_Base_copy.height);
            Assert.AreEqual(mXML_Base.name, mXML_Base_copy.name);
            Assert.AreEqual(mXML_Base.size, mXML_Base_copy.size);
            Assert.AreEqual(mXML_Base.width, mXML_Base_copy.width);
            Assert.AreEqual(mXML_Base.depth, mXML_Base_copy.depth);
            Assert.AreEqual(mXML_Base.flag, mXML_Base_copy.flag);
            Assert.AreEqual(mXML_Base.date, mXML_Base_copy.date);

            foreach (var item in mXML_Base.dictionary.Keys)
            {
                var value1 = mXML_Base.dictionary[item];
                var value2 = mXML_Base_copy.dictionary[item];

                foreach (var item1 in value1.Keys)
                {
                    var value3 = value1[item1];
                    var value4 = value1[item1];
                    Assert.AreEqual(value3, value4);
                }
            }


        }


        [TestMethod()]
        public void Test_Array()
        {
            XML_Array mXML_Array = new XML_Array();

            mXML_Array.a_bool = new[] { true, false, true };
            mXML_Array.a_datatime = new[] { DateTime.MinValue, DateTime.Now, DateTime.MaxValue };
            mXML_Array.a_int = new[] { 0, int.MaxValue, int.MinValue };
            mXML_Array.a_str = new[] { "", "12132435434242354546", "@##$%^^&**(((/*-+-*+_" };
            mXML_Array.list = new List<int>() { 10, 20, 30, 40 };
            mXML_Array.a_lisyint[0] = new List<int>() { 1, 3, 4 };
            mXML_Array.a_lisyint[1] = new List<int>() { 44, 55, 66 };
            mXML_Array.list_array.Add(new[] { 99, 88, 77 });
            mXML_Array.list_array.Add(new[] { 199, 188, 177 });
            mXML_Array.a_a_string[0] = new string[] { "a", "1" };
            mXML_Array.a_a_string[1] = new string[] { "2", "b" };
            mXML_Array.a_a_string[2] = new string[] { "3", "c" };


            var filename = "xml_array.xml";
            mXML_Array.SaveXML(filename);


            XML_Array mXML_Array_copy = new XML_Array();
            mXML_Array_copy.LoadXML(filename);


            for (int i = 0; i < mXML_Array.a_bool.Length; i++)
                Assert.AreEqual(mXML_Array.a_bool[i], mXML_Array_copy.a_bool[i]);

            for (int i = 0; i < mXML_Array.a_datatime.Length; i++)
                Assert.AreEqual(mXML_Array.a_datatime[i], mXML_Array_copy.a_datatime[i]);

            for (int i = 0; i < mXML_Array.a_int.Length; i++)
                Assert.AreEqual(mXML_Array.a_int[i], mXML_Array_copy.a_int[i]);

            for (int i = 0; i < mXML_Array.a_str.Length; i++)
                Assert.AreEqual(mXML_Array.a_str[i], mXML_Array_copy.a_str[i]);

            Assert.AreEqual(mXML_Array.a_lisyint.Length, mXML_Array_copy.a_lisyint.Length);

            Assert.AreEqual(mXML_Array.a_lisyint[1][2], mXML_Array_copy.a_lisyint[1][2]);

            for (int i = 0; i < mXML_Array.list_array.Count; i++)
                for (int j = 0; j < mXML_Array.list_array[i].Length; j++)
                    Assert.AreEqual(mXML_Array.list_array[i][j], mXML_Array_copy.list_array[i][j]);

            for (int i = 0; i < mXML_Array.a_a_string.Length; i++)
                for (int j = 0; j < mXML_Array.a_a_string[i].Length; j++)
                    Assert.AreEqual(mXML_Array.a_a_string[i][j], mXML_Array_copy.a_a_string[i][j]);



        }


        [TestMethod()]
        public void Test_Bodied()
        {
            XML_Bodied mXML_Bodied = new XML_Bodied();
            mXML_Bodied.a_int = new int[] { int.MinValue, 0, int.MaxValue };
            mXML_Bodied.Flag = false;
            mXML_Bodied.list = new List<string>() { "", "1213", "abc", "测试" };
            mXML_Bodied.Name = "Test_Bodied";
            mXML_Bodied.Num = 110;


            var filename = "xml_Bodied.xml";
            mXML_Bodied.SaveXML(filename);

            XML_Bodied mXML_Bodied_copy = new XML_Bodied();
            mXML_Bodied_copy.LoadXML(filename);

            for (int i = 0; i < mXML_Bodied.a_int.Length; i++)
                Assert.AreEqual(mXML_Bodied.a_int[i], mXML_Bodied_copy.a_int[i]);
            for (int i = 0; i < mXML_Bodied.list.Count; i++)
                Assert.AreEqual(mXML_Bodied.list[i], mXML_Bodied_copy.list[i]);

            Assert.AreEqual(mXML_Bodied.Name, mXML_Bodied_copy.Name);
            Assert.AreEqual(mXML_Bodied.Num, mXML_Bodied_copy.Num);
            Assert.AreEqual(mXML_Bodied.Flag, mXML_Bodied_copy.Flag);

        }


    }

    public class XML_BaseType : XMLBase
    {
        //string

        public string name = string.Empty;

        //基元类型

        public bool flag = true;

        private byte age = 0;

        public int id = 0;

        public char character = '0';

        public long size = 0L;

        public float height = 0f;

        public double width = 0d;

        public decimal depth = 0m;

        public Dictionary<int, Dictionary<decimal, string>> dictionary = new Dictionary<int, Dictionary<decimal, string>>();

        //结构和类

        public DateTime date = DateTime.Now;

        public byte Age { get => age; set => age = value; }
    }

    public class XML_Array : XMLBase
    {
        public string[][] a_a_string = new string[3][];

        public List<int[]> list_array = new List<int[]>();

        public List<int> list = new List<int>();

        public List<int>[] a_lisyint = new List<int>[2];

        public int[] a_int = new int[3];

        public string[] a_str = new string[3];

        public bool[] a_bool = new bool[3];

        public DateTime[] a_datatime = new DateTime[3];
    }

    public class XML_Bodied : XMLBase
    {
        public bool Flag { set; get; } = true;
        public int Num { set; get; } = 100;
        public string Name { set; get; } = "lucy";
        public int[] a_int { set; get; } = { 1, 3, 4 };
        public List<string> list { set; get; } = new List<string>() { "xd", "ff" };

    }



}
