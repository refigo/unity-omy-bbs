using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosImage = RosMessageTypes.Sensor.ImageMsg;

public class RosImageSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    [Tooltip("ROS topic to subscribe to for camera images")]
    public string imageTopic = "/camera/camera/color/image_rect_raw";
    
    [Header("UI Settings")]
    [Tooltip("RawImage component to display the camera feed")]
    public RawImage displayImage;
    
    [Tooltip("Automatically find RawImage if not assigned")]
    public bool autoFindRawImage = true;
    
    [Header("Image Processing")]
    [Tooltip("Flip image vertically (common for camera feeds)")]
    public bool flipVertically = true;
    
    [Tooltip("Flip image horizontally")]
    public bool flipHorizontally = false;

    // Private variables
    private Texture2D cameraTexture;
    private bool isReceivingImages = false;
    
    void Start()
    {
        // Try to find RawImage component if not assigned
        if (displayImage == null && autoFindRawImage)
        {
            displayImage = FindObjectOfType<RawImage>();
            if (displayImage != null)
            {
                Debug.Log("Found RawImage component: " + displayImage.name);
            }
        }
        
        if (displayImage == null)
        {
            Debug.LogError("No RawImage component found! Please assign one in the inspector or ensure autoFindRawImage is enabled with a RawImage in the scene.");
            return;
        }
        
        // Subscribe to the ROS image topic
        ROSConnection.GetOrCreateInstance().Subscribe<RosImage>(imageTopic, OnImageReceived);
        Debug.Log($"Subscribed to ROS image topic: {imageTopic}");
    }
    
    void OnImageReceived(RosImage imageMessage)
    {
        Debug.Log("Received image test");
        try
        {
            // Convert ROS image to Unity texture
            ConvertRosImageToTexture(imageMessage);
            isReceivingImages = true;
            Debug.Log("Received image");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing image: {e.Message}");
        }
    }
    
    void ConvertRosImageToTexture(RosImage rosImage)
    {
        // Get image dimensions
        int width = (int)rosImage.width;
        int height = (int)rosImage.height;
        
        // Create or resize texture if needed
        if (cameraTexture == null || cameraTexture.width != width || cameraTexture.height != height)
        {
            if (cameraTexture != null)
            {
                DestroyImmediate(cameraTexture);
            }
            
            cameraTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            Debug.Log($"Created new texture: {width}x{height}, encoding: {rosImage.encoding}");
        }
        
        // Convert image data based on encoding
        Color32[] pixels = ConvertImageData(rosImage, width, height);
        
        // Apply flipping if needed
        if (flipVertically || flipHorizontally)
        {
            pixels = FlipImage(pixels, width, height, flipHorizontally, flipVertically);
        }
        
        // Apply pixels to texture
        cameraTexture.SetPixels32(pixels);
        cameraTexture.Apply();
        
        // Update the UI on the main thread
        if (displayImage != null)
        {
            displayImage.texture = cameraTexture;
        }
    }
    
    Color32[] ConvertImageData(RosImage rosImage, int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        byte[] imageData = rosImage.data;
        
        switch (rosImage.encoding.ToLower())
        {
            case "rgb8":
                ConvertRGB8(imageData, pixels, width, height);
                break;
                
            case "bgr8":
                ConvertBGR8(imageData, pixels, width, height);
                break;
                
            case "rgba8":
                ConvertRGBA8(imageData, pixels, width, height);
                break;
                
            case "bgra8":
                ConvertBGRA8(imageData, pixels, width, height);
                break;
                
            case "mono8":
            case "8uc1":
                ConvertMono8(imageData, pixels, width, height);
                break;
                
            default:
                Debug.LogWarning($"Unsupported image encoding: {rosImage.encoding}. Attempting RGB8 conversion.");
                ConvertRGB8(imageData, pixels, width, height);
                break;
        }
        
        return pixels;
    }
    
    void ConvertRGB8(byte[] data, Color32[] pixels, int width, int height)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int dataIndex = i * 3;
            if (dataIndex + 2 < data.Length)
            {
                pixels[i] = new Color32(data[dataIndex], data[dataIndex + 1], data[dataIndex + 2], 255);
            }
        }
    }
    
    void ConvertBGR8(byte[] data, Color32[] pixels, int width, int height)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int dataIndex = i * 3;
            if (dataIndex + 2 < data.Length)
            {
                pixels[i] = new Color32(data[dataIndex + 2], data[dataIndex + 1], data[dataIndex], 255);
            }
        }
    }
    
    void ConvertRGBA8(byte[] data, Color32[] pixels, int width, int height)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int dataIndex = i * 4;
            if (dataIndex + 3 < data.Length)
            {
                pixels[i] = new Color32(data[dataIndex], data[dataIndex + 1], data[dataIndex + 2], data[dataIndex + 3]);
            }
        }
    }
    
    void ConvertBGRA8(byte[] data, Color32[] pixels, int width, int height)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int dataIndex = i * 4;
            if (dataIndex + 3 < data.Length)
            {
                pixels[i] = new Color32(data[dataIndex + 2], data[dataIndex + 1], data[dataIndex], data[dataIndex + 3]);
            }
        }
    }
    
    void ConvertMono8(byte[] data, Color32[] pixels, int width, int height)
    {
        for (int i = 0; i < pixels.Length && i < data.Length; i++)
        {
            byte intensity = data[i];
            pixels[i] = new Color32(intensity, intensity, intensity, 255);
        }
    }
    
    Color32[] FlipImage(Color32[] original, int width, int height, bool flipHorizontal, bool flipVertical)
    {
        Color32[] flipped = new Color32[original.Length];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = y * width + x;
                
                int newX = flipHorizontal ? (width - 1 - x) : x;
                int newY = flipVertical ? (height - 1 - y) : y;
                int newIndex = newY * width + newX;
                
                flipped[newIndex] = original[originalIndex];
            }
        }
        
        return flipped;
    }
    
    void OnGUI()
    {
        // Display connection status
        if (!isReceivingImages)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(10, 10, 300, 20), $"Waiting for images on topic: {imageTopic}");
        }
        else
        {
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 10, 300, 20), $"Receiving images from: {imageTopic}");
        }
        
        // Display image info if available
        if (cameraTexture != null)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(10, 30, 300, 20), $"Image Size: {cameraTexture.width}x{cameraTexture.height}");
        }
    }
    
    void OnDestroy()
    {
        // Clean up texture
        if (cameraTexture != null)
        {
            DestroyImmediate(cameraTexture);
        }
    }
}
