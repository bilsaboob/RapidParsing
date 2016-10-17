using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util
{
    public class RapidHashSet<T> : IEnumerable<T>
    {
        private static readonly T[] _emptyArray = new T[0];

        private int _initialCapacity;

        private int[] _buckets;
        private Entry[] _entries;
        private int _lastEntryIndex;
        private int _freeList;
        private int _count;
        private int _version;

        public RapidHashSet()
        {
        }
        
        public int Count { get { return _count; } }
        
        public bool AddIfNotExists(T item)
        {
            if (_buckets == null)
                IncreaseCapacity();

            //Get the item hashcode and which bucket to look in!
            var hashCode = GetItemHashCode(item);
            var bucketIndex = hashCode % _buckets.Length;

            //Check if the item already exists!
            for (int i = bucketIndex - 1; i >= 0; i = _entries[i].next)
            {
                if (_entries[i].hashCode == hashCode)
                {
                    if (ItemEquals(_entries[i].value, item))
                    {
                        return false;
                    }
                }
            }

            int freeSlotIndex;
            if (_freeList >= 0)
            {
                freeSlotIndex = _freeList;
                _freeList = _entries[freeSlotIndex].next;
            }
            else
            {
                if (_lastEntryIndex == _entries.Length)
                {
                    IncreaseCapacity();
                    bucketIndex = hashCode % _buckets.Length;
                }
                freeSlotIndex = _lastEntryIndex;
                _lastEntryIndex = _lastEntryIndex + 1;
            }

            var entryIndex = _buckets[bucketIndex];
            var entry = _entries[freeSlotIndex];
            entry.hashCode = hashCode;
            entry.value = item;
            entry.next = entryIndex - 1;
            _buckets[bucketIndex] = freeSlotIndex + 1;
            _count++;
            _version++;
            
            return true;
        }

        public bool Exists(T item)
        {
            if (_buckets == null)
                return false;

            var hashCode = GetItemHashCode(item);
            var bucketIndex = hashCode % _buckets.Length;
            
            for (int i = bucketIndex - 1; i >= 0; i = _entries[i].next)
            {
                if (_entries[i].hashCode == hashCode)
                {
                    if (ItemEquals(_entries[i].value, item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Clear()
        {
            if (_lastEntryIndex > 0)
            {
                Array.Clear(_entries, 0, _lastEntryIndex);
                Array.Clear(_buckets, 0, _buckets.Length);
                _lastEntryIndex = 0;
                _count = 0;
                _freeList = -1;
            }

            _version++;
        }

        private void IncreaseCapacity()
        {
            if (_buckets == null)
            {
                var prime = Primes.GetPrime(0);
                _buckets = new int[prime];
                _entries = new Entry[prime];
                return;
            }

            var newSize = Primes.GetPrime(_buckets.Length * 2);
            SetCapacity(newSize);
        }

        private bool ItemEquals(T item1, T item2)
        {
            object obj1 = item1;
            object obj2 = item2;

            if (obj1 == null)
            {
                if (obj2 == null)
                    return true;
                return false;
            }
            else
            {
                if (obj2 == null)
                    return false;

                return obj1 == obj2;
            }
        }

        private int GetItemHashCode(T item)
        {
            if (item == null)
                return 0;

            return item.GetHashCode();
        }

        protected void SetCapacity(int newCapacity)
        {
            var newEntries = new Entry[newCapacity];
            if (_entries != null)
                Array.Copy(_entries, 0, newEntries, 0, _lastEntryIndex);

            var newBuckets = new int[newCapacity];
            for (int entryIndex = 0; entryIndex < _lastEntryIndex; ++entryIndex)
            {
                int bucketsIndex = newEntries[entryIndex].hashCode % newCapacity;
                newEntries[entryIndex].next = newBuckets[bucketsIndex] - 1;
                newBuckets[bucketsIndex] = entryIndex + 1;
            }

            _entries = newEntries;
            _buckets = newBuckets;
        }

        private struct Entry
        {
            internal int hashCode;
            internal T value;
            internal int next;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new RapicHashSetEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        [Serializable]
        public struct RapicHashSetEnumerator : IEnumerator<T>
        {
            private RapidHashSet<T> _set;
            private int _index;
            private T _current;
            
            public RapicHashSetEnumerator(RapidHashSet<T> set)
            {
                _set = set;
                _index = 0;
                _current = default(T);
            }

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

            public bool MoveNext()
            {
                var lastEntryIndex = _set._lastEntryIndex;
                for (var i = _index; _index < lastEntryIndex; i++)
                {
                    if (_set._entries[i].hashCode >= 0)
                    {
                        _current = _set._entries[i].value;
                        _index = i+1;
                        return true;
                    }
                }

                _index = _set._lastEntryIndex + 1;
                _current = default(T);
                return false;
            }

            public void Dispose()
            {
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default(T);
            }
        }
    }
}
