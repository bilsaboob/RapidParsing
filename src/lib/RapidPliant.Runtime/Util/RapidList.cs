using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Util
{
    public class RapidList<T>
    {
        private T[] _items;
        private int _count;

        public int Count { get { return _count; } }

        public T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }

        public void Add(T item)
        {
            
        }
    }
}
