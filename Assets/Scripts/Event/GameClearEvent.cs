using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClearEvent : InterectedFunction
{
    public override void Invoke()
    {
        SceneManager.LoadScene(SceneNameConst.END);
    }
}
