using UnityEngine;

public class BallCollisionContainer
{
    public BallCollisionContainer(Collision collision, GameObject ball)
    {
        Collision = collision;
        Ball = ball;
    }
    public Collision Collision { get; }
    public GameObject Ball { get; }   
}


