using System;
using System.Collections;
using System.Collections.Generic;

namespace RapidPliant.Collections
{
    public class ProcessOnceQueue<T> : IQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly Dictionary<T, T> _processed;

        public ProcessOnceQueue()
        {
            _queue = new Queue<T>();
            _processed = new Dictionary<T, T>();
        }

        public ProcessOnceQueue(IEnumerable<T> items)
            : this()
        {
            foreach (var item in items)
                Enqueue(item);
        }

        public int Count { get { return _queue.Count; } }
        public int EnqueuedCount { get { return _queue.Count; } }
        public IEnumerable<T> Enqueued { get { return _queue; } }

        public int ProcessedCount { get { return _processed.Count; } }
        public IEnumerable<T> Processed { get { return _processed.Keys; } }
        
        public bool Enqueue(T item)
        {
            if (_processed.ContainsKey(item))
                return false;

            _processed[item] = item;
            _queue.Enqueue(item);

            return true;
        }

        public T EnqueueOrGetExisting(T item)
        {
            bool isExisting;
            return EnqueueOrGetExisting(item, out isExisting);
        }
        
        public T EnqueueOrGetExisting(T item, out bool isExisting)
        {
            isExisting = true;
            if (!_processed.ContainsKey(item))
            {
                isExisting = true;
                _processed[item] = item;
                _queue.Enqueue(item);
                return item;
            }
            return _processed[item];
        }

        public T Dequeue()
        {
            return _queue.Dequeue();
        }

        public T Peek()
        {
            return _queue.Peek();
        }

        public T[] ToArray()
        {
            return _queue.ToArray();
        }

        public void TrimExcess()
        {
            _queue.TrimExcess();
        }

        public void Clear()
        {
            _queue.Clear();
            _processed.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_queue).CopyTo(array, index);
        }
        
        public object SyncRoot
        {
            get
            {
                return ((ICollection)_queue).SyncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ((ICollection)_queue).IsSynchronized;
            }
        }
    }
}
