//----------------------------------------------
// Flip Web Apps: Game Framework
// Copyright © 2016 Flip Web Apps / Mark Hewitt
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

using GameFramework.GameObjects;
using UnityEngine;

namespace GameFramework.Scrolling.Components
{
    /// <summary>
    /// Parallex item. This can be optionally added to scrollable prefabs to help customise or override the behavious.
    /// </summary>
    [AddComponentMenu("Game Framework/Scrolling/ParallaxItem")]
    [HelpURL("http://www.flipwebapps.com/game-framework/")]
    public class ParallaxItem : MonoBehaviour
    {
        [Tooltip("Whether the size should be determined from an attached renderer if present.")]
        public bool SizeFromRenderer = true;

        [Tooltip("Padding to the left of this item.")]
        public MinMaxf PaddingLeft;

        [Tooltip("Padding to the right of this item.")]
        public MinMaxf PaddingRight;

        /// <summary>
        /// Return a new random padding based upon padding right setttings.
        /// </summary>
        /// <returns></returns>
        public float GetNewPaddingLeft()
        {
            return PaddingLeft.RandomValue();
        }

        /// <summary>
        /// Return a new random padding based upon padding right setttings.
        /// </summary>
        /// <returns></returns>
        public float GetNewPaddingRight()
        {
            return PaddingRight.RandomValue();
        }
    }
}
