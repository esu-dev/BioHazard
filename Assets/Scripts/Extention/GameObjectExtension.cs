using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class GameObjectExtension
{
    public static void SetLayer(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetLayer(layer);
        }
    }
}
