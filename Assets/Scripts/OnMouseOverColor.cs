using UnityEngine;
using UnityEngine.UI;

public class OnMouseOverColor : MonoBehaviour
{
    Color m_MouseOverColor = Color.magenta;
    Color m_OriginalColor = Color.yellow;

    public Text skinsText;
    public Text optionsText;

    public void SkinsHover()
    {
        skinsText.color = m_MouseOverColor;
    }

    public void SkinsHoverExit()
    {
        skinsText.color = m_OriginalColor;
    }

    public void OptionsHover()
    {
        optionsText.color = m_MouseOverColor;
    }

    public void OptionsHoverExit()
    {
        optionsText.color = m_OriginalColor;
    }
}