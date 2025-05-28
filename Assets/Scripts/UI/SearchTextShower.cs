using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SearchTextShower : SingletonMonoBehaviour<SearchTextShower>
{
    [SerializeField]
    TextMeshProUGUI _searchText;

    public void ShowText(string text)
    {
        _searchText.gameObject.SetActive(true);

        StartCoroutine(Show(text));
    }

    private void Start()
    {
        _searchText.gameObject.SetActive(false);
    }

    IEnumerator Show(string text)
    {
        _searchText.text = text;
        _searchText.maxVisibleCharacters = 0;

        while (_searchText.maxVisibleCharacters < text.Length)
        {
            _searchText.maxVisibleCharacters++;

            yield return new WaitForSeconds(0.025f);
        }

        yield return new WaitForSeconds(1f);

        _searchText.gameObject.SetActive(false);
    }
}
