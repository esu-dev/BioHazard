using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HouseObjectData", menuName = "ScriptableObjects/HouseObjectData")]
public class HouseObjectData : ScriptableObject
{
    [field: SerializeField]
    public GameObject Prefab { get; private set; }

    [field: SerializeField]
    public float Wall {  get; private set; }

    [field: SerializeField]
    public List<HouseObjectRelationship> RelationshipList { get; private set; }

    [System.Serializable]
    public class HouseObjectRelationship
    {
        [field: SerializeField]
        public HouseObjectData houseObjectData { get; private set; }

        [field: SerializeField]
        public float Weight { get; private set; }
    }
}
