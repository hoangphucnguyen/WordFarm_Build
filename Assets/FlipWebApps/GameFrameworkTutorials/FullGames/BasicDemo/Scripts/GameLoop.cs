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

using GameFramework.GameObjects.Components;
using GameFramework.GameStructure.Levels;
using GameFramework.UI.Dialogs.Components;
using UnityEngine;

namespace GameFrameworkTutorials.FullGames.BasicDemo.Scripts
{
    public class GameLoop : Singleton<GameLoop>
    {
        public int LengthOfGame = 20;
        public UnityEngine.UI.Image[] CoinImages;
        public GameObject[] GameCoins;

        void Start ()
        {
            // show valid coins
            for (int i = 0; i < 10; i++)
                if (i >= LevelManager.Instance.Level.Number)
                {
                    CoinImages[i].gameObject.SetActive(false);
                    GameCoins[i].gameObject.SetActive(false);
                }
        }

        // note: not optimised
        void Update ()
        {
            // update coin images with what is collected
            for (int i = 0; i < LevelManager.Instance.Level.Coins; i++)
                CoinImages[i].color = Color.white;

            // run game over logic
            bool isWon = IsWon();
            bool isLost = LevelManager.Instance.SecondsRunning > LengthOfGame;
            if (LevelManager.Instance.IsLevelRunning && (isWon || isLost))
            {
                LevelManager.Instance.GameOver(isWon);
            }
        }

        public bool IsWon()
        {
            return LevelManager.Instance.Level.Coins >= LevelManager.Instance.Level.Number;
        }
    }
}
