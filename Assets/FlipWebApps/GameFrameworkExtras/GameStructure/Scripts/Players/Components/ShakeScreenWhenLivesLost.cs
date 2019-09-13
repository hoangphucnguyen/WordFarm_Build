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

using System;
using FlipWebApps.BeautifulTransitions.Scripts.Shake.Components;
using GameFramework.GameStructure.Players.Messages;
using GameFramework.Messaging.Components.AbstractClasses;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameFramework.GameStructure.Players.Components
{
    /// <summary>
    /// shake the screen when the number of lives is decreased.
    /// </summary>
    [AddComponentMenu("Game Framework/GameStructure/Players/ShakeScreenWhenLivesLost")]
    [HelpURL("http://www.flipwebapps.com/game-framework/")]
    public class ShakeScreenWhenLivesLost : RunOnMessage<LivesChangedMessage>
    {
        /// <summary>
        /// The shake camera component that will be used for shaking the screen.
        /// </summary>
        [Tooltip("The shake camera component that will be used for shaking the screen.")]
        public ShakeCamera ShakeCamera;


        /// <summary>
        /// Custom initialisation.
        /// </summary>
        public override void Start()
        {
            Assert.IsNotNull(ShakeCamera, "You need to add a reference to a ShakeCamera component.");
            base.Start();
        }


        /// <summary>
        /// This method is run when the number of lives decreases
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool RunMethod(LivesChangedMessage message)
        {
            if (message.NewLives < message.OldLives)
                ShakeCamera.Shake();
            return true;
        }
    }
}