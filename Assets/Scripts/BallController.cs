using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject BallTemplate;
    public Camera PlayerCam;
    public float BallSpeed = 20f;

    private Dictionary<string, Ball> Balls = new Dictionary<string, Ball>();
    private int ballCount = 0;
    private float mouseCounter = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouseCounter += Time.deltaTime;
        }
        if (Input.GetMouseButtonUp(0))
        {
            CreateNewBall();
            mouseCounter = 0;
        }

        foreach (var ball in Balls)
        { 
            ball.Value.BallObject.transform.Translate(Vector3.forward * ball.Value.Speed * Time.deltaTime);
            //TODO Gravity
            //TODO Drag
            //TODO Ball spin
        }
    }

    private void CreateNewBall()
    {
        //TODO add spin based on button press
        GameObject ball = Instantiate(BallTemplate, transform, true);
        ball.transform.forward = PlayerCam.transform.forward;
        ball.transform.forward.Normalize();
        ball.name = $"Ball{ballCount}";
        BallCollision ballCollider = ball.GetComponent<BallCollision>();
        ballCollider.BallCollisionEvent.AddListener(OnBallCollision);
        SphereCollider spherec = ball.GetComponent<SphereCollider>();
        Ball createdBall = new Ball(ball, mouseCounter * BallSpeed);
        Balls.Add(ball.name, createdBall);
        ballCount += 1;
    }

    public void OnBallCollision(BallCollisionContainer collision)
    {
        //TODO collision calculation
    }
}
