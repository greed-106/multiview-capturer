using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class FileTools
{
    /// <summary>
    /// 获取指定目录下特定后缀的文件路径队列（自然排序）
    /// </summary>
    /// <param name="folderPath">目标文件夹路径</param>
    /// <param name="extension">文件后缀（如".png"，不带点则自动补全）</param>
    /// <returns>自然排序后的完整文件路径队列</returns>
    public static Queue<string> GetFilesByExtension(string folderPath, string extension)
    {
        // 处理后缀格式
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        return GetSortedFiles(folderPath,
            f => Path.GetExtension(f).Equals(extension, System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取指定目录下特定前缀的文件路径队列（自然排序）
    /// </summary>
    /// <param name="folderPath">目标文件夹路径</param>
    /// <param name="prefix">文件前缀（区分大小写）</param>
    /// <returns>自然排序后的完整文件路径队列</returns>
    public static Queue<string> GetFilesByPrefix(string folderPath, string prefix)
    {
        return GetSortedFiles(folderPath,
            f => Path.GetFileName(f).StartsWith(prefix, System.StringComparison.Ordinal));
    }

    /// <summary>
    /// 获取指定目录下同时匹配前缀和后缀的文件路径队列（自然排序）
    /// </summary>
    /// <param name="folderPath">目标文件夹路径</param>
    /// <param name="prefix">文件前缀</param>
    /// <param name="extension">文件后缀</param>
    /// <returns>自然排序后的完整文件路径队列</returns>
    public static Queue<string> GetFilesByPrefixAndExtension(string folderPath, string prefix, string extension)
    {
        // 处理后缀格式
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        return GetSortedFiles(folderPath,
            f => Path.GetFileName(f).StartsWith(prefix, System.StringComparison.Ordinal) &&
                 Path.GetExtension(f).Equals(extension, System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 核心方法：获取排序后的文件队列
    /// </summary>
    private static Queue<string> GetSortedFiles(string folderPath, System.Func<string, bool> filterCondition)
    {
        // 检查目录是否存在
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"目录不存在: {folderPath}");
            return new Queue<string>();
        }

        try
        {
            // 获取所有文件并筛选
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                                .Where(filterCondition);

            // 自然排序
            var sortedFiles = files.OrderBy(f => Path.GetFileName(f), new NaturalSortComparer());

            // 转换为队列
            Queue<string> fileQueue = new Queue<string>(sortedFiles);

            // 调试信息
            if (fileQueue.Count > 0)
            {
                Debug.Log($"找到 {fileQueue.Count} 个匹配文件\n" +
                         $"首文件: {Path.GetFileName(fileQueue.Peek())}\n" +
                         $"末文件: {Path.GetFileName(fileQueue.Last())}");
            }
            else
            {
                Debug.LogWarning($"未找到匹配文件: {folderPath}");
            }

            return fileQueue;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"文件搜索出错: {e.Message}");
            return new Queue<string>();
        }
    }

    /// <summary>
    /// 自然排序比较器
    /// </summary>
    private class NaturalSortComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            return CompareNatural(x, y);
        }

        private int CompareNatural(string strX, string strY)
        {
            int i = 0, j = 0;

            while (i < strX.Length && j < strY.Length)
            {
                if (char.IsDigit(strX[i]) && char.IsDigit(strY[j]))
                {
                    // 处理数字部分
                    int numX = 0, numY = 0;

                    while (i < strX.Length && char.IsDigit(strX[i]))
                    {
                        numX = numX * 10 + (strX[i] - '0');
                        i++;
                    }

                    while (j < strY.Length && char.IsDigit(strY[j]))
                    {
                        numY = numY * 10 + (strY[j] - '0');
                        j++;
                    }

                    if (numX != numY)
                        return numX.CompareTo(numY);
                }
                else
                {
                    // 非数字部分直接比较
                    int compareResult = strX[i].CompareTo(strY[j]);
                    if (compareResult != 0)
                        return compareResult;

                    i++;
                    j++;
                }
            }

            return strX.Length.CompareTo(strY.Length);
        }
    }
}