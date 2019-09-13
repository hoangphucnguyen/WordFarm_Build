using GameFramework.GameStructure;
using GameFramework.GameStructure.Levels;
using UnityEngine;

namespace GameFramework._Demo.GameStructure.Scripts
{
    public class CreateLevelObjects : MonoBehaviour
    {
        public GameObject CoinsPrefab;

        public void Start()
        {
            for (var i = 0; i < LevelManager.Instance.Level.Star3Target; i++)
            {
                var gObj = (GameObject)Instantiate(CoinsPrefab, GetValidPosition(), CoinsPrefab.transform.rotation);
                gObj.transform.SetParent(transform, true);
            }

            for (var i = 0; i < LevelManager.Instance.Level.Variables.GetInt("Rock Count").DefaultValue; i++)
            {
                var gObj = (GameObject)Instantiate(LevelManager.Instance.Level.GetPrefab("rock"), GetValidPosition(), CoinsPrefab.transform.rotation);
                gObj.transform.SetParent(transform, true);
            }
        }

        float _count, _delay;
        public void Update()
        {
            if (LevelManager.Instance.IsLevelRunning)
            {
                _count += Time.deltaTime;
                if (_count > _delay)
                {
                    var gObj = (GameObject)Instantiate(LevelManager.Instance.Level.GetPrefab("enemy"), 
                        (Vector2)GameManager.Instance.WorldBottomLeftPosition + new Vector2(0, Random.Range(2, 4)), 
                        Quaternion.identity);
                    var rigidBody2D = gObj.GetComponent<Rigidbody2D>();
                    rigidBody2D.AddForce(new Vector2(Random.Range(50, 200), Random.Range(50, 300)));
                    _count = 0;
                    _delay = Random.Range(2, 4);
                }
            }
        }

        // Get a position that doesn't overlap the Players start position.
        static Vector2 GetValidPosition()
        {
            float xPos, yPos;
            do
            {
                xPos = Random.Range(GameManager.Instance.WorldBottomLeftPositionXYPlane.x + 2, GameManager.Instance.WorldTopRightPositionXYPlane.x - 2);
                yPos = Random.Range(GameManager.Instance.WorldBottomLeftPositionXYPlane.y + 2, GameManager.Instance.WorldTopRightPositionXYPlane.y - 2);

            } while (xPos > -1 && xPos < -1 && yPos > -2.3 && yPos < -0.3);
            return new Vector2(xPos, yPos);
        }
    }
}
