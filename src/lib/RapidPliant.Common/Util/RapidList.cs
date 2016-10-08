using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Common.Rule;
using RapidPliant.Runtime.Util;

namespace RapidPliant.Common.Util
{
    public interface IRapidList<out T> : IEnumerable<T>
    {
    }

    public class RapidList<T> : IRapidList<T>
    {
        private static readonly T[] _emptyArray = new T[0];

        private T[] _items;
        private int _count;
        private int _initialCapacity;
        protected int _version;

        public RapidList()
            : this(4)
        {
        }

        public RapidList(int capacity)
        {
            _initialCapacity = capacity;
        }

        public int Count { get { return _count; } }

        public int Capacity { get { return _items.Length; } set { SetCapacity(value); } }
        
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
            EnsureCapacity(_count + 1);

            _items[_count++] = item;
            _version++;
        }

        public void Clear()
        {
            //just reset the index!
            _count = 0;
            _version++;
        }

        protected void EnsureCapacity(int capacity)
        {
            if (_items.Length >= capacity)
                return;

            var newCapacity = _items.Length == 0 ? _initialCapacity : _items.Length * 2;

            if ((uint)newCapacity > 2146435071U)
                newCapacity = 2146435071;

            if (newCapacity < capacity)
                newCapacity = capacity;

            SetCapacity(newCapacity);
        }

        protected void SetCapacity(int newCapacity)
        {
            if (newCapacity == _items.Length)
                return;

            if (newCapacity == 0)
            {
                _items = _emptyArray;
                return;
            }

            var newItems = new T[newCapacity];

            if (_count > 0)
                Array.Copy(_items, 0, newItems, 0, _count);

            _items = newItems;
        }

        public virtual T[] ToArray()
        {
            var array = new T[_count];
            Array.Copy(_items, array, _count);
            return array;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ArrayEnumerator<T>(_items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CachingRapidList<T> : RapidList<T>
    {
        private int _itemsCachedVersion;
        private T[] _itemsCached;

        public virtual T[] AsArray
        {
            get
            {
                if (_itemsCached == null || _itemsCachedVersion != _version)
                {
                    _itemsCached = ToArray();
                    _itemsCachedVersion = _version;
                }
                return _itemsCached;
            }
        }

        public override T[] ToArray()
        {
            if (_itemsCached != null)
                return _itemsCached;

            _itemsCached = base.ToArray();
            _itemsCachedVersion = _version;
            return _itemsCached;
        }
    }
}
