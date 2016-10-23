using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util.Collections
{
    public class RapidProcessOnceQueue<TKey, TValue>
    {
        private Queue<TValue> _queue;
        private Dictionary<TKey, TValue> _processed;

        public RapidProcessOnceQueue()
        {
            _queue = new Queue<TValue>();
            _processed = new Dictionary<TKey, TValue>();
        }

        public int ProcessedCount { get { return _processed.Count; } }
        public IEnumerable<TValue> Processed { get { return _processed.Values; } }

        public int EnqueuedCount { get { return _queue.Count; } }
        public IEnumerable<TValue> Enqueued { get { return _queue; } }

        public bool Enqueue(TKey key, TValue value)
        {
            if (_processed.ContainsKey(key))
                return false;
            
            _processed[key] = value;
            _queue.Enqueue(value);

            return true;
        }
        
        public TValue Dequeue()
        {
            return _queue.Dequeue();
        }

        public bool HasProcessed(TKey key)
        {
            return _processed.ContainsKey(key);
        }

        public bool TryGetProcessed(TKey key, out TValue value)
        {
            return _processed.TryGetValue(key, out value);
        }
    }
}
