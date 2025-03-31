using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{
    public static Vector3 ToVector3XZ(this Vector2 vector)
    {
        return new Vector3(vector.x, 0, vector.y);
    }

    public static Vector2Int ToVector2Int(this Vector2 vector)
    {
        return new Vector2Int((int)vector.x, (int)vector.y);
    }
}
