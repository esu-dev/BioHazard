using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData")]
public class AudioData : ScriptableObject
{
    [field: SerializeField]
    public AudioClip[] _audioClips { get; private set; }
}
