using System.IO;
using UnityEngine;
using Draco;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DracoDemo : MonoBehaviour {
    
    public string filePath;

    async void Start() {
        
        // Load file into memory
        var fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
        var data = File.ReadAllBytes(fullPath);
        
        // Convert data to Unity mesh
        var draco = new DracoMeshLoader();
        // Async decoding has to start on the main thread and spawns multiple C# jobs.
        var mesh = await draco.ConvertDracoMeshToUnity(data);
        
        if (mesh != null) {
            // Use the resulting mesh
            GetComponent<MeshFilter>().mesh= mesh;
        }
    }
}
