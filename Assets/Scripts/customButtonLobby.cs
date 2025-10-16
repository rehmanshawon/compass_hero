using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class customButtonLobby : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CanvasGroup btn;
    public MainUI ui;
    public GameObject panel;

    private Tween hoverTween;

    public void SetSelected(bool check)
    {
        if (check)
        {
            btn.alpha = 1;
            panel.SetActive(true);
        }
        else
        {
            btn.alpha = 1f;
            panel.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ui.selectButton(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverTween?.Kill();

        hoverTween = transform.DOScale(1.07f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverTween?.Kill();

        transform.DOScale(1f, 0.2f).SetEase(Ease.OutSine);
    }
}