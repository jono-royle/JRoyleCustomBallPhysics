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
        public Ball(GameObject ballObject, float initialSpeed)
        {
            BallObject = ballObject;
            Speed = initialSpeed;
        }
        public GameObject BallObject { get; }
        public float Speed { get; set; }
    }
}
