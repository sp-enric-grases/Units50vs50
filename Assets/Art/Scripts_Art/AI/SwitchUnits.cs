using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchUnits : MonoBehaviour
{
    public Text text;
    public GameObject unitsAnimator;
    public GameObject unitsAnimation;
    public GameObject unitsNoAnimation;
    public GameObject unitsGPUSkinning;

    private int selector = -1;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    public void Switch ()
    {
        selector++;
        if (selector >= 4) selector = 0;

        switch (selector)
        {
            case 0: SetVisibility("With Animator", true, false, false, false); break;
            case 1: SetVisibility("With Simple Animation", false, true, false, false); break;
            case 2: SetVisibility("Without animations", false, false, true, false); break;
            case 3: SetVisibility("With GPU Skinning", false, false, false, true); break;
        }

        Debug.Log(selector);
    }

    private void SetVisibility(string message, params bool[] state)
    {
        unitsAnimator.SetActive(state[0]);
        unitsAnimation.SetActive(state[1]);
        unitsNoAnimation.SetActive(state[2]);
        unitsGPUSkinning.SetActive(state[3]);
        text.text = message;
    }
}
