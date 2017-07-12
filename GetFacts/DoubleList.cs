using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts
{
    public class DoubleList<T>
    {
        private readonly List<T> typedList = new List<T>();
        private readonly List<object> objectList = new List<object>();

        public void Clear()
        {
            typedList.Clear();
            objectList.Clear();
        }

        public ICollection<T> TypedElements
        {
            get { return typedList.AsReadOnly(); }
        }

        public void Add(T typedElement, object o)
        {
            typedList.Add(typedElement);
            objectList.Add(o);
        }

        public T GetTypedElementOf(object o)
        {
            int index = objectList.IndexOf(o);
            return typedList[index];
        }

        public object GetObjectOf(T typedElement)
        {
            int index = typedList.IndexOf(typedElement);
            return objectList[index];
        }
    }
}
