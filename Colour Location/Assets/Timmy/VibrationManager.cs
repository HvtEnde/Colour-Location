using UnityEngine;
using XInputDotNetPure;
using System.Collections;

public class VibrationManager : MonoBehaviour
{

    private float currentDistance;

    private void Start()
    {
        ClearDistance();
    }

    public void Vibrate(float distance, float duration, float powerLeft, float powerRight = -1f)
    {
        if (distance > currentDistance)
        {
            return;
        }
        currentDistance = distance;

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

    private void ClearDistance()
    {
        currentDistance = float.MaxValue;
    }
}