#if !(UNITY_ANDROID || UNITY_WEBGL) || UNITY_EDITOR
#define LOCAL_LOADING
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Networking;
using Unity.Collections;
using System.IO;

public class Benchmark : MonoBehaviour
{
    [SerializeField]
    private string filePath;

    [SerializeField]
    private int count = 10;

    [SerializeField]
    MeshFilter prefab;

    NativeArray<byte> data;

    [SerializeField]
    float spread = .5f;

    [SerializeField]
    float step = .001f;
    float distance = 10;
    float aspectRatio = 1.5f;

    // Start is called before the first frame update
    IEnumerator Start() {
        var url = GetStreamingAssetsUrl(filePath);
        var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();
        if(!string.IsNullOrEmpty(webRequest.error)) {
            Debug.LogErrorFormat("Error loading {0}: {1}",url,webRequest.error);
            yield break;
        }

        // data = webRequest.downloadHandler.data;
        data = new NativeArray<byte>(webRequest.downloadHandler.data,Allocator.Persistent);

        // LoadBatch();
    }

    // Update is called once per frame
    void Update() {
        if(data!=null &&
        (
            Input.GetKeyDown(KeyCode.Space)
            || (Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began)
        )) {
            LoadBatch();
        }
    }

    void LoadBatch() {
        Profiler.BeginSample("LoadBatch");
        for (int i = 0; i < count; i++)
        {
            Profiler.BeginSample("DecodeMesh");
            DracoMeshLoader dracoLoader = new DracoMeshLoader();
            dracoLoader.onMeshesLoaded += OnMeshesLoaded;
            StartCoroutine(dracoLoader.DecodeMesh(data));
            Profiler.EndSample();
        }
        Profiler.EndSample();
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
