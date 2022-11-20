using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BallTests
{
    private int _collisionCount = 0;

    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("CollisionRoom");
    }

    //An example unit test that loads up the scene, fires a ball and waits 1 second to see if the ball hit the floor
    [UnityTest]
    public IEnumerator SimpleBallFireTest()
    {
        BallController ballController = (BallController)GameObject.FindObjectOfType(typeof(BallController));
        if(ballController == null)
        {
            Assert.Fail("BallController not found");
        }
        ballController.CreateNewBall(1);
        yield return new WaitForSeconds(0.1f);
        BallCollision ball = (BallCollision)GameObject.FindObjectOfType(typeof(BallCollision));
        if (ball == null)
        {
            Assert.Fail("Ball not created");
        }
        ball.BallCollisionEvent.AddListener(OnBallCollision);
        yield return new WaitForSeconds(1f);
        Assert.IsTrue(_collisionCount > 0, "Ball has not collided");
    }

    private void OnBallCollision(BallCollisionContainer arg0)
    {
        _collisionCount++;
    }
}