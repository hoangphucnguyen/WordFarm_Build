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
using Random = UnityEngine.Random;

namespace ProPooling._Demo
{

    public class GraphicalDemoSpawnerNotPooled : MonoBehaviour
    {
        public GameObject Prefab;
        public float SpawnDelay;
        public float Velocity;
        public Text Display;

        float _counter;
        double _totalTime;

        /// <summary>
        /// Create items
        /// </summary>
        void Update()
        {
            _counter += Time.deltaTime;

            if (_counter > SpawnDelay)
            {
                var spawnTime = DateTime.Now;
                var gameobject = (GameObject)Instantiate(Prefab, new Vector3(0, 0, Random.Range(-2, 2)), Quaternion.identity);
                gameobject.transform.SetParent(transform, false);
                var rigidBody = gameobject.GetComponent<Rigidbody>();
                rigidBody.velocity = transform.up * Velocity;
                _counter = 0;
                _totalTime += (DateTime.Now - spawnTime).TotalMilliseconds;
            }

            Display.text = "Not Pooled Time: " + (int)_totalTime + "ms";
        }



        /// <summary>
        /// Just placing this here for now :)
        /// </summary>
        public void ShowRatePage()
        {
            Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/59286?aid=1011lGnE");
        }
    }
}