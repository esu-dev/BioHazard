using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    static AsyncOperation _asyncOperation;

    public static float Progress
    {
        get
        {
            if (_asyncOperation == null)
            {
                return 0;
            }
            return _asyncOperation.progress;
        }
    }

    public static async Task LoadScene(string sceneName)
    {
        _asyncOperation = null;

        // ローディングシーンの読み込み
        SceneManager.LoadScene(SceneNameConst.LOADING);

        await Task.Delay(3000);

        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;

        
        TimeScheduler.CreateSchedule(2f, () => _asyncOperation.allowSceneActivation = true);
    }
}
