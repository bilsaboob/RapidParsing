using System;
using System.Collections;
using System.Collections.Generic;

namespace RapidPliant.Collections
{
    [Serializable]
    public struct ArrayEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
    {
        private T[] _items;
        private int _index;
        private T _current;
        private int _size;
        
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
            : this(itemsArray, itemsArray != null?itemsArray.Length:0)
        {
        }

        public ArrayEnumerator(T[] itemsArray, int size)
        {
            _items = itemsArray;
            _index = 0;
            _current = default(T);
            _size = size;
        }
        
        public bool MoveNext()
        {
            if (_index >= _size)
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
