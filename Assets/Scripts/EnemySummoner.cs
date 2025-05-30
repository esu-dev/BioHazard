using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySummoner : MonoBehaviour
{
    [SerializeField]
    GameObject[] _enemyPrefabs;

    private void Start()
    {
        int index = Random.Range(0, _enemyPrefabs.Length);
        Instantiate(_enemyPrefabs[index], this.transform.position, this.transform.rotation);
        Destroy(this.gameObject);
    }
}
