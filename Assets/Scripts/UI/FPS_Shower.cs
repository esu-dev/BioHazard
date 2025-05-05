using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS_Shower : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _fpsText;

    int _frame;
    float _fpsSum;

    private void Update()
    {
        _fpsSum += 1 / Time.deltaTime;

        if (_frame++ == 10)
        {
            _fpsText.text = $"FPS: {(_fpsSum / 10).ToString("F1")}";
            _frame = 0;
            _fpsSum = 0;
        }
    }
}
