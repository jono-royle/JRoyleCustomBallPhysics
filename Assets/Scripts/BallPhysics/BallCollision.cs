using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallCollision : MonoBehaviour
{
    public UnityEvent<BallCollisionContainer> BallCollisionEvent;

    private void OnCollisionEnter(Collision collision)
    {
        BallCollisionContainer container = new BallCollisionContainer(collision, this.gameObject);
        BallCollisionEvent.Invoke(container);
    }
}


