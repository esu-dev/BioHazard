using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ItemImageCapture : MonoBehaviour
{
    [SerializeField]
    ItemData[] _itemDatas;

    [SerializeField]
    Camera _camera;


    private void Start()
    {
        StartCoroutine(Capture());
    }

    IEnumerator Capture()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < _itemDatas.Length; i++)
        {
            // オブジェクトの配置
            ItemData itemData = _itemDatas[i];
            GameObject item = Instantiate(itemData.Prefab);
            item.transform.position = itemData.DefaultPosition;
            item.transform.rotation = Quaternion.Euler(itemData.DefaultRotation);

            yield return new WaitForSeconds(1f);

            //_camera.Render();

            yield return new WaitForSeconds(1f);


            RenderTexture renderTexture = _camera.targetTexture;

            // Texture2dへの変換
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);

            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            string path = $"Assets/ItemImage/{itemData.Name}.png";
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);


            Debug.Log($"{_itemDatas[i].Name}の撮影が完了しました。");

            yield return new WaitForSeconds(1f);

            DestroyImmediate(item);

            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"全てのアイテムの撮影が完了しました。（{_itemDatas.Length}件）");
    }
}
