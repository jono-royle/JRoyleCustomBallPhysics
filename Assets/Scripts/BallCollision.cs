using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BallCollision : MonoBehaviour
{
    public BallCollisionEvent BallCollisionEvent;
    // Start is called before the first frame update

    private void OnCollisionEnter(Collision collision)
    {
        BallCollisionContainer container = new BallCollisionContainer();
        container.Collision = collision;
        container.Ball = this.gameObject;
        BallCollisionEvent.Invoke(container);
    }
}

[System.Serializable]
public class BallCollisionEvent : UnityEvent<BallCollisionContainer>
{

}

public class BallCollisionContainer
{
    public Collision Collision { get; set; }
    public GameObject Ball { get; set; }   
}


