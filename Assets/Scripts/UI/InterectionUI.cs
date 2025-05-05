using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InterectionUI : MonoBehaviour
{
    [SerializeField]
    float _sensedRadius;

    [SerializeField]
    GameObject _inputButtonImage;

    [SerializeField]
    Image _image;

    [SerializeField]
    InteractedObject _interactedObject;

    private void Start()
    {
        _interactedObject.OnIsFocusedChanged.AddListener(isFocused =>
        {
            _inputButtonImage.SetActive(isFocused);
            _image.enabled = !isFocused;
        });

        float y = this.transform.localPosition.y;
        this.transform.DOLocalMoveY(y + 0.1f, 0.75f).SetLoops(-1, LoopType.Yoyo);
        _inputButtonImage.transform.DOLocalMoveY(y + 0.1f, 0.75f).SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        // Player‚ªŽw’è‹——£ˆÈ“à‚É‚¢‚é‚È‚çUI‚ð•\Ž¦‚·‚é
        if (Physics.OverlapSphere(this.transform.position, _sensedRadius, 1 << LayerConst.PLAYER).Length > 0)
        {
            _image.enabled = true;
            this.transform.forward = Camera.main.transform.forward.RemoveY();
            _inputButtonImage.transform.forward = Camera.main.transform.forward.RemoveY();
        }
        else
        {
            _inputButtonImage.SetActive(false);
            _image.enabled = false;
        }
    }
}
