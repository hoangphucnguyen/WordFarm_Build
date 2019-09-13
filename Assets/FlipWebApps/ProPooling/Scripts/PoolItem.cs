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
using UnityEngine;

namespace ProPooling
{
    /// <summary>
    /// A meta data class associated with all pooled instance with shortcuts to key data and lifecycle functionality. 
    /// </summary>
    /// This class contains amongst other things a reference to the containing pool, the original prefab and lifecycle callbacks.
    /// 
    /// You can override this class and use it to store and parse information about the pooled items for your own purposed.
    /// Be sure to consider whether to call the base class methods if you are overriding anything. See PoolGeneric for information about 
    /// using subclassed versions of this class.
    [Serializable]
    public class PoolItem
    {
        /// <summary>
        /// A reference to the Pool that contains this item for quick and easy access.
        /// </summary>
        public Pool Pool { get; set; }

        /// <summary>
        /// The original prefab that was used for creating this item. Can be used to access initial values.
        /// </summary>
        public GameObject OriginalPrefab { get; set; }

        /// <summary>
        /// The instantiated gameobject for this pool item.
        /// </summary>
        public GameObject GameObject { get; set; }

        IPoolComponent[] _poolComponents;

        #region notifications

        /// <summary>
        /// Called when this item is setup
        /// </summary>
        public virtual void OnSetup()
        {
            _poolComponents = GameObject.GetComponents<IPoolComponent>();
        }

        /// <summary>
        /// Called when this item is pulled from the pool
        /// </summary>
        public virtual void OnGetFromPool()
        {
            for (var i = 0; i < _poolComponents.Length; i++)
                _poolComponents[i].OnGetFromPool(this);
        }

        /// <summary>
        /// Called when this item is returned to the pool
        /// </summary>
        public virtual void OnReturnToPool()
        {
            for (var i = 0; i < _poolComponents.Length; i++)
                _poolComponents[i].OnReturnToPool(this);
        }

        /// <summary>
        /// Called just before this item is destroyed.
        /// </summary>
        public virtual void OnDestroy() { }

        #endregion notifications

        /// <summary>
        /// Call to return this item back to its pool
        /// </summary>
        public virtual void ReturnSelf()
        {
            Pool.ReturnToPool(this);
        }
    }
}