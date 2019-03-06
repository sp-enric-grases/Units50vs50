using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour
{
    private const int _maxSmoothFps = 10;
    private Text m_Text;

    private List<float> _listFPS = new List<float>();

    private void Start()
    {
        m_Text = GetComponent<Text>();
    }


    private void Update()
    {
        if (_listFPS.Count < _maxSmoothFps)
        {
            _listFPS.Add(1.0f / Time.unscaledDeltaTime);
        }
        else
        {
            _listFPS.RemoveAt(0);
            _listFPS.Add(1.0f / Time.unscaledDeltaTime);
        }

        var fps = 0.0f;
        for (var i = 0; i < _listFPS.Count; i++)
        {
            fps += _listFPS[i];
        }

        fps /= _listFPS.Count;
        m_Text.text = fps.ToString("0");
    }
}