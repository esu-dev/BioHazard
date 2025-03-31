using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRenderingCameraProxy : SingletonMonoBehaviour<ItemRenderingCameraProxy>
{
    [SerializeField]
    Camera _camera;

    public RenderTexture OutputTexture
    {
        get
        {
            return _camera.targetTexture;
        }
    }

    public void Render()
    {
        _camera.Render();
    }
}
