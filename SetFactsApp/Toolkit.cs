using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetFactsApp
{
    static class Toolkit
    {
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
