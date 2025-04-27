using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public void StartNewGame()
    {
        SceneManager.LoadScene(SceneNameConst.GAME_SCENE);
    }
}
