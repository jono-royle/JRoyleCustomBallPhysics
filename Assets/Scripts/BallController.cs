using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject BallTemplate;
    public Camera PlayerCam;
    public List<BoxCollider> Walls;
    public BoxCollider Ground;
    [Range(1f, 100)] public float BallLaunchSpeed = 40f;
    [Range(0.1f, 25)] public float Gravity = 9.8f;
    [Range(0.01f, 2)] public float BallDiameter = 0.06f;
    [Range(0, 2)] public float DragCoefficient = 0.5f;
    [Range(0, 10)] public float AirDenisty = 1.2f;
    [Range(0.01f, 100)] public float BallMass = 0.06f;
    [Range(0, 1)] public float RestitutionCoefficient = 0.7f;

    private Dictionary<string, Ball> _ballCollection = new Dictionary<string, Ball>();
    private int _ballCount = 0;
    private float _mouseCounter = 0f;
    private Vector3 _gravityVector;
    private Vector3 _scaleVector;
    private float _ballCrossSectionalArea;
    private float _ballRadius;

    // Start is called before the first frame update
    void Start()
    {
        _gravityVector = new Vector3(0, -1 * Gravity, 0);
        _ballRadius = BallDiameter / 2;
        _scaleVector = new Vector3(BallDiameter, BallDiameter, BallDiameter);
        _ballCrossSectionalArea = Mathf.PI * Mathf.Pow(_ballRadius, 2);
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
            Vector3 newBallPosition;
            Vector3 currentBallPosition = namedBall.Value.BallObject.transform.position;

            Vector3 closestPointOnGround = Ground.bounds.ClosestPoint(currentBallPosition);
            float distanceToGround = Vector3.Distance(currentBallPosition, closestPointOnGround);
            if (distanceToGround <= _ballRadius)
            {
                currentBallPosition = currentBallPosition + Vector3.up * (_ballRadius - distanceToGround);
            }
            else
            {
                ApplyGravity(namedBall.Value);
            }
            ApplyDrag(namedBall.Value);
            ApplyMagnusForce(namedBall.Value);
            if (namedBall.Value.Velocity.magnitude > 0.2)
            {
                newBallPosition = currentBallPosition + (namedBall.Value.Velocity * Time.deltaTime);
            }
            else
            {
                newBallPosition = currentBallPosition;
            }

            foreach (var wall in Walls)
            {
                newBallPosition = PreventWallClipping(newBallPosition, wall);
            }

            namedBall.Value.BallObject.transform.position = newBallPosition;
            float spinInDegreesPerSec = namedBall.Value.Spin * (180 / Mathf.PI) * Time.deltaTime;
            namedBall.Value.BallObject.transform.Rotate(new Vector3(0, spinInDegreesPerSec, 0));
        }
    }

    private Vector3 PreventWallClipping(Vector3 newBallPosition, BoxCollider wall)
    {
        Vector3 closestPointOnWall = wall.bounds.ClosestPoint(newBallPosition);
        float distanceToWall = Vector3.Distance(newBallPosition, closestPointOnWall);
        if (distanceToWall <= _ballRadius)
        {
            newBallPosition += (newBallPosition - closestPointOnWall);
        }

        return newBallPosition;
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

    private void ApplyMagnusForce(Ball ball)
    {
        //Magnus force on ball F = 1/2 * Cl * A  * d * v^2
        //Cl = lift coefficient = 1/(2 + (v/vspin)), vspin = radius * w angular speed
        float vspin = _ballRadius * ball.Spin;
        //Cl is negative for topspin
        var cl = (1 / (2 + (ball.Velocity.magnitude / Mathf.Abs(vspin)))) * Mathf.Sign(vspin);
        float force = 0.5f * cl * _ballCrossSectionalArea * AirDenisty * (ball.Velocity.sqrMagnitude);
        ball.Velocity += force * Vector3.up * Time.deltaTime;
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
        Ball createdBall = new Ball(ball, fireVector * (_mouseCounter * BallLaunchSpeed), 500);
        _ballCollection.Add(ball.name, createdBall);
        _ballCount += 1;
    }

    public void OnBallCollision(BallCollisionContainer collisionContainer)
    {
        Ball colliderBall = _ballCollection[collisionContainer.Ball.name];
        float ballSpeed = colliderBall.Velocity.magnitude;
        Vector3 intialDirection = colliderBall.Velocity.normalized;
        Vector3 collisionNormal = collisionContainer.Collision.contacts[0].normal;
        if (collisionContainer.Collision.gameObject.tag == "Scenery")
        {
            //Reflection of a vector V against a plane with normal N: R=V-2N(VdotN)
            Vector3 newDirection = intialDirection - 2 * collisionNormal * Vector3.Dot(intialDirection, collisionNormal);
            colliderBall.Velocity = newDirection.normalized * ballSpeed * RestitutionCoefficient;
        }
        else
        {
            Ball secondBall = _ballCollection[collisionContainer.Collision.gameObject.name];
            if (colliderBall.CollidedWith == secondBall.BallObject.name)
            {
                //we already handled this collision in the other balls onCollision
                colliderBall.CollidedWith = "";
            }
            else
            {
                Vector3 relativeVelocity = colliderBall.Velocity - secondBall.Velocity;
                Vector3 normalVelocity = Vector3.Dot(relativeVelocity, collisionNormal) * collisionNormal;
                colliderBall.Velocity = (colliderBall.Velocity - normalVelocity) * RestitutionCoefficient;
                secondBall.Velocity = (secondBall.Velocity + normalVelocity) * RestitutionCoefficient;
                secondBall.CollidedWith = colliderBall.BallObject.name;
            }
        }
    }
}
