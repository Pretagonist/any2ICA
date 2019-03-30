using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace anyICA
{
    public static class Extensions
    {
        public static string ToXml(this object obj)
        {
            XmlSerializer s = new XmlSerializer(obj.GetType());
            using (StringWriter writer = new StringWriter())
            {
                s.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T FromXml<T>(this string data)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                object obj = s.Deserialize(reader);
                return (T)obj;
            }
        }
    }

    public static class Tools
    {
        public static string RemoveSpecialCharacters(string str)
        {
            //return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
            return System.Net.WebUtility.HtmlDecode(str.Replace("\r", "").Replace("  ", ""));
        }

    }

}
