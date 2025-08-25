using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HouseObjectDataBase", menuName = "ScriptableObjects/HouseObjectDataBase")]
public class HouseObjectDataBase : ScriptableObject
{
    [field: SerializeField]
    public List<HouseObjectData> HouseObjectDataList { get; private set; }
}
