using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Ascendon.ContentService.Contract;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;

namespace TestObjectGenerator
{
    public partial class TestObjectGenerator : Form
    {
        public TestObjectGenerator()
        {
            InitializeComponent();
            comboBox1.DataSource = Enum.GetNames(typeof(SerializationTypesEnum));
            var numberOfItems = Enumerable.Range(1, 3);
            comboBox2.DataSource = numberOfItems.ToList();
            comboBox3.DataSource = new List<string> { "False", "True"};
            comboBox4.DataSource = Enum.GetNames(typeof(ModelEnum));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var c = new Convert();
            string serializationType = comboBox1.SelectedValue.ToString();
            bool isList = bool.Parse(comboBox3.SelectedValue.ToString());
            int count = Int32.Parse(comboBox2.SelectedValue.ToString());
            string model = comboBox4.SelectedValue.ToString();
            var obj = GetObject(model, isList);
            var data = c.GetValue(obj,count);
            if (serializationType.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                DataOutput.Text = JsonConvert.SerializeObject(data);
            }
            else if (serializationType.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                DataOutput.Text = FormatXml(obj);
            }
        }

        public object GetObject(string val, bool isList )
        {
            switch (val)
            {
                case "TestObject":
                    if (!isList)
                    {
                        return new TestObject();
                    }
                    else
                    {
                        return new List<TestObject>();
                    }
                case "Product":
                    if (!isList)
                    {
                        return new Ascendon.ContentService.Contract.Product();
                    }
                    else
                    {
                        return new List<Ascendon.ContentService.Contract.Product>();
                    }
                case "Entitlement":
                    if (!isList)
                    {
                        return new Entitlement();
                    }
                    else
                    {
                        return  new List<Entitlement>();
                    }
                
            }
            return  new object();
        }

        private string FormatXml(object obj)
        {
            StringBuilder bob = new StringBuilder();
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            using (StringWriter stringWriter = new StringWriter(bob))
            {
                // We will use the Formatting of our xmlTextWriter to provide our indentation.
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlSerializer.Serialize(xmlTextWriter, obj);
                }
            }
            return bob.ToString();
        }
    }
}
