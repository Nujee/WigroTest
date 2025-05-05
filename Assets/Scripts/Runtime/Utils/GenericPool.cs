using System.Collections.Generic;
using UnityEngine;

public sealed class GenericPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _pool;

    public GenericPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        _pool = new Queue<T>(initialSize);

        for (var i = 0; i < initialSize; i++)
        {
            var obj = CreateNewObject();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    private T CreateNewObject()
    {
        return Object.Instantiate(_prefab, _parent);
    }

    public T Get()
    {
        T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNewObject();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    public void Clear()
    {
        foreach (var obj in _pool)
        {
            Object.Destroy(obj.gameObject);
        }
        _pool.Clear();
    }
}