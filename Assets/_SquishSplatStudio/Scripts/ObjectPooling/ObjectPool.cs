/* SquishSplatStudio - https://discord.gg/HVjBM9T
 * You are welcome to use this file or contents for learning and/or non-commercial projects
 * Written by Dubh @Dubh#1508 <Discord> - 2020
 */

using System.Collections.Generic;
using UnityEngine;

namespace SquishSplatStudio
{
    public class ObjectPool : MonoBehaviour
    {
        #region ---[ singleton code base ]---

        static ObjectPool() { }

        ObjectPool() { }

        public static ObjectPool Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        [SerializeField] List<ObjectPoolItem> _itemsToPool;
        [SerializeField] List<PlacementLink> _placementLinks;
        [SerializeField] List<GameObject> _pooledItems = new List<GameObject>();

        void Start()
        {
            for (var i = 0; i < _itemsToPool.Count; i++)
                for (int x = 0; x < _itemsToPool[i].AmountToLoad; x++)
                    AddItemToPool(_itemsToPool[i]);
        }

        GameObject AddItemToPool(ObjectPoolItem itemToAdd)
        {
            var obj = Instantiate(itemToAdd.Prefab, GetParent(itemToAdd.Type));
            obj.SetActive(false);
            _pooledItems.Add(obj);
            return obj;
        }

        Transform GetParent(PlacementType type)
        {
            for (var i = 0; i < _placementLinks.Count; i++)
                if (_placementLinks[i].Type == type)
                    return _placementLinks[i].Parent.transform;
            return null;
        }

        public GameObject GetItemPrefab(PlacementType placementType)
        {
            for (var i = 0; i < _itemsToPool.Count; i++)
            {
                if (_itemsToPool[i].Type == placementType)
                    return _itemsToPool[i].Prefab;
            }

            return null;
        }

        public GameObject GetInstantiatedPrefab(PlacementType placementType)
        {
            for (var i = 0; i < _itemsToPool.Count; i++)
            {
                if (_itemsToPool[i].Type == placementType)
                    return Instantiate(_itemsToPool[i].Prefab);
            }

            return null;
        }

        public GameObject GetItemFromPool(PlacementType placementType)
        {
            // ---[ return an available pooled item ]---
            for (var i = 0; i < _pooledItems.Count; i++)
                if (!_pooledItems[i].activeInHierarchy && _pooledItems[i].GetComponent<BuildRequirements>()?.ObjectType == placementType)
                    return _pooledItems[i];

            // ---[ create one where possible ]---
            for (var i = 0; i < _itemsToPool.Count; i++)
            {
                if (_itemsToPool[i].Type == placementType && _itemsToPool[i].CanAddMore)
                    return AddItemToPool(_itemsToPool[i]);
            }

            // ---[ unable to create or use existing ]---
            return null;
        }
    }
}
