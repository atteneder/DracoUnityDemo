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

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID || UNITY_WSA || PLATFORM_LUMIN
#define DRACO_PLATFORM_SUPPORTED
#endif

using System.IO;

using UnityEngine;
#if DRACO_PLATFORM_SUPPORTED
using Draco;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DracoDemoMeshData : MonoBehaviour
{

    public string filePath;

    public bool requireNormals;
    public bool requireTangents;

#if DRACO_PLATFORM_SUPPORTED
    async void Start() {

        var data = await DracoDemo.LoadData(filePath);
        if (data == null) return;

        // Convert data to Unity mesh
        var draco = new DracoMeshLoader();

        // Allocate single mesh data (you can/should bulk allocate multiple at once, if you're loading multiple draco meshes)
        var meshDataArray = Mesh.AllocateWritableMeshData(1);

        // Async decoding has to start on the main thread and spawns multiple C# jobs.
        var result = await draco.ConvertDracoMeshToUnity(
            meshDataArray[0],
            data,
            requireNormals, // Set to true if you require normals. If Draco data does not contain them, they are allocated and we have to calculate them below
            requireTangents // Retrieve tangents is not supported, but this will ensure they are allocated and can be calculated later (see below)
            );

        if (result.success) {

            // Apply onto new Mesh
            var mesh = new Mesh();
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray,mesh);

            // If Draco mesh has bone weigths, apply them now.
            // To get these, you have to supply the correct attribute IDs
            // to `ConvertDracoMeshToUnity` above (optional paramters).
            if (result.boneWeightData != null) {
                result.boneWeightData.ApplyOnMesh(mesh);
                result.boneWeightData.Dispose();
            }

            if (result.calculateNormals) {
                // If draco didn't contain normals, calculate them.
                mesh.RecalculateNormals();
            }
            if (requireTangents) {
                // If required (e.g. for consistent specular shading), calculate tangents
                mesh.RecalculateTangents();
            }

            // Use the resulting mesh
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
#else // DRACO_PLATFORM_SUPPORTED
    void Start()
    {
        Debug.LogError("Draco is not supported on this platform.");
    }
#endif // DRACO_PLATFORM_SUPPORTED
}
