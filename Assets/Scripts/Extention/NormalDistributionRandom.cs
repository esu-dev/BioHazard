using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public static class NormalDistributionRandom
{
    public static int Range(int minInclusive, int maxInclusive)
    {
        float x = Random.Range(0, 1.0f);
        float y = Random.Range(0, 1.0f);

        float z = (Mathf.Sqrt(-2 * Mathf.Log(x)) * Mathf.Cos(2.0f * Mathf.PI * y) + 1) / 2;

        return (int)(minInclusive + (maxInclusive - minInclusive) * z);
    }
}
