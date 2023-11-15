// Copyright 2017 The Draco Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS || UNITY_ANDROID || UNITY_WSA || PLATFORM_LUMIN
#define DRACO_PLATFORM_SUPPORTED
#endif

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
#if DRACO_PLATFORM_SUPPORTED
using Draco;
#endif
using Unity.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DracoDemo : MonoBehaviour
{

    public string filePath;

#if DRACO_PLATFORM_SUPPORTED
    async void Start() {

        var data = await LoadData(filePath);
        if (data == null) return;

        // Convert data to Unity mesh
        var draco = new DracoMeshLoader();
        // Async decoding has to start on the main thread and spawns multiple C# jobs.
        var mesh = await draco.ConvertDracoMeshToUnity(data);

        if (mesh != null)
        {
            FixPointCloudIndices(mesh);

            // Use the resulting mesh
            GetComponent<MeshFilter>().mesh= mesh;
        }
    }

    /// <summary>
    /// Creates progressive indices for points meshes.
    /// TODO: This should be resolved by DracoUnity at some point
    /// </summary>
    /// <seealso href="https://github.com/atteneder/DracoUnity/issues/64"/>
    /// <param name="mesh"></param>
    static void FixPointCloudIndices(Mesh mesh)
    {
        if (mesh.GetTopology(0) == MeshTopology.Points)
        {
            var indices = new NativeArray<int>(mesh.vertexCount, Allocator.Temp);
            for (var i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);
            indices.Dispose();
        }
    }

#pragma warning disable CS1998
    public static async Task<byte[]> LoadData(string filePath) {
#pragma warning restore CS1998
        // Load file into memory
        var fullPath = Path.Combine(Application.streamingAssetsPath, filePath);

        byte[] data = null;

#if UNITY_WEBGL && !UNITY_EDITOR
        // fullPath = $"file://{fullPath}";
        using var request = UnityWebRequest.Get(fullPath);
        request.SendWebRequest();
        while (!request.isDone) {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.Success) {
            data = request.downloadHandler.data;
        }
        else {
            Debug.LogError($"Download failed {request.result}: {request.error} (URI: {fullPath})");
        }

#else
#if NET_STANDARD_2_1
        data = await File.ReadAllBytesAsync(fullPath);
#else
        data = File.ReadAllBytes(fullPath);
#endif
#endif
        return data;
    }
#else // DRACO_PLATFORM_SUPPORTED
    void Start()
    {
        Debug.LogError("Draco is not supported on this platform.");
    }
#endif // DRACO_PLATFORM_SUPPORTED
}
