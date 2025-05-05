// éQçl
// https://mackysoft.net/singleton-scriptableobject/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public abstract class Singleton_ScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get 
        {
            if (_instance == null)
            {
                _instance = Addressables.LoadAssetAsync<T>(typeof(T).Name).WaitForCompletion();
            }
            return _instance;
        }
    }
}
