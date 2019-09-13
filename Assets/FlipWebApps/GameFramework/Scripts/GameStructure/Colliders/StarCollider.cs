﻿//----------------------------------------------
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

using GameFramework.GameStructure.Levels;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameFramework.GameStructure.Colliders
{
    /// <summary>
    /// Collider for winning a star when a tagged gameobject touches the attached collider or trigger.
    /// </summary>
    [AddComponentMenu("Game Framework/GameStructure/Colliders/Star Collider")]
    [HelpURL("http://www.flipwebapps.com/unity-assets/game-framework/game-structure/colliders/")]
    public class StarCollider : GenericCollider
    {
        /// <summary>
        /// A delay before the game over dialog is shown.
        /// </summary>
        public int StarNumber
        {
            get
            {
                return _starNumber;
            }
            set
            {
                _starNumber = value;
            }
        }
        [Header("Star Specific Settings")]
        [Tooltip("The number of the star that will be won when a collision occurs.")]
        [SerializeField]
        int _starNumber = 1;


        /// <summary>
        /// Called when we have detected and processed a valid trigger / collider enter based upon other settings
        /// </summary>
        /// Override this in you custom base classes that you want to hook into the trigger system.
        /// <param name="collidingGameObject">The GameObject that we collided with</param>
        public override void EnterOccurred(GameObject collidingGameObject)
        {
            AdjustStars();
        }
        
        
        /// <summary>
        /// Called when we have detected and processed a valid trigger / collider stay based upon other settings
        /// </summary>
        /// Override this in you custom base classes that you want to hook into the trigger system.
        /// <param name="collidingGameObject">The GameObject that we collided with</param>
        public override void StayOccurred(GameObject collidingGameObject)
        {
            AdjustStars();
        }


        /// <summary>
        /// Adjust the health
        /// </summary>
        void AdjustStars()
        {
            Assert.IsTrue(LevelManager.IsActive, "To use WinLevelCollider, ensure that you have a LevelManager added to your scene.");
            LevelManager.Instance.Level.StarWon(StarNumber, true);
        }
    }
}