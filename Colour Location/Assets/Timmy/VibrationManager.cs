using UnityEngine;
using XInputDotNetPure;
using System.Collections;

public class VibrationManager : MonoBehaviour {

    void Start()
    {
        //Vibrate(1f, 1f, 1f);
    }

    public void Vibrate(float duration, float powerLeft, float powerRight = -1f)
    {
        if (powerRight == -1)
{
            powerRight = powerLeft;
        }
        StopAllCoroutines();
        StartCoroutine(DoVibration(duration, powerLeft, powerRight));
    }

    private IEnumerator DoVibration(float duration, float powerLeft, float powerRight)
    {
        GamePad.SetVibration(PlayerIndex.One, powerLeft, powerRight);
        yield return new WaitForSeconds(duration);
        StopVibration();
    }

    public void StopVibration()
    {
        GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
    }
}