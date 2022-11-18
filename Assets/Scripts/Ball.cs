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
        public Ball(GameObject ballObject, Vector3 initialVelocity)
        {
            BallObject = ballObject;
            Velocity = initialVelocity;
        }
        public GameObject BallObject { get; }
        public Vector3 Velocity { get; set; }
    }
}
