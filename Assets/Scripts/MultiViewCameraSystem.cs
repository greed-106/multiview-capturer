using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class CameraRingConfiguration
{
    [Tooltip("环名称（用于文件名）")]
    public string ringName = "Ring";
    
    [Tooltip("相机数量")]
    public int cameraCount = 8;
    
    [Tooltip("距离目标物体的半径")]
    public float radius = 5f;
    
    [Tooltip("相对于目标的高度")]
    public float height = 5f;
    
    [Tooltip("俯仰角度（-90到90）")]
    public float pitchAngle = 0f;
    
    [Tooltip("水平偏移角度")]
    [Range(0f, 360f)]
    public float horizontalOffset = 0f;
    
    [Tooltip("是否启用此环")]
    public bool enabled = true;
}

public class MultiViewCameraSystem : MonoBehaviour
{
    [Header("目标物体")]
    public Transform targetObject;

    [Header("多环摄像机配置")]
    [Tooltip("添加不同配置的摄像机环")]
    public List<CameraRingConfiguration> cameraRings = new List<CameraRingConfiguration>();

    [Header("摄像机参数")]
    [Tooltip("基础视野角度")]
    public float fieldOfView = 60f;
    [Tooltip("近裁剪面距离")]
    public float nearClipPlane = 0.1f;
    [Tooltip("远裁剪面距离")]
    public float farClipPlane = 100f;

    [Header("渲染设置")]
    public int imageWidth = 1024;
    public int imageHeight = 1024;

    [Header("保存设置")]
    public string savePath = "D:/code/datasets/pngs";
    public string filePrefix = "longdress";

    private Camera tempCamera;
    private RenderTexture renderTexture;

    void Start()
    {
        if (targetObject == null)
            targetObject = transform;

        SetupCamera();
    }

    void SetupCamera()
    {
        // 创建临时摄像机
        GameObject cameraGO = new GameObject("TempMultiViewCamera");
        cameraGO.transform.SetParent(transform);

        tempCamera = cameraGO.AddComponent<Camera>();
        tempCamera.fieldOfView = fieldOfView;
        tempCamera.nearClipPlane = nearClipPlane;
        tempCamera.farClipPlane = farClipPlane;
        tempCamera.clearFlags = CameraClearFlags.Skybox;
        tempCamera.allowHDR = true;
        tempCamera.useOcclusionCulling = true;
        tempCamera.cullingMask = -1;
        tempCamera.enabled = false;

        // 创建渲染纹理
        renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        tempCamera.targetTexture = renderTexture;
    }

    /// <summary>
    /// 同步拍摄所有配置环的视角
    /// </summary>
    public void CaptureMultiView(string infix)
    {
        if (tempCamera == null || renderTexture == null)
        {
            Debug.LogError("摄像机系统未正确初始化");
            return;
        }

        // 确保保存目录存在
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        int totalShots = 0;
        
        foreach (var ring in cameraRings)
        {
            if (!ring.enabled) continue;
            
            for (int i = 0; i < ring.cameraCount; i++)
            {
                // 计算水平角度，包含偏移量
                float yawAngle = (360f / ring.cameraCount) * i + ring.horizontalOffset;
                PositionCamera(ring, yawAngle);
                tempCamera.Render();

                // 生成文件名（包含环标识）
                string filename = $"{filePrefix}_{infix}_{ring.ringName}_{i:D2}.png";
                SaveCurrentView(filename);
                
                totalShots++;
            }
        }

        Debug.Log($"多环拍摄完成，共拍摄 {totalShots} 张图片到: {savePath}");
    }

    /// <summary>
    /// 异步拍摄所有配置环的视角
    /// </summary>
    public void CaptureMultiViewAsync()
    {
        StartCoroutine(CaptureMultiViewCoroutine());
    }

    IEnumerator CaptureMultiViewCoroutine()
    {
        if (tempCamera == null || renderTexture == null)
        {
            Debug.LogError("摄像机系统未正确初始化");
            yield break;
        }

        // 确保保存目录存在
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        int totalShots = 0;

        foreach (var ring in cameraRings)
        {
            if (!ring.enabled) continue;
            
            for (int i = 0; i < ring.cameraCount; i++)
            {
                float yawAngle = (360f / ring.cameraCount) * i + ring.horizontalOffset;
                PositionCamera(ring, yawAngle);
                tempCamera.Render();

                // 生成文件名（包含环标识和时间戳）
                string filename = $"{filePrefix}_{timestamp}_{ring.ringName}_{i:D2}.png";
                SaveCurrentView(filename);
                
                totalShots++;
                yield return null; // 每帧处理一个视角
            }
        }

        Debug.Log($"异步多环拍摄完成，共拍摄 {totalShots} 张图片到: {savePath}");
    }

    /// <summary>
    /// 根据环配置和水平角度放置摄像机
    /// </summary>
    void PositionCamera(CameraRingConfiguration ring, float yawAngleDegrees)
    {
        // 转换为弧度
        float yawRad = yawAngleDegrees * Mathf.Deg2Rad;

        Vector3 targetPos = targetObject.position;
        float x = targetPos.x + ring.radius * Mathf.Cos(yawRad);
        float z = targetPos.z + ring.radius * Mathf.Sin(yawRad);
        float y = targetPos.y + ring.height; // 使用环的高度配置
        Vector3 cameraPosition = new Vector3(x, y, z);

        tempCamera.transform.position = cameraPosition;

        Vector3 directionToTarget = (targetPos - cameraPosition).normalized;
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

        // 使用环的俯仰角度配置
        float pitchRad = ring.pitchAngle * Mathf.Deg2Rad;
        Vector3 finalDirection = new Vector3(
            horizontalDirection.x,
            Mathf.Sin(pitchRad),
            horizontalDirection.z
        ).normalized;

        tempCamera.transform.rotation = Quaternion.LookRotation(finalDirection);
    }

    void SaveCurrentView(string filename)
    {
        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        texture.Apply();

        RenderTexture.active = null;

        byte[] bytes = texture.EncodeToPNG();
        string fullPath = Path.Combine(savePath, filename);
        File.WriteAllBytes(fullPath, bytes);

        DestroyImmediate(texture);
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }

        if (tempCamera != null)
            DestroyImmediate(tempCamera.gameObject);
    }
}