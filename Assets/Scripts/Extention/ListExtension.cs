using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ListExtension
{
    public static bool Contains3<T>(this List<List<List<T>>> list, T value)
    {
        for (int i = 0; i < list.Count(); i++)
        {
            for (int j = 0; j < list[0].Count(); j++)
            {
                if (list[i][j].Contains(value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*public static List<List<List<T>>> Extract<T>(this List<List<List<T>>> list, int x_start, int x_end, int y_start, int y_end, int z_start, int z_end)
    {
        List<List<List<T>>> newList;
        newList.Add()
        return newList;
    }*/

    public static string ToStringWithCommma<T>(this List<T> list)
    {
        string s = "";
        for (int i = 0; i < list.Count(); i++)
        {
            s += list[i].ToString();

            if (i < list.Count() - 1)
            {
                s += ", ";
            }
        }
        return s;
    }

    public static List<T> PooledCopyFrom<T>(this List<T> copiedList, List<T> originalList)
    {
        copiedList.Clear();
        foreach (T t in originalList)
        {
            copiedList.Add(t);
        }

        return copiedList;
    }

    public static T GetValue<T>(this List<List<T>> list, Vector2Int position, T defaultValue)
    {
        if (position.x < 0 || position.x >= list.Count ||
            position.y < 0 || position.y >= list[position.x].Count)
        {
            return defaultValue;
        }

        return list[position.x][position.y];
    }
}
