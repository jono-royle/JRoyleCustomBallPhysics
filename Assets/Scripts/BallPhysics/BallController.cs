using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallController : MonoBehaviour
{
    public GameObject BallTemplate;
    public Camera PlayerCam;
    public BoxCollider BoundingBox;
    public UnityEvent<float> BackSpinChanged;
    public UnityEvent<float> SideSpinChanged;
    [Range(1f, 500)] public float BallLaunchSpeed = 40f;
    [Range(0.03f, 2)] public float BallDiameter = 0.06f;
    [Range(0.01f, 100)] public float BallMass = 0.06f;
    [Range(0, 2)] public float DragCoefficient = 0.5f;
    [Range(0, 1)] public float RestitutionCoefficient = 0.7f;
    [Range(0, 25)] public float Gravity = 9.82f;
    [Range(0, 10)] public float AirDenisty = 1.2f;
    [Range(0, 30)] public float AirViscosity = 16f;

    private Dictionary<string, Ball> _ballCollection = new Dictionary<string, Ball>();
    private int _ballCount = 0;
    private float _mouseCounter = 0f;
    private Vector3 _gravityVector;
    private Vector3 _scaleVector;
    private float _ballCrossSectionalArea;
    private float _ballRadius;
    private Vector3 _inputSpin = Vector3.zero;
    private Vector3 _maxSpin = new Vector3(500, 500, 500);
    private float _rotationalInertia;
    private float _radiusSquared;
    private float _spinIncrement = 100f;
    private float _airViscosity; //Approx value for 25 degrees c air

    // Start is called before the first frame update
    void Start()
    {
        _ballRadius = BallDiameter / 2f;
        _radiusSquared = Mathf.Pow(_ballRadius, 2);
        _gravityVector = new Vector3(0, -1 * Gravity, 0);
        _scaleVector = new Vector3(BallDiameter, BallDiameter, BallDiameter);
        _ballCrossSectionalArea = Mathf.PI * _radiusSquared;
        _rotationalInertia = (2f / 5f) * BallMass * _radiusSquared;
        _airViscosity = AirViscosity * 1e-6f;
        Vector3 bbSize = BoundingBox.size;
        BoundingBox.size = new Vector3(bbSize.x - BallDiameter, bbSize.y - BallDiameter, bbSize.z - BallDiameter);
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayerInput();

        foreach (var namedBall in _ballCollection)
        {
            Vector3 newBallPosition;
            Vector3 currentBallPosition = namedBall.Value.BallObject.transform.position;

            ApplyGravity(namedBall.Value);
            ApplyAirTorque(namedBall.Value);
            ApplyDrag(namedBall.Value);
            ApplyMagnusForce(namedBall.Value);

            newBallPosition = currentBallPosition + (namedBall.Value.Velocity * Time.deltaTime);

            if (!BoundingBox.bounds.Contains(newBallPosition))
            {
                newBallPosition = BoundingBox.bounds.ClosestPoint(newBallPosition);
            }

            namedBall.Value.BallObject.transform.position = newBallPosition;
            Vector3 spin = namedBall.Value.Spin;
            //convert spin velocity to rotation around the perpendicular axis
            namedBall.Value.BallObject.transform.Rotate(new Vector3(spin.y,spin.z,spin.x) * Mathf.Rad2Deg * Time.deltaTime, Space.World);
        }
    }

    private void GetPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            _inputSpin = _inputSpin + _spinIncrement * Vector3.up;
            if (_inputSpin.y > _maxSpin.y)
            {
                _inputSpin.y = _maxSpin.y;
            }
            BackSpinChanged.Invoke(_inputSpin.y);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            _inputSpin = _inputSpin - _spinIncrement * Vector3.up;
            if (_inputSpin.y < -1 * _maxSpin.y)
            {
                _inputSpin.y = -1 * _maxSpin.y;
            }
            BackSpinChanged.Invoke(_inputSpin.y);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _inputSpin = _inputSpin + _spinIncrement * Vector3.right;
            if (_inputSpin.x > _maxSpin.x)
            {
                _inputSpin.x = _maxSpin.x;
            }
            SideSpinChanged.Invoke(_inputSpin.x);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _inputSpin = _inputSpin - _spinIncrement * Vector3.right;
            if (_inputSpin.x < -1 * _maxSpin.x)
            {
                _inputSpin.x = -1 * _maxSpin.x;
            }
            SideSpinChanged.Invoke(_inputSpin.x);
        }

        if (Input.GetMouseButton(0))
        {
            _mouseCounter += Time.deltaTime;
        }
        if (Input.GetMouseButtonUp(0))
        {
            CreateNewBall(_mouseCounter);
            _mouseCounter = 0;
        }
    }

    public void CreateNewBall(float mouseCounter)
    {
        GameObject ball = Instantiate(BallTemplate, transform, true);
        ball.transform.localScale = _scaleVector;
        //fire ball in direction of camera
        Vector3 fireVector = PlayerCam.transform.forward;
        fireVector.Normalize();
        ball.name = $"Ball{_ballCount}";
        BallCollision ballCollider = ball.GetComponent<BallCollision>();
        ballCollider.BallCollisionEvent.AddListener(OnBallCollision);
        //randomly rotate the ball before firing (for aesthetic reasons only)
        ball.transform.Rotate(new Vector3(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f)), Space.World);
        Ball createdBall = new Ball(ball, fireVector * (mouseCounter * BallLaunchSpeed), _inputSpin);
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
        //Drag coefficient of a spinning ball, formula from experimental data http://www.physics.usyd.edu.au/~cross/TRAJECTORIES/42.%20Ball%20Trajectories.pdf 
        float spinningCD = DragCoefficient + 1f / Mathf.Pow(22.5f + 4.2f * Mathf.Pow(ball.Velocity.magnitude / ball.Spin.magnitude, 2.5f),0.4f);
        //Drag force on ball F = 1/2 * Cd * A  * d * v^2
        //Cd = coefficent of drag, A = cross-sectional area, d = air density, v = velocity
        float force = 0.5f * spinningCD * _ballCrossSectionalArea * AirDenisty * (ball.Velocity.sqrMagnitude);
        float acceleration = force / BallMass;
        Vector3 backwards = -1 * ball.Velocity;
        ball.Velocity += backwards.normalized * acceleration * Time.deltaTime;
    }

    private void ApplyMagnusForce(Ball ball)
    {
        //Magnus force on ball F = 1/2 * Cl * A  * d * v^2
        //Cl = lift coefficient = 1/(2 + (v/vspin)), vspin = radius * w angular speed
        if (ball.Spin.magnitude != 0)
        {
            float cl = 1f / (2f + (ball.Velocity.magnitude / ball.Spin.magnitude));
            float force = 0.5f * cl * _ballCrossSectionalArea * AirDenisty * (ball.Velocity.sqrMagnitude);
            float acceleration = force / BallMass;
            ball.Velocity += acceleration * ball.Spin.normalized * Time.deltaTime;
        }
    }

    private void ApplyAirTorque(Ball ball)
    {
        //Torque T = 8 * pi * air viscosity* r^3 * angular velocity
        //Angular acceleration = rotational inertia / torque
        if (ball.Spin.magnitude != 0)
        {
            Vector3 torque = 8f * Mathf.PI * _airViscosity * Mathf.Pow(_ballRadius, 3) * ball.Spin;
            Vector3 deceleration = torque / _rotationalInertia;
            ball.Spin -=  deceleration * Time.deltaTime;
        }
    }

    private void ApplyBounceFriction(Ball ball)
    {
        if (ball.Spin.magnitude != 0)
        {
            //Spin after bounce = (1 / (1 + m * r^2 / rotationalInertia)) * Spin
            ball.Spin = ball.Spin * (1 / (1 + (BallMass * _radiusSquared) / _rotationalInertia));
            //Component of velocity in the direction of spin v = r * new ball spin
            ball.Velocity += new Vector3(ball.Spin.z, ball.Spin.x, ball.Spin.y) * _ballRadius * Time.deltaTime;
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
        else if (collisionContainer.Collision.gameObject.tag == "Ball")
        {
            Ball secondBall = _ballCollection[collisionContainer.Collision.gameObject.name];
            if (colliderBall.CollidedWith == secondBall.BallObject.name)
            {
                //we already handled this collision in the other balls onCollision
                colliderBall.CollidedWith = "";
            }
            else
            {
                //affect each ball by the normal component of the relative velocity of the collision
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
