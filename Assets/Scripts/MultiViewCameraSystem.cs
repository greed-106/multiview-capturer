using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MultiViewCameraSystem : MonoBehaviour
{
    [Header("目标物体")]
    public Transform targetObject;

    [Header("摄像机参数")]
    public int cameraCount = 8;
    public float radius = 8f;
    public float height = 8f;
    public float pitchAngle = 0f; // 俯仰角度（-90到90，负值向下看，正值向上看）

    [Header("渲染设置")]
    public int imageWidth = 1024;
    public int imageHeight = 1024;
    public float fieldOfView = 60f;

    [Header("保存设置")]
    public string savePath = "D:/code/datasets/pngs"; // 指定保存路径
    public string filePrefix = "view"; // 文件名前缀

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
        // 创建临时摄像机用于拍摄
        GameObject cameraGO = new GameObject("TempMultiViewCamera");
        cameraGO.transform.SetParent(transform);

        tempCamera = cameraGO.AddComponent<Camera>();
        tempCamera.fieldOfView = fieldOfView;
        tempCamera.nearClipPlane = 0.1f;
        tempCamera.farClipPlane = 100f;
        //tempCamera.backgroundColor = Color.clear;
        //tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.clearFlags = CameraClearFlags.Skybox; // 使用场景的天空盒
        tempCamera.allowHDR = true; // 允许高动态范围
        tempCamera.useOcclusionCulling = true; // 启用遮挡剔除

        // 设置渲染层级为Everything
        tempCamera.cullingMask = -1; // 所有层

        tempCamera.enabled = false; // 默认禁用

        // 创建渲染纹理
        renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        tempCamera.targetTexture = renderTexture;
    }

    /// <summary>
    /// 外部调用的拍摄方法 - 同步拍摄所有视角
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

        for (int i = 0; i < cameraCount; i++)
        {
            // 计算摄像机位置和旋转
            float yawAngle = (360f / cameraCount) * i;
            PositionCamera(yawAngle);

            // 渲染当前视角
            tempCamera.Render();

            // 保存图片
            string filename = $"{filePrefix}_{infix}_{i:D2}.png";
            SaveCurrentView(filename);
        }

        Debug.Log($"多视角拍摄完成，已保存 {cameraCount} 张图片到: {savePath}");
    }

    /// <summary>
    /// 外部调用的拍摄方法 - 异步拍摄所有视角
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

        for (int i = 0; i < cameraCount; i++)
        {
            // 计算摄像机位置和旋转
            float yawAngle = (360f / cameraCount) * i;
            PositionCamera(yawAngle);

            // 渲染当前视角
            tempCamera.Render();

            // 保存图片
            string filename = $"{filePrefix}_{timestamp}_{i:D2}.png";
            SaveCurrentView(filename);

            yield return null; // 每帧处理一个视角
        }

        Debug.Log($"多视角拍摄完成，已保存 {cameraCount} 张图片到: {savePath}");
    }

    void PositionCamera(float yawAngleDegrees)
    {
        // 将角度转换为弧度
        float yawRad = yawAngleDegrees * Mathf.Deg2Rad;

        // 计算摄像机位置（高度固定，只有水平圆周运动）
        Vector3 targetPos = targetObject.position;

        float x = targetPos.x + radius * Mathf.Cos(yawRad);
        float z = targetPos.z + radius * Mathf.Sin(yawRad);
        float y = targetPos.y + height; // 高度固定

        Vector3 cameraPosition = new Vector3(x, y, z);

        // 设置摄像机位置
        tempCamera.transform.position = cameraPosition;

        // 手动计算朝向（包含俯仰角）
        Vector3 directionToTarget = (targetPos - cameraPosition).normalized;

        // 计算水平方向（忽略Y轴差异）
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

        // 应用俯仰角旋转
        float pitchRad = pitchAngle * Mathf.Deg2Rad;
        Vector3 finalDirection = new Vector3(
            horizontalDirection.x,
            Mathf.Sin(pitchRad), // 基于俯仰角计算Y方向分量
            horizontalDirection.z
        ).normalized;

        // 设置摄像机朝向
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