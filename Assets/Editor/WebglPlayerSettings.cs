using UnityEditor;
using UnityEngine;

public static class WebglPlayerSettings
{
    [MenuItem("Tools/Set WebGL Settings")]
    static void WebglSet()
    {
        PlayerSettings.WebGL.threadsSupport = true;
    }

    [MenuItem("Tools/Unset WebGL Settings")]
    static void WebglUnSet()
    {
        PlayerSettings.WebGL.threadsSupport = false;
    }
}
