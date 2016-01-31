using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Server
{
    public class InterLockedList<T>
    {
        private List<T> collection;

        public InterLockedList()
        {
            // Init a global collection using a volatile write.
            Interlocked.CompareExchange(ref collection, new List<T>(), null);
        }

        public void add(T item)
        {
            while (true)
            {
                // Volatile read of collection
                var original = Interlocked.CompareExchange(ref collection, null, null);

                // add new item to local copy
                var copy = original.ToList();
                copy.Add(item);

                // swap local copy with global collection
                // unless outraced by another thread
                var result = Interlocked.CompareExchange(ref collection, copy, original);
                if (result == original)
                    break;
            }
        }

        public override string ToString()
        {
            // Volatile read of global collection
            var original = Interlocked.CompareExchange(ref collection, null, null);

            // since content won't be modified during
            // the call of this method, read directly
            var ret = collection.Select(x => x.ToString()).Aggregate((x, xs) => x + "\n" + xs);
            //Console.WriteLine(ret);
            return ret;
        }
    }
}
