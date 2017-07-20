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

        public void Set(T typedElement, object o)
        {
            if (typedList.Contains(typedElement) || objectList.Contains(o))
                return;

            typedList.Add(typedElement);
            objectList.Add(o);
        }

        public T GetTypedElementOf(object o)
        {
            int index = objectList.IndexOf(o);
            if (index == -1)
                throw new ArgumentException("not in the list", "o");
            return typedList[index];
        }

        public object GetObjectOf(T typedElement)
        {
            int index = typedList.IndexOf(typedElement);
            if (index == -1)
                throw new ArgumentException("not in the list", "typedElement");
            return objectList[index];
        }
    }
}
