using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;

namespace PropertyPopup
{
    public class PropertyHandler                                                               
    {
        public PropertyHandler(object obj, ToolStripItemCollection items, int depth)
        {
            foreach (PropertyInfo propInfo in obj.GetType().GetProperties())
            {
                string descString = GetDescription(obj, propInfo.Name);
                if (descString.Length == 0)
                    continue;                               //skip property because it has not description
                if(!IsBrowsable(obj, propInfo.Name))
                    continue;                               //skip property because it is not browsable

                if (propInfo.PropertyType.Name == "Point2D")
                {
                    new PropPoint2D(obj, propInfo, descString, items);
                    continue;
                }
                if (propInfo.PropertyType.Name == "Point3D")
                {
                    new PropPoint3D(obj, propInfo, descString, items);
                    continue;
                }
                switch (Type.GetTypeCode(propInfo.PropertyType))
                {
                    case TypeCode.Boolean:
                        new PropBoolean(obj, propInfo, descString, items);
                        break;
                    case TypeCode.Double:
                        new PropDouble(obj, propInfo, descString, items);
                        break;
                    case TypeCode.Int32:
                        if (propInfo.PropertyType.IsEnum)
                            new PropEnum(obj, propInfo, descString, items);
                        else
                            new PropString(obj, propInfo, descString, items);
                        break;
                    /*case TypeCode.String:
                        if (propInfo.PropertyType.IsClass)
                            new PropStringArray(obj, propInfo, descString, items);
                        else
                            new PropString(obj, propInfo, descString, items);
                        break;*/
                    case TypeCode.Object:
                        if (propInfo.PropertyType.Name == "Color")
                            new PropColor(obj, propInfo, descString, items);
                        else
                        {
                            Type tColl = typeof(ICollection<>);
                            Type t = propInfo.PropertyType;
                            if (t.IsGenericType && tColl.IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                                t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == tColl))
                            {
                                new PropCollection(obj, propInfo, descString, items);
                            }
                            else
                            {
                                new PropObject(obj, propInfo, descString, items, depth);
                            }
                        }
                        break;
                    default:
                        new PropString(obj, propInfo, descString, items);
                        break;
                }
            }
        }
        bool IsBrowsable(object obj, string name)
        { 
            AttributeCollection attributes = TypeDescriptor.GetProperties(obj)[name].Attributes;
            return attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.Yes);
        }
        string GetDescription(object obj, string name)
        {
            try 
            { 
                AttributeCollection attributes = TypeDescriptor.GetProperties(obj)[name].Attributes;
                DescriptionAttribute myAttribute = (DescriptionAttribute)attributes[typeof(DescriptionAttribute)];
                return myAttribute.Description;
            }
            catch
            {
                return "";
            }
        }
    }
    class PropBoolean                                                                           //PropBoolean
    {
        object propObject;
        PropertyInfo propInfo;

        public PropBoolean(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            string propValue;
            if ((Boolean)prop.GetValue(obj, null))
                propValue = "True";
            else
                propValue = "False";
            string[] values = { "False", "True" };
            foreach (string str in values)
            {
                ToolStripItem sub = new ToolStripMenuItem();
                sub.Text = str;
                sub.ToolTipText = toolTip;
                if (str == propValue)
                    (sub as ToolStripMenuItem).Checked = true;
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    sub.Click += new EventHandler(Boolean_Item_Click);
                else
                    (sub as ToolStripMenuItem).Enabled = false;
            }
        }
        void Boolean_Item_Click(object sender, EventArgs e)
        {
            if (propInfo.SetMethod != null)
                propInfo.SetValue(propObject, ((sender as ToolStripItem).Text == "True"), null);
        }
    }
    class PropDouble : IDisposable                                                              //PropDouble
    {
        object propObject;
        PropertyInfo propInfo;
        ToolStripTextBox textBox = new ToolStripTextBox();

        public PropDouble(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text =prop.Name;
            items.Add(item);
            if (prop.GetValue(obj, null) != null)
            {
                ToolStripItem sub = textBox;
                textBox.Text = prop.GetValue(obj, null).ToString();
                sub.ToolTipText = toolTip;
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    textBox.TextChanged += new EventHandler(Value_Changed);
                else
                    textBox.Enabled = false;
            }
        }
        void Value_Changed(object sender, EventArgs e)
        {
            double res = GetDouble(textBox.Text, -123456789.987654321);
            if (res != -123456789.987654321)
                propInfo.SetValue(propObject, res);
        }
        public static double GetDouble(string value, double defaultValue)
        {
            double result;
            //Try parsing in the current culture
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
            }
            return result;
        }
        public void Dispose()
        {
            if (textBox != null)
                textBox.Dispose();
        }
    }
    class PropCollection                                                                        //PropCollection
    {
        object propObject;
        PropertyInfo propInfo;

        public PropCollection(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            /*List<Room> list = prop.GetValue(obj, null) as List<Room>;
            if (list != null)
            {
                foreach (object obj in list)
                {
                    Console.WriteLine(obj);
                }
            }*/
        }
    }
    class PropColor                                                                             //PropColor
    {
        object propObject;
        PropertyInfo propInfo;

        public PropColor(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            Color actual = (Color)prop.GetValue(obj, null);
            string actStr = actual.Name;
            foreach (KnownColor clr in colorsArray)
            {
                ToolStripItem sub = new ToolStripMenuItem();
                sub.Text = clr.ToString();
                sub.ToolTipText = toolTip;
                if (sub.Text == actStr)
                    (sub as ToolStripMenuItem).Checked = true;
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    sub.Click += new EventHandler(Color_Item_Click);
                else
                    (sub as ToolStripMenuItem).Enabled = false;
            }
        }
        void Color_Item_Click(object sender, EventArgs e)
        {
            if (propInfo.SetMethod != null)
                propInfo.SetValue(propObject, Color.FromName((sender as ToolStripItem).Text), null);
        }
    }
    class PropEnum                                                                              //PropEnum
    {
        object propObject;
        PropertyInfo propInfo;

        public PropEnum(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            int i = 0;
            foreach (string en in Enum.GetNames(propInfo.PropertyType))
            {
                ToolStripItem sub = new ToolStripMenuItem();
                sub.Text = en;
                sub.ToolTipText = toolTip;
                if ((int)prop.GetValue(obj, null) == i)
                    (sub as ToolStripMenuItem).Checked = true;
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    sub.Click += new EventHandler(Enum_Item_Click);
                else
                    (sub as ToolStripMenuItem).Enabled = false;
                i++;
            }
        }
        void Enum_Item_Click(object sender, EventArgs e)
        {
            string title = (sender as ToolStripItem).Text;
            propInfo.SetValue(propObject, Enum.Parse(propInfo.PropertyType, title));
        }
    }
    class PropObject                                                                            //PropObject
    {
        public PropObject(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items, int depth)
        {
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            object subObj = prop.GetValue(obj, null);
            if ((subObj != null) && (depth < 1))
            //if (subObj != null)
                new PropertyHandler(subObj, (item as ToolStripMenuItem).DropDownItems, ++depth);
        }
    }
    class PropPoint2D : IDisposable                                                             //PropPoint2D
    {
        object propObject;
        PropertyInfo propInfo;
        ToolStripTextBox textBox = new ToolStripTextBox();

        public PropPoint2D(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            if (prop.GetValue(obj, null) != null)
            {
                ToolStripItem sub = textBox;
                sub.ToolTipText = toolTip;
                textBox.Text = prop.GetValue(obj, null).ToString();
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    textBox.TextChanged += new EventHandler(Value_Changed);
                else
                    textBox.Enabled = false;
            }
        }
        void Value_Changed(object sender, EventArgs e)
        {
        }
        public void Dispose()
        {
            if (textBox != null)
                textBox.Dispose();
        }
    }
    class PropPoint3D : IDisposable                                                             //PropPoint3D
    {
        object propObject;
        PropertyInfo propInfo;
        ToolStripTextBox textBox = new ToolStripTextBox();

        public PropPoint3D(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            if (prop.GetValue(obj, null) != null)
            {
                ToolStripItem sub = textBox;
                sub.ToolTipText = toolTip;
                textBox.Text = prop.GetValue(obj, null).ToString();
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    textBox.TextChanged += new EventHandler(Value_Changed);
                else
                    textBox.Enabled = false;
            }
        }
        void Value_Changed(object sender, EventArgs e)
        {
        }
        public void Dispose()
        {
            if (textBox != null)
                textBox.Dispose();
        }
    }
    class PropString : IDisposable                                                              //PropString
    {
        object propObject;
        PropertyInfo propInfo;
        ToolStripTextBox textBox = new ToolStripTextBox();

        public PropString(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text =prop.Name;
            items.Add(item);
            if (prop.GetValue(obj, null) != null)
            {
                ToolStripItem sub = textBox;
                sub.ToolTipText = toolTip;
                textBox.Text = prop.GetValue(obj, null).ToString();
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    textBox.TextChanged += new EventHandler(Value_Changed);
                else
                    textBox.Enabled = false;
            }
        }
        void Value_Changed(object sender, EventArgs e)
        {
            if (propInfo.PropertyType == typeof(System.Int32))
            {
                try { propInfo.SetValue(propObject, Convert.ToInt32(textBox.Text)); }
                catch { }
            }
            else
                propInfo.SetValue(propObject, textBox.Text);
        }
        public void Dispose()
        {
            if (textBox != null)
                textBox.Dispose();
        }
    }
    class PropStringArray : IDisposable                                                         //PropStringArray
    {
        object propObject;
        PropertyInfo propInfo;
        ToolStripTextBox textBox = new ToolStripTextBox();

        public PropStringArray(object obj, PropertyInfo prop, string toolTip, ToolStripItemCollection items)
        {
            propObject = obj;
            propInfo = prop;
            ToolStripItem item = new ToolStripMenuItem();
            item.Text = prop.Name;
            items.Add(item);
            string[] subs = null;
            var attributes = prop.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if (attribute.GetType() == typeof(System.ComponentModel.TypeConverterAttribute))
                {
                    string msg = (attribute as System.ComponentModel.TypeConverterAttribute).ConverterTypeName;
                    string[] sA = msg.Split(',');

                    Type calledType = Type.GetType(sA[0]);
                    MethodInfo mi = calledType.GetMethod("GetNames");
                    subs = (string[]) mi.Invoke(Activator.CreateInstance(calledType), null);

                    break;
                }
            }
            if(subs!=null)
            {
                foreach (string en in subs)
                {
                    ToolStripItem sub = new ToolStripMenuItem();
                    sub.Text = en;
                    sub.ToolTipText = toolTip;
                    if ((string)prop.GetValue(obj, null) == en)
                        (sub as ToolStripMenuItem).Checked = true;
                    (item as ToolStripMenuItem).DropDownItems.Add(sub);
                    if (prop.CanWrite)
                        sub.Click += new EventHandler(List_Item_Click);
                    else
                        sub.Enabled = false;
                }
            }
            else
            {
                ToolStripItem sub = textBox;
                sub.ToolTipText = toolTip;
                textBox.Text = prop.GetValue(obj, null).ToString();
                (item as ToolStripMenuItem).DropDownItems.Add(sub);
                if (prop.CanWrite)
                    textBox.TextChanged += new EventHandler(Value_Changed);
                else
                    textBox.Enabled = false;
            }
        }
        void List_Item_Click(object sender, EventArgs e)
        { 
            string title = (sender as ToolStripItem).Text;
            propInfo.SetValue(propObject, title);
        }
        void Value_Changed(object sender, EventArgs e)
        {
        }
        public void Dispose()
        {
            if (textBox != null)
                textBox.Dispose();
        }
    }
}
