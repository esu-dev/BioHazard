using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    float _intensity;

    private void Awake()
    {
        RenderSettings.ambientIntensity = _intensity;
    }
}
