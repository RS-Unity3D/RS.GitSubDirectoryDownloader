using System;
using System.Collections;
using System.Collections.Generic;

namespace Octokit
{
    public class JsonArray : IList<object>
    {
        private readonly List<object> _list;

        public JsonArray()
        {
            _list = new List<object>();
        }

        public JsonArray(int capacity)
        {
            _list = new List<object>(capacity);
        }

        public JsonArray(IEnumerable<object> collection)
        {
            _list = new List<object>(collection);
        }

        public object this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public void Add(object item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(object item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(object item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(object item)
        {
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public List<T> ConvertAll<T>(Converter<object, T> converter)
        {
            return _list.ConvertAll(converter);
        }
    }
}