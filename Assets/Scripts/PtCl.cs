using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Collections;
using UnityEngine;

public class PtCl : IDisposable
{
    public NativeArray<Vector3> geoData;
    public NativeArray<Color32> colorData;
    public int pointNumber = 0;

    public void Dispose()
    {
        if (geoData.IsCreated)
        {
            geoData.Dispose();
        }
        if (colorData.IsCreated)
        {
            colorData.Dispose();
        }
    }

    public PtCl() { }

    static private string ReadString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        byte b;
        while ((b = reader.ReadByte()) != '\n')
        {
            bytes.Add(b);
        }
        return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
    }

    public void LoadDataFromBinaryPly(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(file))
        {
            string line;
            while ((line = ReadString(reader)) != null)
            {
                if (line.Contains("element vertex"))
                {
                    pointNumber = int.Parse(line.Split(' ')[2]);
                    Debug.Log("Point number: " + pointNumber);
                }

                if (line.Contains("end_header") || line.Contains("end header"))
                {
                    break;
                }
            }

            geoData = new NativeArray<Vector3>(pointNumber, Allocator.Persistent);
            colorData = new NativeArray<Color32>(pointNumber, Allocator.Persistent);

            for (int i = 0; i < pointNumber; i++)
            {
                geoData[i] = new Vector3(
                    -reader.ReadInt16(),
                    reader.ReadInt16(),
                    reader.ReadInt16()
                );
                colorData[i] = new Color32(
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    255
                );
            }
        }
    }

    public void LoadDataFromBinaryPlyBytes(byte[] data)
    {
        using (var reader = new BinaryReader(new MemoryStream(data)))
        {
            string line;
            while ((line = ReadString(reader)) != null)
            {
                if (line.Contains("element vertex"))
                {
                    pointNumber = int.Parse(line.Split(' ')[2]);
                }
                if (line.Contains("end_header") || line.Contains("end header"))
                {
                    break;
                }
            }

            geoData = new NativeArray<Vector3>(pointNumber, Allocator.Persistent);
            colorData = new NativeArray<Color32>(pointNumber, Allocator.Persistent);

            for (int i = 0; i < pointNumber; i++)
            {
                geoData[i] = new Vector3(
                    -reader.ReadInt16(),
                    reader.ReadInt16(),
                    reader.ReadInt16()
                );

                colorData[i] = new Color32(
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    255
                );
            }
        }
    }

    public void setMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.SetVertices(geoData);
        mesh.SetColors(colorData);
        mesh.SetIndexBufferParams(pointNumber, UnityEngine.Rendering.IndexFormat.UInt32);
        mesh.SetIndices(Enumerable.Range(0, pointNumber).ToArray<int>(), MeshTopology.Points, 0);
    }
}
