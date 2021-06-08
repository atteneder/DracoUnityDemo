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
