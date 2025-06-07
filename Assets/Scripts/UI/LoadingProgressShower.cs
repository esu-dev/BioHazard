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
        _percentText.text = "0%";

        StartCoroutine(ShowLoadingText());
    }

    private void Update()
    {
        _percentText.text = $"{(int)(SceneLoader.Progress * 100)}%";
        //_percentText.text = $"{p++}%";
    }

    IEnumerator ShowLoadingText()
    {
        int index = 0;
        string text = "NOW LOADING...";

        _loadingText.text = text;

        while (true)
        {
            _loadingText.maxVisibleCharacters = index;

            if (index > text.Length)
            {
                index = 0;
            }

            index++;

            yield return new WaitForSeconds(0.15f);
        }

    }
}
