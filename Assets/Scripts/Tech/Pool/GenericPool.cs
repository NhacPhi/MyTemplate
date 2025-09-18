using System.Collections.Generic;
using UnityEngine;

namespace Tech.Pool
{
    public class GenericPool<T> where T : class,new()
    {
        private static readonly Stack<T> _pool = new();

        public static T Get() => _pool.Count > 0 ? _pool.Pop() : new T();

        public static void Return(T item)
        {
            if (_pool.Contains(item) && item == null) return;
             
            _pool.Push(item);
        }

        public static void Clean()
        {
            _pool.Clear();
        }

        public static void PrePool(int count)
        {
            for(int i = 0;  i < count; i++)
            {
                _pool.Push(new T());
            }
        }
    }

}


