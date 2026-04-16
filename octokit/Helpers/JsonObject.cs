using System;
using System.Collections;
using System.Collections.Generic;

namespace Octokit
{
    public class JsonObject : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _members;

        public JsonObject()
        {
            _members = new Dictionary<string, object>();
        }

        public JsonObject(IEqualityComparer<string> comparer)
        {
            _members = new Dictionary<string, object>(comparer);
        }

        public object this[string key]
        {
            get => _members[key];
            set => _members[key] = value;
        }

        public ICollection<string> Keys => _members.Keys;
        public ICollection<object> Values => _members.Values;
        public int Count => _members.Count;
        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _members.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _members.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _members.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_members).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _members.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _members.TryGetValue(key, out value);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_members).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _members.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_members).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _members.GetEnumerator();
        }
    }
}