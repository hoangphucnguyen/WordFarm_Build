using FlipWebApps.BeautifulTransitions.Scripts.Shake.Components;
using GameFramework.Display.Placement.Components;
using GameFramework.GameObjects;
using GameFramework.GameStructure.Levels;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float tapSpeed = 100f;
    public GameObject Explosion;
    public FixedRotation FixedRotation;
    public AudioClip GameOverSound;

    GameObject _helicopterModel;

    bool didTap;
    public bool IsDead { get; set; }

    void Awake()
    {
        _helicopterModel = GameObjectHelper.GetChildNamedGameObject(gameObject, "Model", true);
        IsDead = false;
    }



    void Update()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            if (LevelManager.Instance.IsLevelRunning)
            {
                didTap = true;
            }
        }
    }

	// Update is called once per fixed frame
	void FixedUpdate () {
        if (didTap)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * tapSpeed);
            didTap = false;
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // only do once - as we are scrolling the objects this can get called multiple times.
        if (LevelManager.Instance.IsLevelRunning)
        {
            // take a screenshot for use later.
            //Application.CaptureScreenshot();
            Explosion.transform.position = transform.position;
            Explosion.SetActive(true);

            _helicopterModel.SetActive(false);
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * tapSpeed * 15);
            FixedRotation.ZAngle = 180;

            ShakeCamera.Instance.Shake();

            LevelManager.Instance.GameOver(false, 0.5f);
        }
    }
}
