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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProPooling
{
    /// <summary>
    /// Manager for handling pooled items across the scene. You can also manage your own pools directly if 
    /// you so wish by adding your own List<Pool> property to your component.
    /// </summary>
    [AddComponentMenu("ProPooling/PoolManager")]
    [HelpURL("http://www.flipwebapps.com/pro-pooling/")]
    public class PoolManager : MonoBehaviour
    {
        #region Singleton
        // Static singleton property
        public static PoolManager Instance { get; private set; }
        public static bool IsActive { get { return Instance != null; } }

        void Awake()
        {
            // First we check if there are any other instances conflicting then destroy this and return
            if (Instance != null)
            {
                if (Instance != this)
                    Destroy(gameObject);
                return;             // return is my addition so that the inspector in unity still updates
            }

            // Here we save our singleton instance
            Instance = this as PoolManager;

            // Persis if specified.
            if (PersistBetweenScenes)
                DontDestroyOnLoad(gameObject);

            // setup specifics for instantiated object only.
            InitialisePools();
        }

        void OnDestroy()
        {
            // cleanup for instantiated object only.
            if (Instance == this) { }
        }
        #endregion Singleton


        /// <summary>
        /// Whether the Poolmanager should persisit between scenes.
        /// </summary>
        [Tooltip("Whether the Poolmanager should persisit between scenes.")]
        public bool PersistBetweenScenes;

        /// <summary>
        /// A list of pools that should be preallocated.
        /// </summary>
        [Tooltip("Pools that should be preallocated.")]
        public List<Pool> Pools = new List<Pool>();

        // mapping between prefab id and pool
        readonly Dictionary<int, Pool> _poolsIdMapping = new Dictionary<int, Pool>();

        // mapping between prefab name and pool
        readonly Dictionary<string, Pool> _poolsNameMapping = new Dictionary<string, Pool>();


        #region initialisation
        /// <summary>
        /// Initialise all pools
        /// </summary>
        public void InitialisePools() {
            _poolsIdMapping.Clear();
            _poolsNameMapping.Clear();

            foreach (var pool in Pools)
            {
                if (pool.Prefab == null)
                {
                    Debug.LogWarning("Ensure all pools in PoolManager have a valid prefab or gameobject");
                    continue;
                }

                if (pool.InactiveParent == null)
                    pool.InactiveParent = transform;
                pool.Initialise();
                AddPool(pool);
            }
        }
        #endregion initialisation

        #region pool management
        /// <summary>
        /// Create a pool for the given prefab with the specified parameters.
        /// 
        /// If an existing pool is found for this prefab then that is returned instead.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="preInitialiseCount"></param>
        /// <param name="initialCapacity"></param>
        /// <param name="maxCapacity"></param>
        /// <param name="inactiveParent"></param>
        /// <returns></returns>
        public Pool CreatePool(GameObject prefab, int preInitialiseCount = 0, int initialCapacity = 5, int maxCapacity = 0, Transform inactiveParent = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning("Ensure all pools in PoolManager have a valid prefab or gameobject");
                return null;
            }

            // Return any existing pool
            Pool pool;
            if (_poolsIdMapping.TryGetValue(prefab.GetInstanceID(), out pool)) return pool;

            // No existing so create a new pool
            pool = new Pool(prefab, preInitialiseCount, initialCapacity, maxCapacity, inactiveParent);
            AddPool(pool);
            return pool;
        }


        /// <summary>
        /// Add a pool to management. 
        /// </summary>
        /// Note: This only adds references and doesn't actually place the pool in the Pools list!
        /// <param name="pool"></param>
        public void AddPool(Pool pool)
        {
            if (_poolsNameMapping.ContainsKey(pool.Name))
                Debug.LogWarning("Adding a pool with the duplicate name (" + pool.Name + "). This might cause problems under certain conditions so should be avoided by renaming the Prefab of Gameobject you are using.");

            _poolsIdMapping.Add(pool.ID, pool);
            _poolsNameMapping.Add(pool.Name, pool);
        }


        /// <summary>
        /// Clear the pool and remove it from management. Note that if there are still spawned items then these will not 
        /// be destroyed by this call.
        /// </summary>
        /// <param name="pool"></param>
        public void ClearAndRemovePool(Pool pool)
        {
            pool.ClearPool();

            if (_poolsIdMapping.ContainsKey(pool.ID))
                _poolsIdMapping.Remove(pool.ID);
            if (_poolsNameMapping.ContainsKey(pool.Name))
                _poolsNameMapping.Remove(pool.Name);
        }
        #endregion pool management

        #region pool retrieval
        /// <summary>
        /// Get a pool for the given prefab, optionally creating it if needed
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        public Pool GetPool(GameObject prefab, bool autoCreate = true)
        {
            Assert.IsNotNull(prefab, "The prefab you passed must not be null!");

            Pool pool;
            if (_poolsIdMapping.TryGetValue(prefab.GetInstanceID(), out pool))
                return pool;

            return autoCreate ? CreatePool(prefab) : null;
        }


        /// <summary>
        /// Get a pool for the prefab with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Pool GetPool(string name)
        {
            Assert.IsNotNull(name, "The prefabName you passed must not be null!");

            Pool pool;
            return _poolsNameMapping.TryGetValue(name, out pool) ? pool : null;
        }

        #endregion pool retrieval

        #region get from managed pools

        /// <summary>
        /// Get a gameobject from the named pool.
        /// </summary>
        /// <returns></returns>
        public GameObject GetFromPool(string name, Transform parent = null)
        {
            var poolItem = GetPoolItemFromPool(name, parent);
            return poolItem == null ? null : poolItem.GameObject;
        }


        /// <summary>
        /// Get a gameobject from the named pool.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject GetFromPool(string name, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var poolItem = GetPoolItemFromPool(name, position, rotation, parent);
            return poolItem == null ? null : poolItem.GameObject;
        }


        /// <summary>
        /// Get a PoolItem reference from the named pool
        /// </summary>
        /// <returns></returns>
        public PoolItem GetPoolItemFromPool(string name, Transform parent = null)
        {
            var pool = GetPool(name);
            if (pool != null)
            {
                return GetPoolItemFromPool(pool.Prefab, parent);
            }
            else
            {
                Debug.LogError("GetPoolItemFromPool: Pool named '" + name + "' not managed by PoolManager!");
                return null;
            }
        }


        /// <summary>
        /// Get a PoolItem reference from the named pool
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public PoolItem GetPoolItemFromPool(string name, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var pool = GetPool(name);
            if (pool != null)
            {
                return GetPoolItemFromPool(pool.Prefab, position, rotation, parent);
            }
            else
            {
                Debug.LogError("GetPoolItemFromPool: Pool named '" + name + "' not managed by PoolManager!");
                return null;
            }
        }


        /// <summary>
        /// Get a gameobject from the pool of the spefified type.
        /// </summary>
        /// <returns></returns>
        public GameObject GetFromPool(GameObject gameObject, Transform parent = null)
        {
            return GetPoolItemFromPool(gameObject, parent).GameObject;
        }


        /// <summary>
        /// Get a gameobject from the pool of the spefified type.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject GetFromPool(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return GetPoolItemFromPool(gameObject, position, rotation, parent).GameObject;
        }


        /// <summary>
        /// Get a PoolItem reference from the pool of the spefified type.
        /// </summary>
        /// <returns></returns>
        public PoolItem GetPoolItemFromPool(GameObject gameObject, Transform parent = null)
        {
            var pool = GetPool(gameObject);
            if (pool != null)
            {
                return GetPoolItemFromPool(gameObject, pool.Prefab.transform.position, pool.Prefab.transform.rotation,
                    parent);
            }
            else
            {
                return GetPoolItemFromPool(gameObject, default( Vector3), default (Quaternion), parent);
            }
        }


        /// <summary>
        /// Get a PoolItem reference from the pool of the spefified type.
        /// </summary>
        /// <returns></returns>
        public PoolItem GetPoolItemFromPool(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Assert.IsNotNull(gameObject, "The gameObject you passed must not be null when getting an item from a pool!");

            var pool = GetPool(gameObject);
            if (pool != null)
            {
                return pool.GetPoolItemFromPool(position, rotation, parent);
            }
            else
            {
                Debug.LogWarning("GetPoolItemFromPool: Object named '" + gameObject.name + "' not in a pool managed by PoolManager! Creating a new copy gameObject.");
                var newGameobject = GameObject.Instantiate(gameObject, position, rotation) as GameObject;
                newGameobject.transform.parent = null;
                var poolInstance = new PoolItem
                {
                    GameObject = newGameobject,
                    OriginalPrefab = gameObject
                };
                poolInstance.OnSetup();
                return poolInstance;
            }
        }

        #endregion get from managed pools

        #region return to managed pools

        /// <summary>
        /// Return an item back to a pool managed by PoolManager.
        /// </summary>
        /// <param name="poolItem"></param>
        /// <returns></returns>
        public bool ReturnToPool(PoolItem poolItem)
        {
            if (poolItem != null)
            {
                poolItem.ReturnSelf();
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Return an item back to a pool managed by PoolManager.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool ReturnToPool(GameObject gameObject)
        {
            if (gameObject != null)
            {
                var pool = GetPool(gameObject.name);
                if (pool != null)
                {
                    return pool.ReturnToPool(gameObject);
                }
                else
                {
                    Debug.LogWarning("ReturnToPool: Object named '" + gameObject.name + "' not in a pool managed by PoolManager! Destroying gameObject.");
                    Destroy(gameObject);
                    return false;
                }
            }
            return false;
        }

        #endregion return to managed pools
    }
}
