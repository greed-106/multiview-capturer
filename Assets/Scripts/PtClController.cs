using UnityEngine;
using System.Collections.Generic;

public class PtClController : MonoBehaviour
{

    private Mesh mesh;
    private Material material;
    public MultiViewCameraSystem multiViewSystem;
    public string ptclFolderPath;
    private PtCl ptcl;
    private Queue<string> ptclPaths;
    private int frameCount = 0;

    void Start()
    {
        mesh = new Mesh();
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        material = new Material(Shader.Find("Shaders/VertexColor"));
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        ptcl = new PtCl();
        ptclPaths = FileTools.GetFilesByExtension(ptclFolderPath, ".ply");
    }


    void Update()
    {
        if(ptclPaths.Count > 0)
        {
            string path = ptclPaths.Dequeue();
            ptcl.Dispose();
            ptcl.LoadDataFromBinaryPly(path);
            ptcl.setMesh(mesh);

            multiViewSystem.CaptureMultiView(frameCount.ToString("D4"));
            frameCount++;
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 编辑器模式下停止运行
#else
            Application.Quit(); // 发布版本中退出应用
#endif
        }



    }

    void OnDestroy()
    {
        if (ptcl != null)
        {
            ptcl.Dispose();
        }
    }

}