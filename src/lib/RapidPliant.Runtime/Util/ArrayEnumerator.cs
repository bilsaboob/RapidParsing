using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Runtime.Util
{
    [Serializable]
    public struct ArrayEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
    {
        private T[] _items;
        private int _index;
        private T _current;
        
        public T Current
        {
            get
            {
                return _current;
            }
        }
        
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public ArrayEnumerator(T[] itemsArray)
        {
            _items = itemsArray;
            _index = 0;
            _current = default(T);
        }
        
        public bool MoveNext()
        {
            if (_index >= _items.Length)
            {
                _index = _items.Length + 1;
                _current = default(T);
                return false;
            }
                
            _current = _items[_index];
            _index = _index + 1;
            return true;
        }
        
        void IEnumerator.Reset()
        {
            _index = 0;
            _current = default(T);
        }

        public void Dispose()
        {
        }
    }
}
