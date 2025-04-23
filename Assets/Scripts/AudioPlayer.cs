using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField]
    AudioSource _audioSource;

    float _audioTime;
    Action _endAction;
    
    
    public void Play(AudioClip audioClip, Action endAction = null, float delay = 0)
    {
        _audioSource.clip = audioClip;
        _audioSource.PlayDelayed(delay);

        _audioTime = audioClip.length;
        _endAction = endAction;
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    private void Update()
    {
        // Ä¶‚ªI—¹‚µ‚½‚ç
        if (!_audioSource.isPlaying && _audioSource.time >= _audioTime)
        {
            _endAction();
        }
    }
}
