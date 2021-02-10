// Copyright (c) 2019 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Networking;
using Unity.Collections;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Utils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
#if DRACO
using Draco;
#endif

public class Benchmark : MonoBehaviour
{
    enum MeshType {
        Draco,
        Corto
    }

    [SerializeField]
    MeshType meshType;

    [SerializeField]
    private string filePath = null;

    [SerializeField]
    private int count = 10;

    [SerializeField]
    MeshFilter prefab = null;

    NativeArray<byte> data;

    [SerializeField]
    float spread = .5f;

    [SerializeField]
    float step = .001f;
    float distance = 10;
    float aspectRatio = 1.5f;

    StopWatch stopwatch;
    // Start is called before the first frame update
    IEnumerator Start() {
        stopwatch = gameObject.AddComponent<StopWatch>();
        var url = GetStreamingAssetsUrl(filePath);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        if(!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogErrorFormat("Error loading {0}: {1}",url,webRequest.error);
            yield break;
        }

        // data = webRequest.downloadHandler.data;
        data = new NativeArray<byte>(webRequest.downloadHandler.data,Allocator.Persistent);

        if (filePath.EndsWith(".crt")) {
            meshType = MeshType.Corto;
        }
        // LoadBatch();
    }

    // Update is called once per frame
    void Update() {
        if(data!=null) {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                LoadBatch(1);
            } else
            if (Input.GetKeyDown(KeyCode.Space)
                || (Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began)) {
                LoadBatch(count);
            }
        }
    }

    async void LoadBatch(int quantity) {

        stopwatch.StartTime();

        switch (meshType) {
#if DRACO
            case MeshType.Draco:
                await LoadBatchDraco(quantity);
                break;
#endif
            default:
                Debug.LogError("Unsupported mesh type. Install missing packages!");
                stopwatch.StopTime();
                return;
        }
        await Task.Yield();
        stopwatch.StopTime();
        Debug.Log($"Loaded {filePath} {quantity} times in {stopwatch.GetTextReport()}");
    }
    
#if DRACO
    async Task LoadBatchDraco(int quantity) {

        var tasks = new List<Task<Mesh>>(quantity);

        for (int i = 0; i < quantity; i++)
        {
            DracoMeshLoader dracoLoader = new DracoMeshLoader();
            var task = dracoLoader.ConvertDracoMeshToUnity(data);
            tasks.Add(task);
        }

        while (tasks.Count > 0) {
            var task = await Task.WhenAny(tasks);
            tasks.Remove(task);
            var mesh = await task;
            if (mesh == null) {
                Debug.LogError("Nope");
            }
            else {
                ApplyMesh(mesh);
            }
        }
    }
#endif

        
        await Task.Yield();

        stopwatch.StopTime();
        Debug.Log($"Loaded {filePath} {quantity} times in {stopwatch.GetTextReport()}");
    }

    void OnMeshesLoaded( Mesh mesh ) {
        ApplyMesh(mesh);
    }

    void ApplyMesh(Mesh mesh) {
        Profiler.BeginSample("ApplyMesh");
        if (mesh==null) return;
        var b = Object.Instantiate<MeshFilter>(prefab);
        b.transform.position = new Vector3(
            (Random.value-.5f)* spread * aspectRatio,
            (Random.value-.5f)* spread,
            distance
            );
        var m = b.GetComponent<Renderer>().material;
        m.color = Color.HSVToRGB(Random.value, .5f, 1.0f );
        distance-=step;
        b.mesh = mesh;
        Profiler.EndSample();
    }

    /// <summary>
    /// Converts a relative sub path within StreamingAssets
    /// and creates an absolute URI from it. Useful for loading
    /// via UnityWebRequests.
    /// </summary>
    /// <param name="subPath">Path, relative to StreamingAssets. Example: path/to/file.basis</param>
    /// <returns>Platform independent URI that can be loaded via UnityWebRequest</returns>
    public static string GetStreamingAssetsUrl( string subPath ) {

        var path = Path.Combine(Application.streamingAssetsPath,subPath);

        #if LOCAL_LOADING
        path = string.Format( "file://{0}", path );
        #endif

        return path;
    }

    void OnDestroy() {
        data.Dispose();
    }
}
