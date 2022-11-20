using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Ball
    {
        public Ball(GameObject ballObject, Vector3 initialVelocity, Vector3 spin, float radius)
        {
            BallObject = ballObject;
            Velocity = initialVelocity;
            Spin = spin;
            _radius = radius;
        }
        public GameObject BallObject { get; }
        public Vector3 Velocity { get; set; }
        public string CollidedWith { get; set; }
        public Vector3 Spin { get; set; }

        private float _radius;
        public float HeightFromGround()
        {
            return BallObject.transform.position.y + 3 - _radius;
        }
    }
}
