using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public static Vector2Int ScreenSize = new Vector2Int(1920, 1080);

    [SerializeField]
    Image _fadePanel;

    [SerializeField]
    Character _character;

    private void Start()
    {
        _character.OnDie.AddListener(() =>
        {
            Debug.Log("Die");

            // ‰æ–Ê‚ðˆÃ‚­‚·‚é->ƒV[ƒ“‘JˆÚ
            _fadePanel.gameObject.SetActive(true);
            _fadePanel.DOFade(1, 7f).onComplete = () => SceneManager.LoadScene(SceneNameConst.GAME_OVER);
        });
    }
}
