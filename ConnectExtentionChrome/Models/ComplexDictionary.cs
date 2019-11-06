using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectExtentionChrome
{
    public class ComplexDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IList<TValue>
    {
        public TValue this[int index]
        {
            get => Values.ElementAt(index);
            set
            {
                var key = Keys.ElementAt(index);
                this[key] = value;
            }
        }

        public bool IsReadOnly => IsReadOnly;

        [Obsolete]
        public void Add(TValue item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TValue item)
        {
            return ContainsValue(item);
        }

        [Obsolete]
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public int IndexOf(TValue item)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public void Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public bool Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            RemoveAt(index);
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
    }
}
