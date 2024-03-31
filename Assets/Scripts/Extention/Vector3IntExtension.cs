using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3IntExtension
{
    public static Vector3Int RemoveX(this Vector3Int vector)
    {
        return new Vector3Int(0, vector.y, vector.z);
    }

    public static Vector3Int RemoveY(this Vector3Int vector)
    {
        return new Vector3Int(vector.x, 0, vector.z);
    }

    public static Vector3Int RemoveZ(this Vector3Int vector)
    {
        return new Vector3Int(vector.x, vector.y, 0);
    }

    public static Vector3Int AddX(this Vector3Int vector, int x)
    {
        return new Vector3Int(vector.x + x, vector.y, vector.z);
    }

    public static Vector3Int AddY(this Vector3Int vector, int y)
    {
        return new Vector3Int(vector.x, vector.y + y, vector.z);
    }

    public static Vector3Int AddZ(this Vector3Int vector, int z)
    {
        return new Vector3Int(vector.x, vector.y, vector.z + z);
    }

    /*public static Vector2 ToVector2XZ(this Vector3Int vector)
    {
        return new Vector2(vector.x, vector.z);
    }*/

    public static Vector3 ToVector3(this Vector3Int vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }
}
