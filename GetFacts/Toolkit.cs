using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GetFacts
{
    public static class Toolkit
    {
        /*public static FlowDocument JSonToFlowDocument(string path)
        {
            using (FileStream fileStream = new FileStream(path,FileMode.Open,FileAccess.Read))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    using (JsonReader jsonReader = new JsonTextReader(streamReader))
                    {
                        return JSonToFlowDocument(jsonReader);
                    }
                }
            }
        }

        public static FlowDocument JSonToFlowDocument(JsonReader reader)
        {
            FlowDocument output = new FlowDocument();
            Paragraph p = new Paragraph();
            Span content = new Span();
            p.Inlines.Add(content);
            output.Blocks.Add(p);           

            while (reader.Read())
            {
                switch(reader.TokenType)
                {
                    case JsonToken.StartObject:
                        content.Inlines.Add(new Run("{"));
                        content.Inlines.Add(new LineBreak());
                        break;

                    case JsonToken.Boolean: break;
                    case JsonToken.Bytes:break;
                    case JsonToken.Comment:break;
                    case JsonToken.Date:break;
                    case JsonToken.EndArray:break;
                    case JsonToken.EndConstructor:break;
                    case JsonToken.EndObject:break;
                    case JsonToken.Float:break;
                    case JsonToken.Integer:break;
                    case JsonToken.None:break;
                    case JsonToken.Null:break;
                    case JsonToken.PropertyName:break;
                    case JsonToken.Raw:break;
                    case JsonToken.StartArray: break;
                    case JsonToken.StartConstructor: break;
                    case JsonToken.String:break;
                    case JsonToken.Undefined:break;
                }
            }

            return output;
        }*/

        public static string GetRelativePath(string absolutePath, string root)
        {
            char[] separators = new char[] 
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar,
                Path.PathSeparator,
                Path.VolumeSeparatorChar
            };

            string[] absolutePathElements = absolutePath.Split(separators);
            string[] rootPathElements = root.Split(separators);

            int min = Math.Min(absolutePathElements.Length, rootPathElements.Length);
            int max = Math.Max(absolutePathElements.Length, rootPathElements.Length);
            for (int i=0;i<min;i++)
            {
                if (string.Compare(absolutePathElements[i], rootPathElements[i]) != 0)
                    throw new ArgumentException();
            }

            int remainingElementsCount = max - min;
            string[] remainingElements = new string[remainingElementsCount];
            Array.Copy(absolutePathElements, min, remainingElements, 0, remainingElementsCount);
            string result = Path.Combine(remainingElements);
            return result;
        }
    }
}
