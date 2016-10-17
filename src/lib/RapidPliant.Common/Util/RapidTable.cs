using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Common.Util
{
    public interface IRapidTable<in TKey, out TValue>
    {
    }

    public class RapidTable<TKey, TValue> : IRapidTable<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _items;
        private TValue[] _valuesCached;

        public RapidTable()
        {
            _items = new Dictionary<TKey, TValue>();
            _valuesCached = null;
        }

        public TValue[] Values
        {
            get
            {
                if (_valuesCached == null)
                {
                    _valuesCached = _items.Values.ToArray();
                }
                return _valuesCached;
            }
        }

        public int Count { get { return _items.Count; } }

        public bool AddIfNotExists(TKey key, TValue value)
        {
            if (_items.ContainsKey(key))
                return false;

            _items[key] = value;
            return true;
        }

        public TValue AddOrGetExisting(TKey key, Func<TValue> valueFn)
        {
            TValue value;

            if (!_items.TryGetValue(key, out value))
            {
                value = valueFn();
                _items[key] = value;
            }

            return value;
        }
    }
}
