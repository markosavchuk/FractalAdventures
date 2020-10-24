using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSHelper : MonoBehaviour
{
    [SerializeField] private float updateTime = 0.5f;
    [SerializeField] private Text framerateInfoText;

    private void Start()
    {
        StartCoroutine(UpdateFPSCounter());
    }    

    IEnumerator UpdateFPSCounter()
    {
        while (true)
        {
            int count = 0;
            float timeSinceLastUpdate = 0.0f;
            while (timeSinceLastUpdate < updateTime)
            {
                yield return new WaitForEndOfFrame();
                timeSinceLastUpdate += Time.unscaledDeltaTime;
                count++;
            }
            framerateInfoText.text = $"Framerate: {CalculateFPS(timeSinceLastUpdate, count)}";
        }
    }

    string CalculateFPS(float timeSinceLastUpdate, int count)
    {
        if (count != 0)
        {
            float averageTimePerFrame = timeSinceLastUpdate / count;
            int newFramerate = (int)(1.0f / averageTimePerFrame);
            return newFramerate.ToString();
        }
        else return "...";
    }
}
