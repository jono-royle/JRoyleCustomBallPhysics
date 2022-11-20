using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Text BackspinText;
    public Text SidespinText;
    public Text LastBallSpeed;
    public Text LastBallSpin;
    public Text LastBallHeight;

    public void UpdateSideSpin(float spinValue)
    {
        //Convert radians per second to rpm, then show approximate value
        float rpm = GetRPMFromRadiansPerSec(spinValue);
        if (rpm > 0)
        {
            SidespinText.text = $"Leftspin: {Nearest100(rpm)} rpm";
        }
        else if (rpm < 0)
        {
            SidespinText.text = $"Rightspin: {Nearest100(rpm)} rpm";
        }
        else
        {
            SidespinText.text = "No Sidespin";
        }
    }

    public void UpdateBackspin(float spinValue)
    {
        //Convert radians per second to rpm, then show approximate value
        float rpm = GetRPMFromRadiansPerSec(spinValue);
        if (rpm > 0)
        {
            BackspinText.text = $"Backspin: {Nearest100(rpm)} rpm";
        }
        else if (rpm < 0)
        {
            BackspinText.text = $"Topspin: {Nearest100(rpm)} rpm";
        }
        else
        {
            BackspinText.text = "No Backspin";
        }
    }

    public void UpdateLastBall(Ball ball)
    {
        LastBallSpeed.text = $"Speed: {ball.Velocity.magnitude.ToString("0.00")} m/s";
        LastBallSpin.text = $"Spin: { GetRPMFromRadiansPerSec(ball.Spin.magnitude).ToString("F0")} rpm";
        LastBallHeight.text = $"Height: {ball.HeightFromGround().ToString("0.00")} m";
    }

    private int Nearest100(float input)
    {
        return (int)Mathf.Abs((Mathf.Round(input / 100) * 100));
    }

    private float GetRPMFromRadiansPerSec(float rads)
    {
        return rads / (2 * Mathf.PI) * 60;
    }
}
