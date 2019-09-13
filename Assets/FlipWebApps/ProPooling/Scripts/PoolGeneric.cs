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

using UnityEngine;

namespace ProPooling
{
    /// <summary>
    /// A generic implementation of the Pooling class that can be used to easily handle custom PoolItem types.
    /// By specifying your own type you can add additional customisation during setup, deallocation and otherwise.
    /// </summary>
    public class PoolGeneric<T> : Pool where T : PoolItem, new()
    {
        /// <summary>
        /// Initialise this pool instance
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="preInitialiseCount"></param>
        /// <param name="initialCapacity"></param>
        /// <param name="maxCapacity"></param>
        /// <param name="inactiveParent"></param>
        public PoolGeneric(GameObject prefab, int preInitialiseCount = 0, int initialCapacity = 5, int maxCapacity = 0, Transform inactiveParent = null) :
            base(prefab, preInitialiseCount, initialCapacity, maxCapacity, inactiveParent)
        {
        }

        #region GetFromPool

        /// <summary>
        /// Get an item from the pool, optionally creating a new one if there aren't already enough avilable.
        /// </summary>
        /// Position and rotation will be set to the default values that the initial prefab had.
        /// <returns></returns>
        public new T GetPoolItemFromPool(Transform parent = null)
        {
            return base.GetPoolItemFromPool(parent) as T;
        }


        /// <summary>
        /// Get an item from the pool, optionally creating a new one if there aren't already enough avilable.
        /// </summary>
        /// <returns></returns>
        public new T GetPoolItemFromPool(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return base.GetPoolItemFromPool(position, rotation, parent) as T;
        }

        #endregion GetFromPool

        #region Create

        /// <summary>
        /// Create a new PoolItem of the generic typs
        /// </summary>
        /// <returns></returns>
        protected override PoolItem CreatePoolItem()
        {
            return new T();
        }

        #endregion Create
    }
}