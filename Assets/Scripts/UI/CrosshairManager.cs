using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairManager : MonoBehaviour
{
    [SerializeField]
    Character _character;

    [SerializeField]
    Crosshair[] _crosshairParts;
 
    public void Focus(int level, float time)
    {
        foreach (Crosshair crosshair in _crosshairParts)
        {
            crosshair.Focus(level, time);
        }
    }

    private void Start()
    {
        this.gameObject.SetActive(false);
        foreach (Crosshair crosshair in _crosshairParts)
        {
            crosshair.Initialize();
        }
        Focus(0, 0);

        _character.OnAimStateChange.AddListener(isAimState =>
        {
            this.gameObject.SetActive(isAimState);

            // ƒNƒƒXƒwƒA‚ð–ß‚·
            if (!isAimState)
            {
                Focus(0, 0.25f);
            }
        });
        _character.OnFocus.AddListener(() => Focus(0, 0.25f));
        _character.OnWalk.AddListener(() => Focus(2, 0.5f));
        _character.OnStand.AddListener(() => Focus(1, 0.5f));
    }
}
