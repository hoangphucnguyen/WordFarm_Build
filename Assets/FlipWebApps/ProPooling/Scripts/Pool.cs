//----------------------------------------------
// Flip Web Apps: Pro Pooling
// Copyright © 2016-2017 Flip Web Apps / Mark Hewitt
//
// Please direct any bugs/comments/suggestions to http://www.flipwebapps.com
// 
// The copyright owner grants to the end user a non-exclusive, worldwide, and perpetual license to this Asset
// to integrate only as incorporated and embedded components of electronic games and interactive media and 
// distribute such electronic game and interactive media. End user may modify Assets. End user may otherwise 
// not reproduce, distribute, sublicense, rent, lease or lend the Assets. It is emphasized that the end 
// user shall not be entitled to distribute or transfer in any way (including, without, limitation by way of 
// sublicense) the Assets in any other way than as integrated components of electronic games and interactive media. 

// The above copyright notice and this permission notice must not be removed from any files.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//----------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProPooling
{
    /// <summary>
    /// Prefab pooling class that can preallocate a certain number of instances.
    /// </summary>
    [Serializable]
    public class Pool
    {
        /// <summary>
        /// The prefab / gameobject that we want to pool.
        /// </summary>
        [Tooltip("The prefab / gameobject that we want to pool.")]
        public GameObject Prefab;

        /// <summary>
        /// The number of instances that should be created when this pool is loaded.
        /// </summary>
        [Tooltip("The number of instances that should be created when this pool is loaded.")]
        public int PreInitialiseCount = 2;

        /// <summary>
        /// The initial number of slots in this pool. The pool can expand dynamically, but setting this appropriately at the start can give a performance increase.
        /// </summary>
        [Tooltip("The initial number of slots in this pool. The pool can expand dynamically, but setting this appropriately at the start can give a performance increase.")]
        public int InitialCapacity = 5;

        /// <summary>
        /// The maximum number of slots in this pool. More items can be created, but the inactive pool will only contain this number with any additional items destroyed when returned to the pool. This allows you to handle 'peaks' without holding on to unnecessary memory. 0 allows an unlimited number.
        /// </summary>
        [Tooltip("The maximum number of slots in this pool. More items can be created, but the inactive pool will only contain this number with any additional items destroyed when returned to the pool. This allows you to handle 'peaks' without holding on to unnecessary memory. 0 allows an unlimited number.")]
        public int MaxCapacity;

        /// <summary>
        /// An optional transform under which inactive items will be added.
        /// </summary>
        [Tooltip("An optional transform under which inactive items will be added.")]
        public Transform InactiveParent;

        //[Tooltip("Whether to skip sending lifecycle notifications to other game objects. Use this if you don't need lifecycle notifications to get some speed benefits.")]
        //public bool SkipLifeCycleNotifications = true;

        //[Tooltip("Only pool the gameobject giving some speed benefits. Use this if you don't need lifecycle notifications and does not need to get or override the pool item.")]
        //public bool PoolGameobjectOnly = true;

        /// <summary>
        /// Returns the number of inactive items in this pool
        /// </summary>
        public int InactiveCount { get { return _inactiveInstances.Count; } }

        /// <summary>
        /// Returns the number of in use instances from this pool
        /// </summary>
        public int InUseCount { get { return _inUseInstances.Count; } }

        /// <summary>
        /// Returns the instance ID of the prefab associated with this pool
        /// </summary>
        public int ID
        {
            get { return Prefab.GetInstanceID(); }
        }

        /// <summary>
        /// Returns the name of the prefab associated with this pool
        /// </summary>
        public string Name
        {
            get { return Prefab == null ? null : Prefab.name; }
        }

        Queue<PoolItem> _inactiveInstances;
        Dictionary<GameObject, PoolItem> _inUseInstances;

        /// <summary>
        /// Parameterless constructor to let us override and instantiate from within the pool editor
        /// </summary>
        public Pool() { } 


        /// <summary>
        /// Initialise this pool instance
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="preInitialiseCount"></param>
        /// <param name="initialCapacity"></param>
        /// <param name="maxCapacity"></param>
        /// <param name="inactiveParent"></param>
        public Pool(GameObject prefab, int preInitialiseCount = 0, int initialCapacity = 5, int maxCapacity = 0, Transform inactiveParent = null)
        {
            Assert.IsNotNull(prefab, "Prefab must not be null!");

            Prefab = prefab;
            PreInitialiseCount = preInitialiseCount;
            InitialCapacity = Mathf.Max(initialCapacity, PreInitialiseCount);
            MaxCapacity = maxCapacity;
            InactiveParent = inactiveParent;

            Initialise();
        }


        #region Initialisation and Cleanup

        /// <summary>
        /// Create the specified number of instances in an inactive state and add to the inactive list.
        /// </summary>
        public void Initialise()
        {
            _inactiveInstances = new Queue<PoolItem>(InitialCapacity);
            _inUseInstances = new Dictionary<GameObject, PoolItem>(InitialCapacity);

            for (var i = 0; i < PreInitialiseCount; i++)
            {
                var poolInstance = CreatePoolInstance(InactiveParent, Vector3.zero, Quaternion.identity);
                poolInstance.GameObject.SetActive(false);
                _inactiveInstances.Enqueue(poolInstance);
            }
        }


        /// <summary>
        /// Clear all inactive pooled items. Note we can't clear items that are already active.
        /// </summary>
        public void ClearPool()
        {
            while (_inactiveInstances.Count > 0)
            {
                var poolItem = _inactiveInstances.Dequeue();
                poolItem.OnDestroy();
#if UNITY_EDITOR
                // different for editor mode so we can run tests.
                if (!Application.isPlaying)
                    GameObject.DestroyImmediate(poolItem.GameObject);
                else
#endif
                    GameObject.Destroy(poolItem.GameObject);
            }
        }

#endregion Initialisation and Cleanup

#region GetFromPool

        /// <summary>
        /// Get a gameobject from the pool, optionally creating a new one if there aren't already enough available.
        /// </summary>
        /// Position and rotation will be set to the default values that the initial prefab had.
        /// <returns></returns>
        public GameObject GetFromPool(Transform parent = null)
        {
            return GetPoolItemFromPool(Prefab.transform.position, Prefab.transform.rotation, parent).GameObject;
        }


        /// <summary>
        /// Get a gameobject from the pool, optionally creating a new one if there aren't already enough available.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject GetFromPool(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return GetPoolItemFromPool(position, rotation, parent).GameObject;
        }



        /// <summary>
        /// Get a PoolItem from the pool, optionally creating a new one if there aren't already enough avilable.
        /// </summary>
        /// Position and rotation will be set to the default values that the initial prefab had.
        /// <returns></returns>
        public virtual PoolItem GetPoolItemFromPool(Transform parent = null)
        {
            return GetPoolItemFromPool(Prefab.transform.position, Prefab.transform.rotation, parent);
        }


        ///// <summary>
        ///// Get an item from the pool, optionally creating a new one if there aren't already enough avilable.
        ///// </summary>
        ///// <returns></returns>
        //public T GetPoolItemFromPool<T>(Vector3 position, Quaternion rotation, Transform parent = null) where T : PoolItem
        //{
        //    return GetPoolItemFromPool(position, rotation, parent = null) as T;
        //}


        /// <summary>
        /// Get a PoolItem from the pool, optionally creating a new one if there aren't already enough avilable.
        /// </summary>
        /// <returns></returns>
        public PoolItem GetPoolItemFromPool(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            PoolItem poolItem;
            if (_inactiveInstances.Count > 0)
            {
                poolItem = _inactiveInstances.Dequeue();
                poolItem.GameObject.transform.position = position;
                poolItem.GameObject.transform.rotation = rotation;
                if (parent != null)
                    poolItem.GameObject.transform.SetParent(parent, false);
            }
            else
            {
                poolItem = CreatePoolInstance(parent, position, rotation);
            }
            poolItem.GameObject.SetActive(Prefab.activeSelf);
            poolItem.OnGetFromPool();
            _inUseInstances.Add(poolItem.GameObject, poolItem);

            return poolItem;
        }

#endregion GetFromPool

#region ReturnToPool

        /// <summary>
        /// Return an item back to the queue. If we are above the maximum capacity then we just delete the item.
        /// </summary>
        /// <param name="poolItem"></param>
        /// <returns></returns>
        public bool ReturnToPool(PoolItem poolItem)
        {
            if (poolItem != null)
            {
                var gameObject = poolItem.GameObject;
                _inUseInstances.Remove(gameObject);

                if (_inactiveInstances.Count < MaxCapacity || MaxCapacity <= 0)
                {
                    _inactiveInstances.Enqueue(poolItem);

                    //if (InactiveParent != null)
                    gameObject.transform.SetParent(InactiveParent, false);

                    poolItem.OnReturnToPool();

                    gameObject.SetActive(false);
                }
                else
                    GameObject.Destroy(gameObject);
            }
            else
                Debug.LogWarning("ReturnToPool: Can not return a null object!");

            return false;
        }


        /// <summary>
        /// Return an item back to the queue. If we are above the maximum capacity then we just delete the item.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool ReturnToPool(GameObject gameObject)
        {
            if (gameObject != null)
            {
                PoolItem poolItem;
                if (_inUseInstances.TryGetValue(gameObject, out poolItem))
                {
                    return ReturnToPool(poolItem);
                }
                else
                    Debug.LogError("ReturnToPool: Object not managed by this object pool!");
            }
            else
                Debug.LogWarning("ReturnToPool: Can not return a null object!");

            return false;
        }

#endregion

#region Create

        /// <summary>
        /// Create a new pooled instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        PoolItem CreatePoolInstance(Transform parent, Vector3 position, Quaternion rotation)
        {
            var prefabClone = (GameObject)GameObject.Instantiate(Prefab, position, rotation);
            prefabClone.transform.SetParent(parent, false);
            prefabClone.name = Prefab.name;

            var poolInstance = CreatePoolItem();
            poolInstance.GameObject = prefabClone;
            poolInstance.OriginalPrefab = Prefab;
            poolInstance.Pool = this;
            poolInstance.OnSetup();
            return poolInstance;
        }


        /// <summary>
        /// Create a new PoolItem
        /// </summary>
        /// Override this in subclasses if you want to use subclasses of PoolItem. See also PoolGeneric for a generic implementation of this
        /// <returns></returns>
        protected virtual PoolItem CreatePoolItem()
        {
            return new PoolItem();
        }

#endregion Create
    }
}