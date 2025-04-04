using UnityEngine;

public class WebGLVisibilityHandler : MonoBehaviour
{
    [SerializeField] private bool isFocused;
    [SerializeField] private float deltaTime;
    [SerializeField] private int fps;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (isFocused)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        isFocused = hasFocus;
    }
}