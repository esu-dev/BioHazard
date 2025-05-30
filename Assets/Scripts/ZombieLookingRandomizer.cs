using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieLookingRandomizer : MonoBehaviour
{
    [SerializeField]
    GameObject[] _zombieParts;

    private void Start()
    {
        foreach (GameObject part in _zombieParts)
        {
            int random = Random.Range(0, 2);
            part.SetActive(random == 0 ? false : true);
        }
    }
}
