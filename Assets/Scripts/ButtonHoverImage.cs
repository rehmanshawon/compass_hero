using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite defaultImage;
    public Sprite hoverImage;

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();

        // Set default image at start
        if (buttonImage != null && defaultImage != null)
        {
            buttonImage.sprite = defaultImage;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null && hoverImage != null)
        {
            buttonImage.sprite = hoverImage;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null && defaultImage != null)
        {
            buttonImage.sprite = defaultImage;
        }
    }
}
