using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Collections
{
    public interface IQueue<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        bool Enqueue(T item);
        T Dequeue();
        T Peek();
    }
}
