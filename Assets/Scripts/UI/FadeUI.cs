using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FadeUI : MonoBehaviour
{
    [SerializeField, Range(0, 255)]
    float _transparency;

    [SerializeField]
    bool _applyToChild;

    
    public Tween Transparency(float time)
    {
        Tween t = DOVirtual.DelayedCall(time, null);
        this.GetComponent<Image>()?.DOFade(0, time);
        this.GetComponent<TextMeshProUGUI>()?.DOFade(0, time);

        if (_applyToChild)
        {
            void fade(Transform parent)
            {
                foreach (Transform child in parent)
                {
                    child.GetComponent<FadeUI>()?.Transparency(time);

                    fade(child);
                }
            }

            fade(this.transform);
        }

        return t;
    }

    public Tween Visualize(float time)
    {
        Tween t = DOVirtual.DelayedCall(time, null);
        this.GetComponent<Image>()?.DOFade(_transparency / 255f, time);
        this.GetComponent<TextMeshProUGUI>()?.DOFade(_transparency / 255f, time);

        if (_applyToChild)
        {
            void fade(Transform parent)
            {
                foreach (Transform child in parent)
                {
                    child.GetComponent<FadeUI>()?.Visualize(time);

                    fade(child);
                }
            }

            fade(this.transform);
        }

        return t;
    }

    public void VisualizeImmediately()
    {
        if (this.TryGetComponent(out Image image))
        {
            image.color = image.color.SetAlpha(_transparency / 255f);
        }

        if (this.TryGetComponent(out TextMeshProUGUI textMeshProUGUI))
        {
            textMeshProUGUI.color = textMeshProUGUI.color.SetAlpha(_transparency / 255f);
        }

        if (_applyToChild)
        {
            void fade(Transform parent)
            {
                foreach (Transform child in parent)
                {
                    child.GetComponent<FadeUI>()?.VisualizeImmediately();

                    fade(child);
                }
            }

            fade(this.transform);
        }
    }
}
