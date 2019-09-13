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

using GameFramework.GameStructure.Levels;
using GameFramework.GameStructure.Levels.ObjectModel;
using GameFramework.Localisation;
using GameFramework.UI.Dialogs.Components;

namespace GameFrameworkTutorials.FullGames.BasicDemo.Scripts
{
    public class MyGameOver : GameOver
    {

        public override void Show(bool isWon)
        {
            base.Show(isWon);

            DialogManager.Instance.ShowOnce("Watch.Videos", title: LocaliseText.Get("Watch.Videos.Title"), text: LocaliseText.Get("Watch.Videos.Text"));
        }

        public override int GetNewStarsWon()
        {
            // can only win new stars if the game was won
            if (!GameLoop.Instance.IsWon()) return 0;

            int newStarsWon = 0;
            Level CurrentLevel = LevelManager.Instance.Level;
            if (LevelManager.Instance.SecondsRunning <= GameLoop.Instance.LengthOfGame *.25f && (CurrentLevel.StarsWon & 4) != 4)
            {
                newStarsWon |= 4;
            }
            if (LevelManager.Instance.SecondsRunning <= GameLoop.Instance.LengthOfGame * .5f && (CurrentLevel.StarsWon & 2) != 2)
            {
                newStarsWon |= 2;
            }
            if (LevelManager.Instance.SecondsRunning <= GameLoop.Instance.LengthOfGame * .75f && (CurrentLevel.StarsWon & 1) != 1)
            {
                newStarsWon |= 1;
            }
            return newStarsWon;
        }
    }
}