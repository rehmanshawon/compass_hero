using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;

namespace Warzoom.MapShot
{
    [RequireComponent(typeof(MapShotAvailabilityChecker))]
    public class ScreenshotManager : MonoBehaviour
    {
        [Header("References:")]
        [SerializeField] private List<MiniDisplay> _miniDisplaysCollection;
        [Tooltip("Add the Ui objects that will hide to left only the map on screen after the screenshot is taken the objects on the list will be visible again")]
        [SerializeField] private GameObject[] _objectsToHideCollection;
        [SerializeField] private ScreenShotButton _screenShotButton;
        [SerializeField] private BigDisplay _bigDisplay;
        [SerializeField] private Transform _endPointBigPic;
        [SerializeField] private Camera _targetCamera; 
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private TilemapRenderer _tilemapRenderer;
        [Space]
        [Header("Setup")]
        [SerializeField] private int _maxScreenshots;
        
        
      
        //private Queue<Sprite> _spriteQueue;
        private MapShotAvailabilityChecker _availabilityChecker;
        private Dictionary<int, MiniDisplay> _miniDisplaysDictionary;
        private Dictionary<int, Sprite> _screenShotDictionary;
        private Queue<MiniDisplay> _miniDisplaysAvailableQueue;

        [SerializeField] private GameObject miniDisplays;
        
        #region UNITY CYCLE

        private void Awake()
        {
            _availabilityChecker = GetComponent<MapShotAvailabilityChecker>();
            InitCollections();
            
        }

        private void OnEnable()
        {
            MiniDisplay.MiniDisplayClicked += OnMiniDisplayClick;
            MiniDisplay.MiniDisplayClose += OnCloseMiniPic;
            BigDisplay.BigDisplayClickEvent += OnClickBigDisplay;
            
        }

        private void OnClickBigDisplay()
        {
            var currentID = _bigDisplay.GetId();
            var miniDisplay = _miniDisplaysDictionary[currentID];
            var positionEnd = miniDisplay.transform.position;

            var sequense = DOTween.Sequence();
            sequense.Append(_bigDisplay.transform.DOMove(positionEnd, 0.15f))
                .Join(_bigDisplay.transform.DOScale(0,0.15f))
                .OnComplete(() =>
                {
                    _bigDisplay.Show(false);
                    //miniDisplays.SetActive(false);
                });
        }

        private void OnDisable()
        {
            MiniDisplay.MiniDisplayClicked -= OnMiniDisplayClick;
            MiniDisplay.MiniDisplayClose -= OnCloseMiniPic;
            BigDisplay.BigDisplayClickEvent -= OnClickBigDisplay;

        }

        private void Start()
        {
            UpdateButtonUI();
        }

        #endregion
        
        #region Capture Stuff

        [ContextMenu("Take ScreenShot")]
        public void CaptureScreenshot()
        {
            miniDisplays.SetActive(true);

            if (_maxScreenshots == _screenShotDictionary.Count) return;
            _targetCamera.enabled = true;
            _mainCamera.enabled = false;
            ShowObjects(false);            
            StartCoroutine(Co_TakeShot());
        }

        public void PicturesBtnClick()
        {
            if (miniDisplays.activeSelf)
            {
                miniDisplays.SetActive(false);
            }
            else
            {
                miniDisplays.SetActive(true);
            }
        }

        IEnumerator Co_TakeShot()
        {
            
            yield return new WaitForEndOfFrame();
    
            // Get the bounds of the TilemapRenderer in world coordinates
            Bounds tilemapBounds = _tilemapRenderer.bounds;
            Vector3 targetPosition = new Vector3(tilemapBounds.center.x, tilemapBounds.center.y, _targetCamera.transform.position.z);
            _targetCamera.transform.position = targetPosition;
            // Convert the world coordinates to screen coordinates
            Vector3 screenMin = _targetCamera.WorldToScreenPoint(tilemapBounds.min);
            Vector3 screenMax = _targetCamera.WorldToScreenPoint(tilemapBounds.max);

            // Calculate the width and height of the TilemapRenderer in screen space
            int screenWidth = Mathf.CeilToInt(screenMax.x - screenMin.x);
            int screenHeight = Mathf.CeilToInt(screenMax.y - screenMin.y);

            // Create a RenderTexture with the calculated size
            RenderTexture renderTexture = new RenderTexture(screenWidth, screenHeight, 24);
            _targetCamera.targetTexture = renderTexture;

            // Render the camera to the RenderTexture
            _targetCamera.Render();

            // Create a new Texture2D to read the pixels from the RenderTexture
            Texture2D screenshotTexture = new Texture2D(screenWidth, screenHeight, TextureFormat.RGBA32, false);

            // Read the pixels from the RenderTexture to the Texture2D
            RenderTexture.active = renderTexture;
            screenshotTexture.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
            screenshotTexture.Apply();

            // Reset the target camera's targetTexture to null
            _targetCamera.targetTexture = null;

            // Disable the target camera after taking the screenshot
            _targetCamera.enabled = false;
            _mainCamera.enabled = true;

            // Create a Sprite from the Texture2D
            Sprite screenshotSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, screenWidth, screenHeight), new Vector2(0.5f, 0.5f));

