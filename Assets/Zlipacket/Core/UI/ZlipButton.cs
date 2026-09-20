using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zlipacket.Core.Audio;
using Zlipacket.Core.Scene;
using Zlipacket.Core.Tools.Utilities;

namespace Zlipacket.Core.UI
{
    public class ZlipButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        
        [Header("Curser Hover Sprite")]
        [SerializeField] public Texture2D curserSprite;
        [SerializeField] public Texture2D hoverSprite;
        
        [Header("Hover Scale")]
        public bool useHoverScale = false;
        public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
        public float scaleSpeed = 5f;
        
        [Header("Others")]
        public bool onlyClickOnce = false;
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            if (button != null)
            {
                if (onlyClickOnce)
                    button.onClick.AddListener(() => button.onClick.RemoveAllListeners());
            }
        }
        
        public void ChangeToScene(string sceneName)
        {
            SceneManager.Instance.LoadScene(sceneName);
        }

        public void OverlayToScene(string sceneName)
        {
            SceneManager.Instance.LoadSceneAdditive(sceneName);
        }

        public void UnOverlayToScene(string sceneName)
        {
            SceneManager.Instance.UnloadScene(sceneName);
        }

        public void Pause()
        {
            PauseManager.Instance.Pause();
        }

        public void Resume()
        {
            PauseManager.Instance.Resume();
        }
        
        public void Quit()
        {
            Application.Quit();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverSprite != null)
            {
                Vector2 cursorHotSpot = new Vector2(hoverSprite.width / 2, hoverSprite.height / 2);
                Cursor.SetCursor(hoverSprite, cursorHotSpot, CursorMode.Auto);
            }

            if (useHoverScale)
            {
                Scale(hoverScale);
            }
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (curserSprite != null)
            {
                Vector2 cursorHotSpot = new Vector2(curserSprite.width / 2, curserSprite.height / 2);
                Cursor.SetCursor(curserSprite, cursorHotSpot, CursorMode.Auto);
            }
            
            if (useHoverScale)
            {
                Scale(Vector3.one);
            }
        }
        
        private Coroutine co_Scale;
        public bool isScaling => co_Scale != null;

        public void Scale(Vector3 targetScale)
        {
            if (isScaling)
                StopCoroutine(co_Scale);
            
            co_Scale = StartCoroutine(Scaling(targetScale));
        }

        public IEnumerator Scaling(Vector3 targetScale)
        {
            while (transform.localScale != targetScale)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, scaleSpeed * Time.unscaledDeltaTime);
                
                yield return null;
            }
            
            co_Scale = null;
        }
    }
}