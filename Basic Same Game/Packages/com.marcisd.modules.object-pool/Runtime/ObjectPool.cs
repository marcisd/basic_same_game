using System.Collections.Generic;
using UnityEngine;

namespace MSD.Modules.ObjectPooling
{
    public class ObjectPool : MonoBehaviour
    {
        private readonly Dictionary<int, List<Transform>> _poolDictionary = new Dictionary<int, List<Transform>>();

        private readonly Dictionary<int, int> _instanceToPoolDictionaryLookup = new Dictionary<int, int>();

        public T Spawn<T>(T prefab) where T : MonoBehaviour, IPoolable
        {
            return Spawn(prefab, null);
        }

        public T Spawn<T>(T prefab, Transform parent) where T : MonoBehaviour, IPoolable
        {
            var pool = PoolForPrefab(prefab.transform);
            T output;
            if (pool.Count > 0) {
                Transform instance = pool[0];
                pool.Remove(instance);
                output = instance.GetComponent<T>();
            } else {
                T instance = Instantiate(prefab);
                AddInstanceToLookup(prefab.transform, instance.transform);
                output = instance;
            }
            output.OnAfterSpawn();
            output.transform.SetParent(parent, false);
            return output;
        }

        public void Despawn<T>(T instance) where T : MonoBehaviour, IPoolable
        {
            if (TryGetInstanceLookupValue(instance.transform, out int prefabId)) {
                if (_poolDictionary.TryGetValue(prefabId, out List<Transform> pool)
                    && gameObject != null && gameObject.activeInHierarchy) {
                    instance.OnBeforeDespawn(() => {
                        pool.Add(instance.transform);
                        instance.transform.SetParent(transform, false);
                        RemoveInstanceFromLookup(instance.transform);
                    });
                    return;
                }
            }
            Destroy(instance.gameObject);
        }

        private List<Transform> PoolForPrefab(Transform prefab)
        {
            if (_poolDictionary.ContainsKey(prefab.GetInstanceID())) {
                return _poolDictionary[prefab.GetInstanceID()];
            } else {
                List<Transform> pool = new List<Transform>();
                _poolDictionary.Add(prefab.GetInstanceID(), pool);
                return pool;
            }
        }

        private void AddInstanceToLookup(Transform prefab, Transform instance)
        {
            _instanceToPoolDictionaryLookup.Add(instance.GetInstanceID(), prefab.GetInstanceID());
        }

        private void RemoveInstanceFromLookup(Transform instance)
        {
            _instanceToPoolDictionaryLookup.Remove(instance.GetInstanceID());
        }

        private bool TryGetInstanceLookupValue(Transform instance, out int prefabId)
        {
            return _instanceToPoolDictionaryLookup.TryGetValue(instance.GetInstanceID(), out prefabId);
        }
    }
}