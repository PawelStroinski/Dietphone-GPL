using System;
using System.Collections;
using System.Collections.Generic;

namespace Dietphone.Tools
{
    public class StateSerializer : IDictionary<string, object>
    {
        private IDictionary<string, string> storage;
        private const string TYPE_SUFFIX = "__TYPE__";
        private const string STRING = "STRING";

        public StateSerializer(IDictionary<string, string> storage)
        {
            this.storage = storage;
        }

        public object this[string key]
        {
            get
            {
                var value = storage[key];
                var typeName = storage[key + TYPE_SUFFIX];
                if (typeName == STRING)
                    return value;
                else
                {
                    var type = Type.GetType(typeName);
                    return value.Deserialize(string.Empty, type);
                }
            }
            set
            {
                if (value is string)
                {
                    storage[key] = (string)value;
                    storage[key + TYPE_SUFFIX] = STRING;
                }
                else
                {
                    storage[key] = value.Serialize(string.Empty);
                    storage[key + TYPE_SUFFIX] = value.GetType().AssemblyQualifiedName;
                }
            }
        }

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<object> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return storage.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            var removed = storage.Remove(key);
            if (removed)
                storage.Remove(key + TYPE_SUFFIX);
            return removed;
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
