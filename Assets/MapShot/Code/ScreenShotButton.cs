using UnityEngine;
using UnityEngine.UI;

namespace Warzoom.MapShot
{
    public class ScreenShotButton : MonoBehaviour
    {
        [SerializeField] private Button _btn;

        public Button Btn => _btn;

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
        
    }
}