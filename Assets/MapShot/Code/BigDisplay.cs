using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Warzoom.MapShot
{
    public class BigDisplay : MonoBehaviour, IPointerClickHandler
    {
        public static event Action BigDisplayClickEvent;
        [SerializeField] private Image _image;
        private int _id;
        
        public void SetCurrentSpriteId(int id)
        {
            _id = id;
        }

        public int GetId() => _id;
        
        public void SetTexture(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            BigDisplayClickEvent?.Invoke();
            
        }

        public bool IsOn()
        {
            return gameObject.activeSelf;
        }
    }
}