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

using GameFramework.GameStructure;
using GameFramework.GameStructure.Levels;
using UnityEngine;

namespace GameFramework.Display.Placement.Components
{
    /// <summary>
    /// Rotate this gameobject at a given rate.
    /// </summary>
    [AddComponentMenu("Game Framework/Display/Placement/FixedRotation")]
    [HelpURL("http://www.flipwebapps.com/unity-assets/game-framework/display/")]
    public class FixedRotation : MonoBehaviour
    {
        /// <summary>
        /// The coordinate space in which to operate.
        /// </summary>
        [Tooltip("The coordinate space in which to operate.")]
        public Space Space;

        /// <summary>
        /// X Azis rotation speed
        /// </summary>
        [Tooltip("X Azis rotation speed")]
        public float XAngle;

        /// <summary>
        /// Y Azis rotation speed
        /// </summary>
        [Tooltip("Y Azis rotation speed")]
        public float YAngle;

        /// <summary>
        /// Z Azis rotation speed
        /// </summary>
        [Tooltip("Z Azis rotation speed")]
        public float ZAngle;

        /// <summary>
        /// Specify whether to only rotate when a level is actually running, otherwise this gameobject will always rotate
        /// </summary>
        [Tooltip("Specify whether to only rotate when a level is actually running, otherwise this gameobject will always rotate")]
        public bool OnlyWhenLevelRunning = false;

        // Update is called once per frame
        void Update()
        {
            if (OnlyWhenLevelRunning && !LevelManager.Instance.IsLevelRunning)
                return;

            transform.Rotate(XAngle * Time.deltaTime, YAngle * Time.deltaTime, ZAngle * Time.deltaTime, Space);
        }
    }
}