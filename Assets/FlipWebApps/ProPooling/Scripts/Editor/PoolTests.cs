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
    public class PoolTests {

        #region Initialisation and Cleanup

        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void PoolId(string name)
        {
            // Arrange
            var testGameObject = new GameObject(name);

            // Act
            var pool = new Pool(testGameObject);

            // Assert
            Assert.AreEqual(pool.ID, testGameObject.GetInstanceID(), "The Pool Id is not set correctly");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
        }


        [TestCase("Test 1")]
        [TestCase("Test 2")]
        public void PoolName(string name)
        {
            // Arrange
            var testGameObject = new GameObject(name);

            // Act
            var pool = new Pool(testGameObject);

            // Assert
            Assert.AreEqual(pool.Name, testGameObject.name, "The Pool Name is not set correctly");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
        }


        [Test]
        public void ConstructorCreatesEmptyPool()
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");

            // Act
            var pool = new Pool(testGameObject);

            // Assert
            Assert.AreEqual(0, pool.InactiveCount, "Number of initial prefabs does not meet the counter");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void AutoInitialiseCreatesItems(int count)
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");

            // Act
            var pool = new Pool(testGameObject, preInitialiseCount: count, inactiveParent: parentGameObject.transform);

            // Assert
            Assert.AreEqual(count, pool.InactiveCount, "Number of initial prefabs does not meet the counter");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void InactiveItemsCreatedOnParent(int count)
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");

            // Act
            var pool = new Pool(testGameObject, preInitialiseCount: count, inactiveParent: parentGameObject.transform);

            // Assert
            Assert.AreEqual(count, parentGameObject.transform.childCount, "Number of initial prefabs does not meet the counter");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }


        [Test]
        public void ClearPoolDeletedItems()
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");
            var pool = new Pool(testGameObject, preInitialiseCount: 5);

            // Act
            pool.ClearPool();

            // Assert
            Assert.AreEqual(0, pool.InactiveCount, "Pool items were not cleared.");

            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }

        #endregion Initialisation and Cleanup

        #region GetFromPool

        public void GetFromPoolReturnsPreAllocatedInstance()
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");
            var pool = new Pool(testGameObject, preInitialiseCount: 1, inactiveParent: parentGameObject.transform);

            // Act
            var gameObject = pool.GetFromPool();

            // Assert
            Assert.IsTrue(gameObject == testGameObject.transform.GetChild(0).gameObject, "Number of initial prefabs does not meet the counter");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        public void GetFromPoolReturnesPrefab(int count)
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");
            var pool = new Pool(testGameObject, preInitialiseCount: 5);

            // Act
            for (var i = 0; i < count; i++)
                pool.GetFromPool();

            // Assert
            Assert.AreEqual(5 - count, pool.InactiveCount, "Number of remaining pool items is not correct.");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }

        [Test]
        public void GetFromEmptyPoolReturnesPrefab()
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");
            var pool = new Pool(testGameObject);

            // Act
            var gameobject = pool.GetFromPool();

            // Assert
            Assert.IsNotNull(gameobject, "Did not return a gameobject.");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }

        #endregion GetFromPool

        #region ReturnToPool

        [Test]
        public void ReturnToPoolCounterIsCorrect()
        {
            // Arrange
            var testGameObject = new GameObject("test");
            var parentGameObject = new GameObject("parent");
            var pool = new Pool(testGameObject, preInitialiseCount: 5);
            var gameobject = pool.GetFromPool();

            // Act
            pool.ReturnToPool(gameobject);

            // Assert
            Assert.AreEqual(5, pool.InactiveCount, "Number of pool items is not correct.");

            pool.ClearPool();
            UnityEngine.Object.DestroyImmediate(testGameObject);
            UnityEngine.Object.DestroyImmediate(parentGameObject);
        }

        #endregion ReturnToPool
    }
}
#endif