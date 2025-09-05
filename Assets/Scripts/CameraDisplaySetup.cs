using UnityEngine;
using UnityEngine.UI;

public class CameraDisplaySetup : MonoBehaviour
{
    [Header("Camera Display Settings")]
    [Tooltip("Width of the camera display")]
    public int displayWidth = 640;
    
    [Tooltip("Height of the camera display")]
    public int displayHeight = 480;
    
    [Tooltip("Position of the camera display (0,0 = top-left, 1,1 = bottom-right)")]
    public Vector2 displayPosition = new Vector2(0.02f, 0.02f);
    
    [Tooltip("Automatically create UI Canvas and RawImage for camera display")]
    public bool autoCreateUI = true;
    
    private Canvas uiCanvas;
    private RawImage cameraDisplay;
    
    void Start()
    {
        if (autoCreateUI)
        {
            CreateCameraDisplayUI();
        }
    }
    
    public void CreateCameraDisplayUI()
    {
        // Create Canvas if it doesn't exist
        uiCanvas = FindObjectOfType<Canvas>();
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("Camera UI Canvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 10;
            
            // Add CanvasScaler for proper scaling
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster for UI interactions
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("Created UI Canvas for camera display");
        }
        
        // Create RawImage for camera display
        GameObject imageObj = new GameObject("Camera Display");
        imageObj.transform.SetParent(uiCanvas.transform, false);
        
        cameraDisplay = imageObj.AddComponent<RawImage>();
        cameraDisplay.color = Color.white;
        
        // Set up RectTransform
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        
        // Set anchor to top-left
        rectTransform.anchorMin = displayPosition;
        rectTransform.anchorMax = displayPosition;
        rectTransform.pivot = new Vector2(0, 1); // Top-left pivot
        
        // Set size
        rectTransform.sizeDelta = new Vector2(displayWidth, displayHeight);
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Add a background panel
        GameObject backgroundObj = new GameObject("Camera Background");
        backgroundObj.transform.SetParent(imageObj.transform, false);
        backgroundObj.transform.SetAsFirstSibling();
        
        Image background = backgroundObj.AddComponent<Image>();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(-5, -5); // Slight padding
        bgRect.offsetMax = new Vector2(5, 5);
        
        Debug.Log($"Created camera display UI: {displayWidth}x{displayHeight} at position {displayPosition}");
        
        // Try to connect with RosImageSubscriber
        RosImageSubscriber imageSubscriber = FindObjectOfType<RosImageSubscriber>();
        if (imageSubscriber != null)
        {
            imageSubscriber.displayImage = cameraDisplay;
            Debug.Log("Connected RosImageSubscriber to camera display");
        }
        else
        {
            Debug.LogWarning("RosImageSubscriber not found. Please ensure it's in the scene and assign the RawImage manually.");
        }
    }
    
    public RawImage GetCameraDisplay()
    {
        return cameraDisplay;
    }
    
    // Method to resize display at runtime
    public void ResizeDisplay(int width, int height)
    {
        if (cameraDisplay != null)
        {
            RectTransform rectTransform = cameraDisplay.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height);
            displayWidth = width;
            displayHeight = height;
        }
    }
    
    // Method to move display at runtime
    public void MoveDisplay(Vector2 newPosition)
    {
        if (cameraDisplay != null)
        {
            RectTransform rectTransform = cameraDisplay.GetComponent<RectTransform>();
            rectTransform.anchorMin = newPosition;
            rectTransform.anchorMax = newPosition;
            displayPosition = newPosition;
        }
    }
}
