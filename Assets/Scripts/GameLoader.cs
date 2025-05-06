using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameLoader : MonoBehaviour
{
    [SerializeField]
    CanvasGroup _titleTextGroup;

    public void StartNewGame()
    {
        _titleTextGroup.DOFade(0, 2f).onComplete = () => SceneLoader.LoadScene(SceneNameConst.GAME);
    }
}
