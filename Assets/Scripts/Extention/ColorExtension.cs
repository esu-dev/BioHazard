using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtension
{
    public static Color Transparency(this Color color)
    {
        return new Color(color.r, color.g, color.b, 0);
    }

    public static Color Visualize(this Color color)
    {
        return new Color(color.r, color.g, color.b, 1);
    }

    public static Color AddAlpha(this Color color, float a)
    {
        return new Color(color.r, color.g, color.b, color.a + a);
    }
}
