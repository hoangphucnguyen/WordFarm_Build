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
using UnityEngine.UI;

namespace ProPooling._Demo
{
    /// <summary>
    ///  Demo component for showing time comparison of Unity allocation v's Pro Pooling allocation.
    /// </summary>
    public class PoolManagerCreatedPool : MonoBehaviour
    {
        public GameObject Prefab;
        public Text UnityTime;
        public Text PoolTime;

        void Start()
        {
            var startTime = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                var GameObject = Instantiate(Prefab);
                GameObject.DestroyImmediate(GameObject);
            }
            UnityTime.text = "Unity Time : " + (DateTime.Now - startTime).TotalMilliseconds.ToString();

            startTime = DateTime.Now;
            var pool = PoolManager.Instance.GetPool(Prefab, false);
            for (int i = 0; i < 10000; i++)
            {
                pool.ReturnToPool(pool.GetPoolItemFromPool());
            }
            PoolTime.text = "Pro Pooling Time : " + (DateTime.Now - startTime).TotalMilliseconds.ToString();
        }
    }
}