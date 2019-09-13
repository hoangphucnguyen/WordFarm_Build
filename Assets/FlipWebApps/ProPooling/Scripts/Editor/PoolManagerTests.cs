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

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#else
using NUnit.Framework;
using UnityEngine;

namespace ProPooling.Editor
{
    public class PoolManagerTests
    {

        #region Initialisation and Cleanup


        [TestCase("Test 1", 4)]
        [TestCase("Test 2", 8)]
        public void InitialisePool(string name, int initialCount)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var parentGameObject = new GameObject("parent");
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.Pools.Add(new Pool(poolGameObject, preInitialiseCount: initialCount,
                inactiveParent: parentGameObject.transform));
            poolManager.InitialisePools();

            // Act
            var retreivedPool = poolManager.GetPool(poolGameObject);

            // Assert
            Assert.IsNotNull(retreivedPool, "The Pool was not retrieved correctly");
            Assert.AreEqual(retreivedPool.InactiveCount, initialCount, "The Pool was not retrieved correctly");

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void GetMissingPoolByNameFails(string name)
        {
            // Arrange
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            // Act
            var retreivedPool = poolManager.GetPool(name);

            // Assert
            Assert.IsNull(retreivedPool, "The Pool should have been missing");

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void GetPoolByName(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            var pool = new Pool(poolGameObject, 5);
            pool.Initialise();
            poolManager.AddPool(pool);

            // Act
            var retreivedPool = poolManager.GetPool(name);
            // Assert
            Assert.IsNotNull(retreivedPool, "The Pool was not retrieved correctly");

            pool.ClearPool();

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);
        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void GetMissingPoolByGameObject(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            // Act
            var retreivedPool = poolManager.GetPool(poolGameObject, false);

            // Assert
            Assert.IsNull(retreivedPool, "The Pool should have been missing");

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);
        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void GetMissingPoolByGameObjectAutoCreate(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            // Act
            var retreivedPool = poolManager.GetPool(poolGameObject, true);

            // Assert
            Assert.IsNotNull(retreivedPool, "The Pool should have been created");

            retreivedPool.ClearPool();

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);
        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void GetPoolByGameObject(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            var pool = new Pool(poolGameObject, 5);
            pool.Initialise();
            poolManager.AddPool(pool);

            // Act
            var retreivedPool = poolManager.GetPool(poolGameObject, false);

            // Assert
            Assert.IsNotNull(retreivedPool, "The Pool was not retrieved correctly");

            pool.ClearPool();

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);
        }


        #endregion Initialisation and Cleanup

        #region GetFromPool


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void GetPoolItemFromPoolByName(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            var pool = new Pool(poolGameObject, 5);
            pool.Initialise();
            poolManager.AddPool(pool);

            // Act
            var retreivedItem = poolManager.GetPoolItemFromPool(name);

            // Assert
            Assert.IsNotNull(retreivedItem, "The Pool Item was not retrieved correctly");
            Assert.AreEqual(4, pool.InactiveCount, "The item was not retrieved from a pool");

            pool.ClearPool();

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);
        }

        #endregion GetFromPool

        #region ReturnToPool

        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void ReturnToPool(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            var pool = new Pool(poolGameObject, 5);
            pool.Initialise();
            poolManager.AddPool(pool);
            var poolItem = pool.GetPoolItemFromPool();
            Assert.AreEqual(4, pool.InactiveCount, "The item was not retrieved from a pool");

            // Act
            poolManager.ReturnToPool(poolItem);

            // Assert
            Assert.AreEqual(5, pool.InactiveCount, "The item was not returned to the pool");

            pool.ClearPool();

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);

        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void ReturnToPoolGameObject(string name)
        {
            // Arrange
            var poolGameObject = new GameObject(name);
            var poolManagerGameObject = new GameObject("PoolManager");
            var poolManager = poolManagerGameObject.AddComponent<PoolManager>();
            poolManager.InitialisePools();

            var pool = new Pool(poolGameObject, 5);
            pool.Initialise();
            poolManager.AddPool(pool);
            var poolItem = pool.GetPoolItemFromPool();
            Assert.AreEqual(4, pool.InactiveCount, "The item was not retrieved from a pool");

            // Act
            poolManager.ReturnToPool(poolItem.GameObject);

            // Assert
            Assert.AreEqual(5, pool.InactiveCount, "The item was not returned to the pool");

            pool.ClearPool();

            UnityEngine.Object.DestroyImmediate(poolManagerGameObject);
            UnityEngine.Object.DestroyImmediate(poolGameObject);

        }

        #endregion ReturnToPool
    }
}
#endif