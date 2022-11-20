using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Text BackspinText;
    public Text SidespinText;

    public void UpdateSideSpin(float spinValue)
    {
        //Convert radians per second to rpm, then show approximate value
        float rpm = spinValue / (2 * Mathf.PI) * 60;
        if (rpm > 0)
        {
            SidespinText.text = $"Leftspin: {Nearest50(rpm)} rpm";
        }
        else if (rpm < 0)
        {
            SidespinText.text = $"Rightspin: {Nearest50(rpm)} rpm";
        }
        else
        {
            SidespinText.text = "No Sidespin";
        }
    }

    public void UpdateBackspin(float spinValue)
    {
        //Convert radians per second to rpm, then show approximate value
        float rpm = spinValue / (2 * Mathf.PI) * 60;
        if (rpm > 0)
        {
            BackspinText.text = $"Backspin: {Nearest50(rpm)} rpm";
        }
        else if (rpm < 0)
        {
            BackspinText.text = $"Topspin: {Nearest50(rpm)} rpm";
        }
        else
        {
            BackspinText.text = "No Backspin";
        }
    }

    private int Nearest50(float input)
    {
        return (int)Mathf.Abs((Mathf.Round(input / 100) * 100));
    }
}
