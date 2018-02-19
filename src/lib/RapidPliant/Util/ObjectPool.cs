using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Collections;

namespace RapidPliant.Util
{
    public class ObjectPool<TObj>
        where TObj : class
    {
        private readonly Queue<Entry> _queue;
        private readonly ObjectFactory _factory;

        public delegate TObj ObjectFactory();

        public ObjectPool(int size, ObjectFactory factory)
        {
            _factory = factory;
            _queue = new Queue<Entry>(size);
        }

        public ObjectPool(ObjectFactory factory)
            : this(20, factory)
        {
        }

        public TObj GetInstance(out bool isClean)
        {
            isClean = true;

            if (_queue.Count == 0)
                return CreateInstance();

            var entry = _queue.Dequeue();
            if (entry == null)
                return default(TObj);

            var value = entry.Value;
            isClean = entry.IsClean;

            return value;
        }

        private TObj CreateInstance()
        {
            return _factory();
        }

        public void Free(TObj value, bool isClean)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var entry = new Entry() {
                Value = value,
                IsClean = isClean
            };

            _queue.Enqueue(entry);
        }

        private class Entry
        {
            public bool IsClean { get; set; }

            public TObj Value { get; set; }
        }
    }

    public static class Reusable
    {
        private static int DefaultSize = 20;

        #region Helpers
        public static ObjectPool<T> For<T>()
            where T : class, new()
        {
            return SingletonObjectPool<T>.Instance;
        }

        public sealed class SingletonObjectPool<TObj>
            where TObj : class, new()
        {
            public ObjectPool<TObj> ObjectPool { get; set; }

            private SingletonObjectPool()
            {
                ObjectPool = new ObjectPool<TObj>(DefaultSize, () => new TObj());
            }

            public static ObjectPool<TObj> Instance { get { return Nested.Instance.ObjectPool; } }

            private class Nested
            {
                static Nested()
                {
                }

                internal static readonly SingletonObjectPool<TObj> Instance = new SingletonObjectPool<TObj>();
            }
        }
        #endregion
    }

    public static class ReusableQueue<T>
    {
        public static Queue<T> GetAndClear()
        {
            return Reusable.For<Queue<T>>().GetAndClear();
        }
    }

    public static class ReusableUniqueList<T>
    {
        public static UniqueList<T> GetAndClear()
        {
            return Reusable.For<UniqueList<T>>().GetAndClear();
        }
    }

    public static class ReusableHashSet<T>
    {
        public static HashSet<T> GetAndClear()
        {
            return Reusable.For<HashSet<T>>().GetAndClear();
        }
    }

    public static class ReusableProcessOnceQueue<T>
    {
        public static ProcessOnceQueue<T> GetAndClear()
        {
            return Reusable.For<ProcessOnceQueue<T>>().GetAndClear();
        }
    }

    public static class ReusableSortedSet<T>
    {
        public static SortedSet<T> GetAndClear()
        {
            return Reusable.For<SortedSet<T>>().GetAndClear();
        }
    }

    public static class ReusableStringBuilder
    {
        public static StringBuilder GetAndClear()
        {
            return Reusable.For<StringBuilder>().GetAndClear();
        }
    }

    public static class ReusableDictionary<TKey, TValue>
    {
        public static Dictionary<TKey, TValue> GetAndClear()
        {
            return Reusable.For<Dictionary<TKey, TValue>>().GetAndClear();
        }
    }

    public static class ReusableList<T>
    {
        public static List<T> GetAndClear()
        {
            return Reusable.For<List<T>>().GetAndClear();
        }

        public static List<T> GetAndClear(IEnumerable<T> items)
        {
            return Reusable.For<List<T>>().GetAndClear(items);
        }

        public static void ClearAndFree(List<T> list)
        {
            Reusable.For<List<T>>().ClearAndFree(list);
        }
    }

    public static class ObjectPoolExtensions
    {
        #region Queue<T>

        public static Queue<T> GetAndClear<T>(this ObjectPool<Queue<T>> pool)
        {
            bool isClean;
            var queue = pool.GetInstance(out isClean);
            if (!isClean)
                queue.Clear();
            return queue;
        }

        public static void ClearAndFree<T>(this Queue<T> queue)
        {
            if (queue == null)
                return;

            Reusable.For<Queue<T>>().ClearAndFree(queue);
        }

        public static void ClearAndFree<T>(this ObjectPool<Queue<T>> pool, Queue<T> queue)
        {
            if (queue == null)
                return;

            queue.Clear();

            if (pool == null)
                return;

            pool.Free(queue, true);
        }

        #endregion Queue<T>

        #region UniqueList<T>

        public static UniqueList<T> GetAndClear<T>(this ObjectPool<UniqueList<T>> pool)
        {
            bool isClean;
            var list = pool.GetInstance(out isClean);
            if (!isClean)
                list.Clear();
            return list;
        }

        public static void ClearAndFree<T>(this UniqueList<T> list)
        {
            if (list == null)
                return;

            Reusable.For<UniqueList<T>>().ClearAndFree(list);
        }

        public static void ClearAndFree<T>(this ObjectPool<UniqueList<T>> pool, UniqueList<T> list)
        {
            if (list == null)
                return;

            list.Clear();

            if (pool == null)
                return;

            pool.Free(list, true);
        }

        #endregion UniqueList<T>

        #region HashSet<T>

        public static HashSet<TValue> GetAndClear<TValue>(this ObjectPool<HashSet<TValue>> pool)
        {
            bool isClean;
            var hashSet = pool.GetInstance(out isClean);
            if (!isClean)
                hashSet.Clear();
            return hashSet;
        }

        public static void ClearAndFree<TValue>(this HashSet<TValue> hashSet)
        {
            if (hashSet == null)
                return;

            Reusable.For<HashSet<TValue>>().ClearAndFree(hashSet);
        }

        public static void ClearAndFree<TValue>(this ObjectPool<HashSet<TValue>> pool, HashSet<TValue> hashSet)
        {
            if (hashSet == null)
                return;

            hashSet.Clear();

            if (pool == null)
                return;

            pool.Free(hashSet, true);
        }

        #endregion HashSet<T>

        #region ProcessOnceQueue<T>

        public static ProcessOnceQueue<TValue> GetAndClear<TValue>(this ObjectPool<ProcessOnceQueue<TValue>> pool)
        {
            bool isClean;
            var queue = pool.GetInstance(out isClean);
            if (!isClean)
                queue.Clear();
            return queue;
        }

        public static void ClearAndFree<TValue>(this ProcessOnceQueue<TValue> processOnceQueue)
        {
            if (processOnceQueue == null)
                return;

            Reusable.For<ProcessOnceQueue<TValue>>().ClearAndFree(processOnceQueue);
        }

        public static void ClearAndFree<TValue>(this ObjectPool<ProcessOnceQueue<TValue>> pool, ProcessOnceQueue<TValue> processOnceQueue)
        {
            if (processOnceQueue == null)
                return;

            processOnceQueue.Clear();

            if (pool == null)
                return;

            pool.Free(processOnceQueue, true);
        }

        #endregion SortedSet<T>

        #region SortedSet<T>

        public static SortedSet<TValue> GetAndClear<TValue>(this ObjectPool<SortedSet<TValue>> pool)
        {
            bool isClean;
            var set = pool.GetInstance(out isClean);
            if (!isClean)
                set.Clear();
            return set;
        }

        public static void ClearAndFree<T>(this SortedSet<T> set)
        {
            if (set == null)
                return;

            Reusable.For<SortedSet<T>>().ClearAndFree(set);
        }

        public static void ClearAndFree<TValue>(this ObjectPool<SortedSet<TValue>> pool, SortedSet<TValue> set)
        {
            if (set == null)
                return;

            set.Clear();

            if (pool == null)
                return;

            pool.Free(set, true);
        }

        #endregion SortedSet<T>

        #region Dictionary<TKey, TValue>

        public static Dictionary<TKey, TValue> GetAndClear<TKey, TValue>(this ObjectPool<Dictionary<TKey, TValue>> pool)
        {
            bool isClean;
            var dictionary = pool.GetInstance(out isClean);
            if (!isClean)
                dictionary.Clear();
            return dictionary;
        }

        public static void ClearAndFree<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return;

            Reusable.For<Dictionary<TKey, TValue>>().ClearAndFree(dictionary);
        }

        public static void ClearAndFree<TKey, TValue>(this ObjectPool<Dictionary<TKey, TValue>> pool, Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return;

            dictionary.Clear();

            if (pool == null)
                return;

            pool.Free(dictionary, true);
        }

        #endregion Dictionary<TKey, TValue>

        #region List<T>

        public static List<T> GetAndClear<T>(this ObjectPool<List<T>> pool)
        {
            bool isClean;
            var list = pool.GetInstance(out isClean);
            if (!isClean)
                list.Clear();
            return list;
        }

        public static List<T> GetAndClear<T>(this ObjectPool<List<T>> pool, IEnumerable<T> items)
        {
            bool isClean;
            var list = pool.GetInstance(out isClean);
            if (!isClean)
                list.Clear();
            list.AddRange(items);
            return list;
        }

        public static void ClearAndFree<T>(this List<T> list)
        {
            if (list == null)
                return;

            Reusable.For<List<T>>().ClearAndFree(list);
        }

        public static void ClearAndFree<T>(this ObjectPool<List<T>> pool, List<T> list)
        {
            if (list == null)
                return;

            list.Clear();

            if (pool == null)
                return;

            pool.Free(list, true);
        }

        #endregion List<T>

        #region StringBuilder

        public static StringBuilder GetAndClear(this ObjectPool<StringBuilder> pool)
        {
            bool isClean;
            var builder = pool.GetInstance(out isClean);
            if (!isClean)
                builder.Clear();
            return builder;
        }

        public static void ClearAndFree(this StringBuilder builder)
        {
            if (builder == null)
                return;

            Reusable.For<StringBuilder>().ClearAndFree(builder);
        }

        public static void ClearAndFree(this ObjectPool<StringBuilder> pool, StringBuilder builder)
        {
            if (builder == null)
                return;

            builder.Clear();

            if (pool == null)
                return;

            pool.Free(builder, true);
        }

        #endregion StringBuilder
    }
}
