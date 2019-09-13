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

#if PLAYMAKER
using GameFramework.GameStructure.Levels;
using HutongGames.PlayMaker;
using UnityEngine.Assertions;

namespace GameFrameworkPlayMakerExtensions.Actions.GameStructure.Levels
{
    /// <summary>
    /// Checks whether the specified star has been won.
    /// </summary>
    [ActionCategory("Game Framework")]
    [Tooltip("Checks whether the specified star has been won.")]
    public class LevelIsStarWon : FsmStateAction
    {
        /// <summary>
        /// The star to check if it is won
        /// </summary>
        [Tooltip("The star to check if it is won.")]
        [RequiredField]
        public FsmInt StarNumber;

        /// <summary>
        /// Event to send if the star is won.
        /// </summary>
        [Tooltip("Event to send if the star is won.")]
        public FsmEvent TrueEvent;

        /// <summary>
        /// Event to send if the star isn't won
        /// </summary>
        [Tooltip("Event to send if the star isn't won.")]
        public FsmEvent FalseEvent;

        /// <summary>
        /// Store the result in a bool variable.
        /// </summary>
        [Tooltip("Store the result in a bool variable.")]
        [UIHint(UIHint.Variable)]
        public FsmBool StoreResult;

        /// <summary>
        /// Repeate every frame.
        /// </summary>
        [Tooltip("Repeate every frame.")]
        public bool EveryFrame;

        public override void Reset()
        {
            base.Reset();
            StarNumber = 1;
            TrueEvent = null;
            FalseEvent = null;
            StoreResult = null;
            EveryFrame = false;
        }

        public override void OnEnter()
        {
            PerformAction();

            if (!EveryFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            PerformAction();
        }

        /// <summary>
        /// The actual method that does the work
        /// </summary>
        void PerformAction()
        {
            Assert.IsTrue(LevelManager.IsActive, "Ensure that you have a LevelManager added to your scene before using the LevelGetCoins action!");

            var isStarWon = LevelManager.Instance.Level.IsStarWon(StarNumber.Value);

            StoreResult.Value = isStarWon;

            Fsm.Event(isStarWon ? TrueEvent : FalseEvent);
        }
    }
}
#endif