using System.Collections.Generic;

namespace Duktape
{
    public class ObjectCache
    {
        private int _index = 0;
        private Dictionary<int, object> _map = new Dictionary<int, object>();

        public int Add(object obj)
        {
            var id = ++_index;
            _map.Add(id, obj);
            return id;
        }

        public bool Remove(int id)
        {
            return _map.Remove(id);
        }

        public bool TryGetValue(int id, out object o)
        {
            return _map.TryGetValue(id, out o);
        }
    }
}