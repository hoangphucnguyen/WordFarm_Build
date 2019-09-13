﻿//----------------------------------------------
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

using System.Collections;
using UnityEngine;

namespace ProPooling.Components
{
    /// <summary>
    /// Component to automatically return the gameobjects back to the pool after a specified delay.
    /// </summary>
    [AddComponentMenu("ProPooling/ReturnToPoolAfterDelay")]
    [HelpURL("http://www.flipwebapps.com/pro-pooling/")]
    public class ReturnToPoolAfterDelay : MonoBehaviour, IPoolComponent
    {
        /// <summary>
        /// A delay before returning the gameobject back to the pool.
        /// </summary>
        [Tooltip("A delay before returning the gameobject back to the pool")]
        public float Delay;

        #region IPoolComponent

        /// <summary>
        /// When pulled from the pool, start a coroutine to return the item after the specified delay.
        /// </summary>
        /// <param name="poolItem"></param>
        public void OnGetFromPool(PoolItem poolItem)
        {
            StartCoroutine(ReturnToPoolDelayed(poolItem));
        }


        public void OnReturnToPool(PoolItem poolItem)
        {
        }

        #endregion IPoolComponent


        IEnumerator ReturnToPoolDelayed(PoolItem poolItem)
        {
            yield return new WaitForSeconds(Delay);
            poolItem.ReturnSelf();
        }
    }
}