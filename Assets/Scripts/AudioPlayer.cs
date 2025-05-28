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
    bool _isPlaying;
    Action _endAction;
    
    
    public void Play(AudioClip audioClip, Action endAction = null, float delay = 0)
    {
        _audioSource.clip = audioClip;
        _audioSource.time = 0;
        _audioSource.PlayDelayed(delay);

        _audioTime = audioClip.length;
        _isPlaying = true;
        _endAction = endAction;
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    private void Update()
    {
        // Ä¶‚ªI—¹‚µ‚½‚ç
        if (_isPlaying && _audioSource.time >= _audioTime)
        {
            _isPlaying = false;
            _endAction();
        }
    }
}
