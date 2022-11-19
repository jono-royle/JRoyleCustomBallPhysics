using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallController : MonoBehaviour
{
    public GameObject BallTemplate;
    public Camera PlayerCam;
    public BoxCollider Ground;
    public UnityEvent<float> SpinChanged;
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
    private float _inputSpin = 0f;
    private float _maxSpin = 500f;
    private float _rotationalInertia;
    private float _radiusSquared;

    private const float _spinIncrement = 100f;
    private const float _airViscosity = 16e-6f; //Approx value for 25 degrees c air

    // Start is called before the first frame update
    void Start()
    {
        _ballRadius = BallDiameter / 2f;
        _radiusSquared = Mathf.Pow(_ballRadius, 2);
        _gravityVector = new Vector3(0, -1 * Gravity, 0);
        _scaleVector = new Vector3(BallDiameter, BallDiameter, BallDiameter);
        _ballCrossSectionalArea = Mathf.PI * _radiusSquared;
        _rotationalInertia = (2f / 5f) * BallMass * _radiusSquared;
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

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            _inputSpin = Mathf.Min(_inputSpin + _spinIncrement, _maxSpin);
            SpinChanged.Invoke(_inputSpin);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            _inputSpin = Mathf.Max(_inputSpin - _spinIncrement, -1 * _maxSpin);
            SpinChanged.Invoke(_inputSpin);
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
            ApplyAirTorque(namedBall.Value);
            if (namedBall.Value.Velocity.magnitude > 0.6)
            {
                newBallPosition = currentBallPosition + (namedBall.Value.Velocity * Time.deltaTime);
            }
            else
            {
                newBallPosition = currentBallPosition;
            }

            namedBall.Value.BallObject.transform.position = newBallPosition;
            float spinInDegreesPerSec = namedBall.Value.Spin * (180 / Mathf.PI) * Time.deltaTime;
            namedBall.Value.BallObject.transform.Rotate(new Vector3(spinInDegreesPerSec, 0, 0), Space.World);
        }
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
        ball.transform.Rotate(new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f)), Space.World);
        Ball createdBall = new Ball(ball, fireVector * (_mouseCounter * BallLaunchSpeed), _inputSpin);
        _ballCollection.Add(ball.name, createdBall);
        _ballCount += 1;
    }

    #region Physics

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
        if (ball.Spin != 0)
        {
            float vspin = _ballRadius * ball.Spin;
            //Cl is negative for topspin
            var cl = (1 / (2 + (ball.Velocity.magnitude / Mathf.Abs(vspin)))) * Mathf.Sign(vspin);
            float force = 0.5f * cl * _ballCrossSectionalArea * AirDenisty * (ball.Velocity.sqrMagnitude);
            float acceleration = force / BallMass;
            ball.Velocity += acceleration * Vector3.up * Time.deltaTime;
        }
    }

    private void ApplyAirTorque(Ball ball)
    {
        //Torque T = 8 * pi * air viscosity* r^3 * angular velocity
        //Angular acceleration = rotational inertia / torque
        if (ball.Spin != 0)
        {
            float torque = 8 * Mathf.PI * _airViscosity * Mathf.Pow(_ballRadius, 3) * ball.Spin;
            float acceleration = torque / _rotationalInertia;
            ball.Spin -=  acceleration * Time.deltaTime;
        }
    }

    private void ApplyBounceFriction(Ball ball)
    {
        if (ball.Spin != 0)
        {
            ball.Spin = ball.Spin * (1 / (1 + (BallMass * _radiusSquared) / _rotationalInertia));
            //Horizontal component of velocity v = r * new ball spin
            ball.Velocity += new Vector3(0, 0, ball.Spin * _ballRadius * Time.deltaTime);
        }
    } 

    private void OnBallCollision(BallCollisionContainer collisionContainer)
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
            ApplyBounceFriction(colliderBall);
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

    #endregion
}
