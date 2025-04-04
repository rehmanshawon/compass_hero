using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Warzoom.MapShot
{
    public class MiniDisplay : MonoBehaviour
    {
        public static event Action<int> MiniDisplayClicked;
        public static event Action<int> MiniDisplayClose;
        [SerializeField] private Image _screenShotImage;
        [SerializeField] private int _id;
        
        
        public void SetImage(Sprite sprite)
        {
            _screenShotImage.sprite = sprite;
        }
        
        public void Show(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void OnSelectPicture()
        {
            MiniDisplayClicked?.Invoke(_id);
        }

        public void CloseMe()
        {
            Show(false);
            transform.SetAsLastSibling();
            MiniDisplayClose?.Invoke(_id);
        }

        public void SetId(int id)
        {
            _id = id;
        }

        public Sprite GetSprite()
        {
            return _screenShotImage.sprite;
        }

        public int GetId()
        {
            return _id;
        }
    }
}