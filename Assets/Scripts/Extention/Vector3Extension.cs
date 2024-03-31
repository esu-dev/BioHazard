using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extension
{
    public static Vector3 RemoveX(this Vector3 vector)
    {
        return new Vector3(0, vector.y, vector.z);
    }

    public static Vector3 RemoveY(this Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    public static Vector3 RemoveZ(this Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, 0);
    }

    public static Vector3 AddX(this Vector3 vector, float x)
    {
        return new Vector3(vector.x + x, vector.y, vector.z);
    }

    public static Vector3 AddY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, vector.y + y, vector.z);
    }

    public static Vector3 AddZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, vector.z + z);
    }

    public static Vector3 Times(this Vector3 a, Vector3 b)
    {
        a.x *= b.x;
        a.y *= b.y;
        a.z *= b.z;

        return a;
    }

    public static Vector3 Devide(this Vector3 a, Vector3 b)
    {
        a.x /= b.x;
        a.y /= b.y;
        a.z /= b.z;

        return a;
    }

    public static Vector2 ToVector2XZ(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    public static Vector3Int ToVector3Int(this Vector3 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
    }

    public static Vector3Int ToVector3Int_Round (this Vector3 vector)
    {
        return new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
    }

    public static Vector3 Select(this Vector3 vector, Func<float, float> action)
    {
        vector.x = action(vector.x);
        vector.y = action(vector.y);
        vector.z = action(vector.z);

        return vector;
    }
}
