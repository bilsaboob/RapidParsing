using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util.Collections
{
    public abstract class RapidDictionary<TKey, TValue, TEntry> : IEnumerable<TEntry>
        where TEntry : RapidDictionary<TKey, TValue, TEntry>.Entry
    {
        private static readonly TValue[] _emptyArray = new TValue[0];

        private int _initialCapacity;

        private int[] _buckets;
        private TEntry[] _entries;
        private int _lastEntryIndex;
        private int _freeList;
        private int _count;
        private int _version;

        private RapidDictionaryEnumerable<TEntry> _entryEnumerable;
        private RapidDictionaryEnumerable<TKey> _keyEnumerable;
        private RapidDictionaryEnumerable<TValue> _valueEnumerable;

        public RapidDictionary()
        {
            _freeList = -1;
        }

        protected abstract TEntry CreateEntry();
        
        public int Count { get { return _count; } }

        public IEnumerable<TEntry> Entries
        {
            get { return this; }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                if(_keyEnumerable == null)
                    _keyEnumerable = new RapidDictionaryEnumerable<TKey>(() => new RapidDictionaryKeyEnumerator(this));
                return _keyEnumerable;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                if (_valueEnumerable == null)
                    _valueEnumerable = new RapidDictionaryEnumerable<TValue>(() => new RapidDictionaryValueEnumerator(this));
                return _valueEnumerable;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                EnsureBuckets();

                var entry = FindEntry(key);
                if (entry == null)
                    return default(TValue);

                if (!entry.hasValue)
                    return default(TValue);

                return entry.value;
            }
            set
            {
                var entry = GetEntry(key);
                entry.SetValue(value);
            }
        }

        public bool AddIfNotExists(TKey key, TValue value)
        {
            EnsureBuckets();

            //Get the item hashcode and which bucket to look in!
            var keyHashCode = GetKeyHashCode(key);
            var bucketIndex = GetBucketIndex(keyHashCode);

            var entry = FindEntry(key, keyHashCode, bucketIndex);
            if (entry != null)
                return false;

            entry = GetNextNewEntry(key, keyHashCode, bucketIndex);

            //Set the key and value!
            entry.key = key;
            entry.SetValue(value);

            _count++;
            _version++;

            return true;
        }

        public TEntry GetEntry(TKey key)
        {
            EnsureBuckets();

            //Get the item hashcode and which bucket to look in!
            var keyHashCode = GetKeyHashCode(key);
            var bucketIndex = GetBucketIndex(keyHashCode);

            var entry = FindEntry(key, keyHashCode, bucketIndex);
            if (entry == null)
            {
                entry = GetNextNewEntry(key, keyHashCode, bucketIndex);
                
                //Set the key and value!
                entry.key = key;

                _count++;
                _version++;
            }

            return entry;
        }
        
        public bool Exists(TKey key)
        {
            if (_buckets == null)
                return false;

            var keyHashCode = GetKeyHashCode(key);
            var bucketIndex = GetBucketIndex(keyHashCode);

            TEntry entry = null;
            for (int i = _buckets[bucketIndex] - 1; i >= 0; i = entry.next)
            {
                entry = _entries[i];
                if (entry == null)
                    return false;

                if (entry.keyHashCode == keyHashCode)
                {
                    if (KeyEquals(entry.key, key))
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

        private TEntry FindEntry(TKey key)
        {
            var keyHashCode = GetKeyHashCode(key);
            var bucketIndex = GetBucketIndex(keyHashCode);
            return FindEntry(key, keyHashCode, bucketIndex);
        }
        
        private TEntry FindEntry(TKey key, int keyHashCode, int bucketIndex)
        {
            //Check if the item already exists!
            TEntry entry = null;
            for (int i = _buckets[bucketIndex] - 1; i >= 0; i = entry.next)
            {
                entry = _entries[i];
                if (entry == null)
                    return null;
                
                if (entry.keyHashCode == keyHashCode)
                {
                    if (KeyEquals(entry.key, key))
                    {
                        return entry;
                    }
                }
            }

            return default(TEntry);
        }

        private TEntry GetNextNewEntry(TKey key, int keyHashCode, int bucketIndex)
        {
            int freeEntryIndex;
            TEntry entry = null;
            if (_freeList >= 0)
            {
                freeEntryIndex = _freeList;
                entry = _entries[freeEntryIndex];
                if (entry != null)
                {
                    _freeList = entry.next;
                }
            }
            else
            {
                if (_lastEntryIndex == _entries.Length)
                {
                    IncreaseCapacity();
                    bucketIndex = keyHashCode % _buckets.Length;
                }
                freeEntryIndex = _lastEntryIndex;
                _lastEntryIndex = _lastEntryIndex + 1;
            }
            
            if (entry == null)
            {
                entry = _entries[freeEntryIndex];
            }

            if (entry == null)
            {
                entry = CreateEntry();
                entry.keyHashCode = keyHashCode;
                _entries[freeEntryIndex] = entry;
            }
            
            entry.key = key;

            var entryIndex = _buckets[bucketIndex];
            entry.next = entryIndex - 1;
            
            _buckets[bucketIndex] = freeEntryIndex + 1;
            
            return entry;
        }

        private bool KeyEquals(TKey item1, TKey item2)
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

                return obj1.Equals(obj2);
            }
        }

        private int GetKeyHashCode(TKey key)
        {
            if (key == null)
                return 0;

            return key.GetHashCode();
        }

        private int GetBucketIndex(int keyHashCode)
        {
            return (int)((uint)keyHashCode % (uint)_buckets.Length);
        }

        private void EnsureBuckets()
        {
            if (_buckets == null)
                IncreaseCapacity();
        }

        private void IncreaseCapacity()
        {
            if (_buckets == null)
            {
                var prime = Primes.GetPrime(0);
                _buckets = new int[prime];
                _entries = new TEntry[prime];
                return;
            }

            var newSize = Primes.GetPrime(_buckets.Length * 2);
            SetCapacity(newSize);
        }

        protected void SetCapacity(int newCapacity)
        {
            var newEntries = new TEntry[newCapacity];
            if (_entries != null)
                Array.Copy(_entries, 0, newEntries, 0, _lastEntryIndex);

            var newBuckets = new int[newCapacity];
            for (int entryIndex = 0; entryIndex < _lastEntryIndex; ++entryIndex)
            {
                //Move the old item over
                var entry = _entries[entryIndex];
                newEntries[entryIndex] = entry; 
                
                var bucketsIndex = entry.keyHashCode % newCapacity;
                entry.next = newBuckets[bucketsIndex] - 1;
                newBuckets[bucketsIndex] = entryIndex + 1;
            }

            _entries = newEntries;
            _buckets = newBuckets;
        }
        
        public IEnumerator<TEntry> GetEnumerator()
        {
            return new RapidDictionaryEntryEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract class Entry
        {
            internal TKey key;
            internal TValue value;
            internal int keyHashCode;
            internal int next;
            internal bool hasValue;

            public TKey Key { get { return key; } }
            public TValue Value { get { return this.value; } }
            public bool HasValue { get { return hasValue; } }

            public void ClearValue()
            {
                value = default(TValue);
                hasValue = false;
            }

            public void SetValue(TValue value)
            {
                this.value = value;
                hasValue = true;
            }
        }

        [Serializable]
        protected abstract class RapidDictionaryEnumerator<T> : IEnumerator<T>
        {
            private RapidDictionary<TKey, TValue, TEntry> _dictionary;
            private int _index;
            private TEntry _currentEntry;
            private T _currentValue;

            public RapidDictionaryEnumerator(RapidDictionary<TKey, TValue, TEntry> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _currentEntry = default(TEntry);
            }

            public T Current
            {
                get { return _currentValue; }
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
                var lastEntryIndex = _dictionary._lastEntryIndex;
                for (var i = _index; _index < lastEntryIndex; i++)
                {
                    var entry = _dictionary._entries[i];
                    if(entry== null)
                        break;

                    if (entry.keyHashCode >= 0)
                    {
                        _currentEntry = _dictionary._entries[i];
                        _currentValue = GetValue(_currentEntry);
                        _index = i + 1;
                        return true;
                    }
                }

                _index = _dictionary._lastEntryIndex + 1;
                _currentEntry = default(TEntry);
                _currentValue = default(T);
                return false;
            }

            protected abstract T GetValue(TEntry entry);

            public void Dispose()
            {
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _currentEntry = default(TEntry);
            }
        }

        protected class RapidDictionaryEntryEnumerator : RapidDictionaryEnumerator<TEntry>
        {
            public RapidDictionaryEntryEnumerator(RapidDictionary<TKey, TValue, TEntry> dictionary) 
                : base(dictionary)
            {
            }

            protected override TEntry GetValue(TEntry entry)
            {
                return entry;
            }
        }

        protected class RapidDictionaryValueEnumerator : RapidDictionaryEnumerator<TValue>
        {
            public RapidDictionaryValueEnumerator(RapidDictionary<TKey, TValue, TEntry> dictionary) 
                : base(dictionary)
            {
            }

            protected override TValue GetValue(TEntry entry)
            {
                return entry.value;
            }
        }

        protected class RapidDictionaryKeyEnumerator : RapidDictionaryEnumerator<TKey>
        {
            public RapidDictionaryKeyEnumerator(RapidDictionary<TKey, TValue, TEntry> dictionary)
                : base(dictionary)
            {
            }

            protected override TKey GetValue(TEntry entry)
            {
                return entry.key;
            }
        }

        protected class RapidDictionaryEnumerable<T> : IEnumerable<T>
        {
            private Func<RapidDictionaryEnumerator<T>> _enumeratorFactoryFn;

            public RapidDictionaryEnumerable(Func<RapidDictionaryEnumerator<T>> enumeratorFactoryFn)
            {
                _enumeratorFactoryFn = enumeratorFactoryFn;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _enumeratorFactoryFn();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    public class RapidDictionary<TKey, TValue> : RapidDictionary<TKey, TValue, RapidDictionary<TKey, TValue>.RapidDictionaryEntry>
    {
        public RapidDictionary()
        {
        }

        protected override RapidDictionaryEntry CreateEntry()
        {
            return new RapidDictionaryEntry();
        }

        public class RapidDictionaryEntry : Entry
        { 
        }
    }
}
