﻿using System;
using System.Collections.Generic;

namespace Haumea_Core.Collections
{
    /// <summary>
    /// Bidirectional dictionary.
    /// </summary>
    public class BiDictionary<T1, T2>
    {
        private readonly IDictionary<T1, T2> _forward;
        private readonly IDictionary<T2, T1> _backward;

        public BiDictionary()
        {
            _forward  = new Dictionary<T1, T2>();
            _backward = new Dictionary<T2, T1>();
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward [t1] = t2;
            _backward[t2] = t1;
        }

        public void Add(T2 t2, T1 t1)
        {
            Add(t1, t2);
        }

        public bool Remove(T1 t1, T2 t2)
        {
            return _forward.Remove(t1) && _backward.Remove(t2);
        }

        public bool Remove(T2 t2, T1 t1)
        {
            return Remove(t1, t2);
        }

        public bool Contains(T1 t1)
        {
            return _forward.ContainsKey(t1);
        }

        public bool Contains(T2 t2)
        {
            return _backward.ContainsKey(t2);
        }

        public int Count
        {
            get { return _forward.Count; }
        }

        public T1 this [T2 key]
        {
            get { return _backward[key]; }
            set { _backward[key] = value; }
        }

        public T2 this [T1 key]
        {
            get { return _forward[key]; }
            set { _forward[key] = value; }
        }

        public bool Remove(T1 key){ return Remove(key, this[key]); }
        public bool Remove(T2 key){ return Remove(key, this[key]); }

        public bool TryGetValue(T1 key, out T2 value)
        {
            if (Contains(key)) {
                value = this[key];
                return true;
            } else {
                value = default(T2);
                return false;
            }
        }

        public bool TryGetValue(T2 key, out T1 value)
        {
            if (Contains(key)) {
                value = this[key];
                return true;
            } else {
                value = default(T1);
                return false;
            }
        }

        public void Clear()
        {
            _forward.Clear();
            _backward.Clear();
        }
    }
}