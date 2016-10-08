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
        public TValue[] Values { get; set; }

        public bool AddIfNotExists(TKey key, TValue value)
        {
            return false;
        }

        public TValue AddOrGetExisting(TKey key, Func<TValue> valueFn)
        {
            return valueFn();
        }
    }
}
