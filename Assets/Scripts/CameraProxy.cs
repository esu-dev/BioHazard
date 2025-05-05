using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraProxy : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera _cinemachineVirtualCamera;

    public float GetNoiseStrength()
    {
        var perlin = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        return perlin.m_AmplitudeGain;
    }

    public void SetNoiseStrength(float strength)
    {
        var perlin = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = strength;
        perlin.m_FrequencyGain = strength;
    }
}
