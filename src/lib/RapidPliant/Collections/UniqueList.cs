using System.Collections;
using System.Collections.Generic;

namespace RapidPliant.Collections
{
    public class UniqueList<T> : IList<T>, IReadOnlyList<T>
    {
        private const int Threshold = 10;

        private HashSet<T> _set;
        private readonly List<T> _list;
        
        public UniqueList()
        {
            _list = new List<T>();
        }

        public UniqueList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        public UniqueList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
            if (HashSetIsMoreEfficient())
                AllocateAndPopulateHashSet();
        }

        public int Count { get { return _list.Count; } }

        public bool IsReadOnly { get { return false; } }

        public T this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            InsertIfNotExists(index, item);
        }

        public bool InsertIfNotExists(int index, T item)
        {
            if (HashSetIsMoreEfficient())
                return InsertIfNotExistsUsingHashSet(index, item);

            return InsertIfNotExistsUsingList(index, item);
        }

        private bool InsertIfNotExistsUsingHashSet(int index, T item)
        {
            AllocateAndPopulateHashSet();
            if (!_set.Add(item))
                return false;

            _list.Insert(index, item);
            return false;
        }

        public bool InsertIfNotExistsUsingList(int index, T item)
        {
            if (_list.Count == 0)
            {
                _list.Insert(index, item);
                return true;
            }

            var hashCode = item.GetHashCode();
            for (int i = 0; i < _list.Count; i++)
            {
                var listItem = _list[i];
                if (hashCode.Equals(listItem.GetHashCode()))
                    return false;
            }
            _list.Insert(index, item);
            if (HashSetIsMoreEfficient())
                AllocateAndPopulateHashSet();
            return true;
        }

        public void RemoveAt(int index)
        {
            if (HashSetIsMoreEfficient())
            {
                var item = _list[index];
                _set.Remove(item);
            }
            _list.RemoveAt(index);
        }

        public void Add(T item)
        {
            AddIfNotExists(item);
        }

        public bool AddIfNotExists(T item)
        {
            if (HashSetIsMoreEfficient())
                return AddUniqueUsingHashSet(item);
            return AddUniqueUsingList(item);
        }

        private bool AddUniqueUsingHashSet(T item)
        {
            AllocateAndPopulateHashSet();
            if (!_set.Add(item))
                return false;
            _list.Add(item);
            return true;
        }

        private bool AddUniqueUsingList(T item)
        {
            if (_list.Count == 0)
            {
                _list.Add(item);
                return true;
            }
            var hashCode = item.GetHashCode();
            for (int i = 0; i < _list.Count; i++)
            {
                var listItem = _list[i];
                if (hashCode.Equals(listItem.GetHashCode()))
                    return false;
            }
            _list.Add(item);
            if (HashSetIsMoreEfficient())
                AllocateAndPopulateHashSet();
            return true;
        }

        private void AllocateAndPopulateHashSet()
        {
            if (_set != null)
                return;
            _set = new HashSet<T>();
            for (int i = 0; i < _list.Count; i++)
                _set.Add(_list[i]);
        }

        public void Clear()
        {
            _list.Clear();
            if (_set != null)
                _set.Clear();
        }

        public bool Contains(T item)
        {
            AllocateAndPopulateHashSet();
            if (HashSetIsMoreEfficient())
                return _set.Contains(item);
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            AllocateAndPopulateHashSet();
            if (HashSetIsMoreEfficient())
            {
                _set.Remove(item);
            }
            return _list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        private bool HashSetIsMoreEfficient()
        {
            return _list.Count >= Threshold;
        }

        public override int GetHashCode()
        {
            return _list.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (((object)obj) == null)
                return false;
            var uniqueList = obj as UniqueList<T>;
            if (((object)uniqueList) == null)
                return false;
            return _list.Equals(uniqueList._list);
        }
    }
}
