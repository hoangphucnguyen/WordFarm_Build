using GameFramework.GameStructure.Levels;
using UnityEngine;

namespace GameFrameworkTutorials.GettingStarted.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public float Speed = 0.5f;
        Rigidbody2D _rigidBody2D;

        void Awake()
        {
            _rigidBody2D = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (LevelManager.Instance.IsLevelRunning && Input.GetMouseButton(0))
            {
                var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (mousePosition - transform.position);
                _rigidBody2D.AddForce(direction * Speed);
            }
        }
    }
}