            // Show the mini displays and objects
            ShowMiniDisplays(screenshotSprite);
            ShowObjects(true);
        }

        #endregion

        #region UI Stuff
        
        
        private void ShowMiniDisplays(Sprite sprite)
        {
            if (_miniDisplaysAvailableQueue.Count == 0)
            {
                Debug.LogWarning("Not MiniDisplay Avaialble to put the picture");
                return;
            }
            var miniDisplayAvailable = _miniDisplaysAvailableQueue.Dequeue();
            var miniDisplayId = miniDisplayAvailable.GetId();
            _screenShotDictionary.Add(miniDisplayId, sprite);
            miniDisplayAvailable.SetImage(sprite);
            miniDisplayAvailable.Show(true);
        }
        
        
        
        /// <summary>
        /// Show or Hide the visual elements that be over the map like the UI buttons for example
        /// </summary>
        /// <param name="value">True - Show them. False - Hide them</param>
        private void ShowObjects(bool value)
        {
            foreach (var item in _objectsToHideCollection)
            {
                item.SetActive(value);
            }
        }

        private void OnMiniDisplayClick(int id)
        {
            var sprite = _screenShotDictionary[id];
          
           
            if (!_bigDisplay.isActiveAndEnabled)
            {
                var miniDisplay = _miniDisplaysDictionary[id];
                var initPosition = miniDisplay.transform.position;
                var positionToMove = _endPointBigPic.position;
                _bigDisplay.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.Linear);
                
                _bigDisplay.transform.DOMove(positionToMove, 0.25f).From(initPosition).SetEase(Ease.Linear);
            }
            _bigDisplay.SetCurrentSpriteId(id);
            _bigDisplay.SetTexture(sprite);
            _bigDisplay.Show(true);
        }
        
        
        private void OnCloseMiniPic(int id)
        {
            var sprite = _screenShotDictionary[id];

            if (_bigDisplay.IsOn())
            {
                if (_bigDisplay.GetId() == id)
                {
                    OnClickBigDisplay();
                }
            }
            
            _screenShotDictionary.Remove(id);
            var miniDisplay = _miniDisplaysDictionary[id];
            _miniDisplaysAvailableQueue.Enqueue(miniDisplay);
            if (sprite == null)
            {
                return;
            }
            CleanTexture(sprite.texture);
            
        }
        
        #endregion

        #region Textures Stuff

        /// <summary>
        /// Destroy the texture for release memory
        /// </summary>
        /// <param name="itemToRemove"></param>
        private void CleanTexture(Texture2D itemToRemove)
        {
            Destroy(itemToRemove);
        }
        
        #endregion

        #region Texture collections Stuff

        /// <summary>
        /// Initialize the collections
        /// </summary>
        private void InitCollections()
        {
            _miniDisplaysAvailableQueue = new Queue<MiniDisplay>();
            _miniDisplaysDictionary = new Dictionary<int, MiniDisplay>();
            _screenShotDictionary = new Dictionary<int, Sprite>();
            
            for (int i = 0; i < _miniDisplaysCollection.Count; i++)
            {
                var id = i;
                var miniDisplay = _miniDisplaysCollection[id];
                miniDisplay.SetId(id);
                _miniDisplaysDictionary.Add(id,miniDisplay);
                _miniDisplaysAvailableQueue.Enqueue(miniDisplay);
            }
        }

        #endregion
        
        #region Feature Handler
        
        private void UpdateButtonUI()
        {
            var show = _availabilityChecker.IsAvailable();
            
            //_screenShotButton.Show(show);
            _screenShotButton.Btn.onClick.AddListener(CaptureScreenshot);
        }

        #endregion                
    }
}
