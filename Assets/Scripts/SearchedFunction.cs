using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchedFunction : InterectedFunction
{
    [SerializeField]
    string _text;

    public override void Invoke()
    {
        // テキストを表示
        SearchTextShower.Instance.ShowText(_text);
    }
}

public abstract class InterectedFunction : MonoBehaviour
{
    public abstract void Invoke();
}
