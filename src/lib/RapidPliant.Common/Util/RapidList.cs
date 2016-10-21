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
        int Count { get; }
    }

    public class RapidList<T> : IRapidList<T>
    {
        protected static readonly T[] _emptyArray = new T[0];

        protected T[] _items;
        protected int _count;
        protected int _initialCapacity;
        protected int _version;

        public RapidList()
            : this(4)
        {
        }

        public RapidList(IEnumerable<T> items)
            : this(items.Count())
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public RapidList(int capacity)
        {
            _initialCapacity = capacity;
            _items = _emptyArray;
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
            return new ArrayEnumerator<T>(_items, _count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RapidList<T> Clone()
        {
            return CloneThis();
        }

        protected virtual RapidList<T> CloneThis()
        {
            var other = new RapidList<T>();
            other._items = _emptyArray;
            if (_items.Length > 0)
            {
                var otherItems = new T[_items.Length];
                _items.CopyTo(otherItems, 0);
                other._items = otherItems;
            }
            other._count = _count;
            other._initialCapacity = _initialCapacity;
            other._version = _version;
            return other;
        }

        public virtual void FromArray(T[] values)
        {
            _items = values;
            _count = values.Length;
            _initialCapacity = _count;
            _version++;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(_count);

            if (_count > 0)
            {
                sb.Append(":");

                for (var i = 0; i < _count; ++i)
                {
                    sb.AppendLine(string.Format("{0},",_items[i].ToString()));
                }
            }

            return sb.ToString();
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

        public new CachingRapidList<T> Clone()
        {
            return (CachingRapidList<T>)CloneThis();
        }

        protected override RapidList<T> CloneThis()
        {
            var other = new CachingRapidList<T>();
            other._items = _emptyArray;
            if (_items.Length > 0)
            {
                var otherItems = new T[_items.Length];
                _items.CopyTo(otherItems, 0);
                other._items = otherItems;
            }
            other._count = _count;
            other._initialCapacity = _initialCapacity;
            other._version = _version;
            other._itemsCachedVersion = _itemsCachedVersion;
            other._itemsCached = _itemsCached;
            return other;
        }

        public override T[] ToArray()
        {
            if (_itemsCached != null)
                return _itemsCached;

            _itemsCached = base.ToArray();
            _itemsCachedVersion = _version;
            return _itemsCached;
        }

        public override void FromArray(T[] values)
        {
            base.FromArray(values);

            //Use the same cached values as the base array
            _itemsCached = null;
            _itemsCachedVersion = -1;
        }
    }
}
