using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Text DialogueSentence;

    public void UpdateUI(float spinValue)
    {
        //Convert radians per second to rpm, then show approximate value
        float rpm = spinValue / (2 * Mathf.PI) * 60;
        if (rpm > 0)
        {
            DialogueSentence.text = $"Backspin: {Nearest50(rpm)} rpm";
        }
        else if (rpm < 0)
        {
            DialogueSentence.text = $"Topspin: {Nearest50(rpm)} rpm";
        }
        else
        {
            DialogueSentence.text = "No spin";
        }
    }

    private int Nearest50(float input)
    {
        return (int)Mathf.Abs((Mathf.Round(input / 100) * 100));
    }
}
