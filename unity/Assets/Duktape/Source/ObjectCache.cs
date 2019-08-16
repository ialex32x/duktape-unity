using System;
using System.Collections.Generic;

namespace Duktape
{
    public class EqualityComparer : IEqualityComparer<object>
    {
        public static readonly EqualityComparer Default = new EqualityComparer();

        public new bool Equals(object x, object y)
        {

            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class ObjectCache
    {
        private int _index = 0;

        // id => host object
        private Dictionary<int, object> _map = new Dictionary<int, object>();
        // host object => jsvalue heapptr (dangerous)
        private Dictionary<object, IntPtr> _rmap = new Dictionary<object, IntPtr>(EqualityComparer.Default);

        public void Clear()
        {
            _index = 0;
            _map.Clear();
            _rmap.Clear();
        }

        public void AddJSValue(object o, IntPtr heapptr)
        {
            if (o != null)
            {
                _rmap.Add(o, heapptr);
            }
        }

        public bool TryGetJSValue(object o, out IntPtr heapptr)
        {
            if (o == null)
            {
                heapptr = IntPtr.Zero;
                return false;
            }
            return _rmap.TryGetValue(o, out heapptr);
        }

        public bool RemoveJSValue(object o)
        {
            return o != null && _rmap.Remove(o);
        }

        public int AddWeakObject(object o)
        {
            return AddObject(new WeakReference(o));
        }

        public int AddObject(object o)
        {
            if (o != null)
            {
                var id = ++_index;
                _map.Add(id, o);
                return id;
            }
            return 0;
        }

        public bool RemoveObject(int id)
        {
            object o;
            if (_map.TryGetValue(id, out o))
            {
                var op1 = _map.Remove(id);
                RemoveJSValue(o);
                return op1;
            }
            return false;
        }

        // 覆盖已有记录, 无记录返回 false
        public bool ReplaceObject(int id, object o)
        {
            object oldValue;
            if (_map.TryGetValue(id, out oldValue))
            {
                _map[id] = o;
                //TODO: 是否成立?
                IntPtr heapptr;
                if (oldValue != null && _rmap.TryGetValue(oldValue, out heapptr))
                {
                    _rmap.Remove(oldValue);
                    _rmap[o] = heapptr;
                }
                return true;
            }
            return false;
        }

        public bool TryGetTypedWeakObject<T>(int id, out T o)
        where T : class
        {
            object obj;
            if (_map.TryGetValue(id, out obj))
            {
                var w = obj as WeakReference;
                o = w != null ? w.Target as T : null;
                return true;
            }
            o = null;
            return false;
        }

        public bool TryGetTypedObject<T>(int id, out T o)
        where T : class
        {
            object obj;
            if (_map.TryGetValue(id, out obj))
            {
                o = obj as T;
                return true;
            }
            o = null;
            return false;
        }

        public bool TryGetObject(int id, out object o)
        {
            return _map.TryGetValue(id, out o);
        }

        public bool MatchObjectType(int id, Type type)
        {
            object o;
            if (_map.TryGetValue(id, out o))
            {
                if (o != null)
                {
                    var otype = o.GetType();
                    return otype == type || otype.IsSubclassOf(type) || type.IsAssignableFrom(otype);
                }
                return true;
            }
            return false;
        }
    }
}