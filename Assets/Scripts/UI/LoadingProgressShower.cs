using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingProgressShower : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _loadingText;

    [SerializeField]
    TextMeshProUGUI _percentText;

    int p;

    private void Start()
    {
        StartCoroutine(ShowLoadingText());
    }

    private void Update()
    {
        //_percentText.text = $"{(int)(SceneLoader.Progress * 100)}%";
        _percentText.text = $"{p++}%";
    }

    IEnumerator ShowLoadingText()
    {
        int index = 0;
        string text = "NOW LOADING...";

        while (true)
        {
            _loadingText.text = text.Substring(0, index++);

            if (index > text.Length)
            {
                index = 0;
            }

            yield return new WaitForSeconds(0.25f);
        }

    }
}
