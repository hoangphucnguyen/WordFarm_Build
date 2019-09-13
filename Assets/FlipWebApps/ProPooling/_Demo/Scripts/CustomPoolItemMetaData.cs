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

namespace ProPooling._Demo
{
    /// <summary>
    /// Demo component for showing custom PoolItem
    /// </summary>
    public class CustomPoolItemMetaData : MonoBehaviour
    {
        public GameObject Prefab;
        void Start()
        {
            // We need to setup such an item manually or through the editor (PoolManager only works with standard PoolItem types).
            PoolGeneric<CustomPoolItem> pool = new PoolGeneric<CustomPoolItem>(Prefab, 2);
            var poolItem = pool.GetPoolItemFromPool();
            // no need here to gget the component reference as that is handled by the CustomPoolItem.
            Debug.Log("Got pooled item - LifecycleComponent has initial value of " + poolItem.LifecycleComponent.Value);
            poolItem.LifecycleComponent.Value = 10;
            Debug.Log("Perform in game action - setting component value to 10 and then returning to pool.");
            pool.ReturnToPool(poolItem);

            poolItem = pool.GetPoolItemFromPool();
            // no need here to reget the component reference that we had in the LifeCycleCallback example.
            Debug.Log("Got pooled item second time - LifecycleComponent has initial value of " + poolItem.LifecycleComponent.Value);
            pool.ReturnToPool(poolItem);

            pool.ClearPool();
        }

        /// <summary>
        /// Custom Pool Item that outputs log information on the different states.
        /// </summary>
        public class CustomPoolItem : PoolItem
        {
            public LifecycleComponent LifecycleComponent;

            public override void OnSetup()
            {
                base.OnSetup();
                LifecycleComponent = GameObject.GetComponent<LifecycleComponent>();     // store a reference for later
                Debug.Log("CustomPoolItem: Setup");
            }

            public override void OnGetFromPool()
            {
                base.OnGetFromPool();
                Debug.Log("CustomPoolItem: GetFromPool");
            }

            public override void OnReturnToPool()
            {
                base.OnReturnToPool();
                Debug.Log("CustomPoolItem: ReturnToPool");
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                Debug.Log("CustomPoolItem: Destroy");
            }
        }
    }
}