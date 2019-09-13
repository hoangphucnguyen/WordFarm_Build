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

using GameFramework.GameStructure;
using UnityEngine;

namespace GameFrameworkTutorials.FullGames._2DInfinateRunner.Scripts
{
    public class Helicopter : MonoBehaviour
    {
        Vector3 _lastPosition;
        Vector3 _targetPosition;
        Vector3 _lastRotation;
        Vector3 _targetRotation;
        float _counter;
        float _speed;

        void Start()
        {
            SetNewTarget();
        }

        void Update()
        {
            _counter += Time.deltaTime/_speed;
            transform.position = new Vector3(
                EaseInOutQuad(_lastPosition.x, _targetPosition.x, _counter),
                EaseInOutQuad(_lastPosition.y, _targetPosition.y, _counter),
                _targetPosition.z);
            transform.eulerAngles = new Vector3(_targetRotation.x, EaseInOutQuad(_lastRotation.y, _targetRotation.y, Mathf.Clamp(_counter * 2f - 1f, 0, 1)), _targetRotation.z);
            if (_counter >= Random.Range(1.0f, 1.1f))
                SetNewTarget();
        }

        void SetNewTarget()
        {
            _lastPosition = transform.position;
            _targetPosition = new Vector3(Random.Range(4, GameManager.Instance.WorldTopRightPosition.x - 1),
                                        Random.Range(0, GameManager.Instance.WorldBottomLeftPosition.y + 2.5f), _lastPosition.z);
            if (transform.position.x > 0) _targetPosition.x *= -1;
            _speed = Vector3.Distance(_lastPosition, _targetPosition) / Random.Range(4f, 3f);
            _counter = 0;
            _lastRotation = transform.eulerAngles;
            _targetRotation = new Vector3(_lastRotation.x, _lastRotation.y + (Random.Range(0, 2) == 0 ? 180 : -180), _lastRotation.z); 
        }


        static float EaseInOutQuad(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if (value < 1) return end * 0.5f * value * value + start;
            value--;
            return -end * 0.5f * (value * (value - 2) - 1) + start;
        }
    }
}