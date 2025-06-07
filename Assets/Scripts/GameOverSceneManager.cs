using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameOverSceneManager : MonoBehaviour
{
    [SerializeField]
    CanvasGroup _uiGroup;

    public void LoadTitleScene()
    {
        SceneLoader.LoadScene(SceneNameConst.TITLE);
    }

    private void Start()
    {
        _uiGroup.alpha = 0;
        _uiGroup.DOFade(1, 3f);

        GameStateManager.Instance.ChangeStateToPauseState();
    }
}
