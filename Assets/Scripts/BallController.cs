using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject BallTemplate;
    public Camera PlayerCam;
    public float BallLaunchSpeed = 40f;
    public float Gravity = 9.8f;
    public float BallDiameter = 0.06f;
    public float DragCoefficient = 0.5f;
    public float AirDenisty = 1.2f;
    public float BallMass = 0.06f;

    private Dictionary<string, Ball> _ballCollection = new Dictionary<string, Ball>();
    private int _ballCount = 0;
    private float _mouseCounter = 0f;
    private Vector3 _gravityVector;
    private Vector3 _scaleVector;
    private float _ballCrossSectionalArea;

    // Start is called before the first frame update
    void Start()
    {
        _gravityVector = new Vector3(0, -1 * Gravity, 0);
        _scaleVector = new Vector3(BallDiameter, BallDiameter, BallDiameter);
        _ballCrossSectionalArea = Mathf.PI * Mathf.Pow((BallDiameter / 2), 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _mouseCounter += Time.deltaTime;
        }
        if (Input.GetMouseButtonUp(0))
        {
            CreateNewBall();
            _mouseCounter = 0;
        }

        foreach (var namedBall in _ballCollection)
        {
            ApplyGravity(namedBall.Value);
            ApplyDrag(namedBall.Value);
            //Movement
            namedBall.Value.BallObject.transform.position += (namedBall.Value.Velocity * Time.deltaTime);
            //TODO Ball spin
        }
    }

    private void ApplyGravity(Ball ball)
    {
        ball.Velocity += _gravityVector * Time.deltaTime;
    }

    private void ApplyDrag(Ball ball)
    {
        //Drag force on ball F = 1/2 * C * A  * d * v^2
        //C = coefficent of drag, A = cross-sectional area, d = air density, v = velocity
        float force = 0.5f * DragCoefficient * _ballCrossSectionalArea * AirDenisty * (ball.Velocity.sqrMagnitude);
        float acceleration = force / BallMass;
        Vector3 backwards = -1 * ball.Velocity;
        ball.Velocity += backwards.normalized * acceleration * Time.deltaTime;

    }

    private void CreateNewBall()
    {
        //TODO add spin based on button press
        GameObject ball = Instantiate(BallTemplate, transform, true);
        ball.transform.localScale = _scaleVector;
        Vector3 fireVector = PlayerCam.transform.forward;
        fireVector.Normalize();
        ball.name = $"Ball{_ballCount}";
        BallCollision ballCollider = ball.GetComponent<BallCollision>();
        ballCollider.BallCollisionEvent.AddListener(OnBallCollision);
        Ball createdBall = new Ball(ball, fireVector * (_mouseCounter * BallLaunchSpeed));
        _ballCollection.Add(ball.name, createdBall);
        _ballCount += 1;
    }

    public void OnBallCollision(BallCollisionContainer collision)
    {
        //TODO collision calculation
    }
}
