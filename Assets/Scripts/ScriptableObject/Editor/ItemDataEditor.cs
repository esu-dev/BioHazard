using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AddressableAssets;


[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Capture"))
        {
            EditorSceneManager.OpenScene("Assets/Scenes/ItemRenderingScene.unity", OpenSceneMode.Additive);

            ItemRenderingCameraProxy itemRenderingCameraProxy = GameObject.Find("ItemRenderingCamera").GetComponent<ItemRenderingCameraProxy>();


            // オブジェクトの配置
            ItemData itemData = serializedObject.targetObject as ItemData;
            GameObject item = Instantiate(itemData.Prefab);
            item.transform.position = itemData.DefaultPosition;
            item.transform.rotation = Quaternion.Euler(itemData.DefaultRotation);


            // カメラのレンダリング
            //itemRenderingCameraProxy.Render();


            RenderTexture renderTexture = itemRenderingCameraProxy.OutputTexture;

            // Texture2dへの変換
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);

            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            string path = $"{Application.dataPath}/ItemImage/{itemData.Name}.png";
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);

            
            DestroyImmediate(item);


            Debug.Log($"アイテムの撮影が完了しました。（1件）");
        }
    }
}
