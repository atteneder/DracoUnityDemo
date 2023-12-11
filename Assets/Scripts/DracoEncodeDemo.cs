// SPDX-FileCopyrightText: 2023 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using UnityEngine;
using Draco.Encode;
using UnityEngine.Rendering;

public class DracoEncodeDemo : MonoBehaviour
{
    IEnumerator Start()
    {
        var sphere = GetComponent<MeshFilter>().sharedMesh;
        var vertices = sphere.vertices;

        var mesh = new Mesh
        {
            subMeshCount = 1
        };
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, 0, MeshTopology.Points));
        mesh.vertices = vertices;

        var task = DracoEncoder.EncodeMesh(mesh);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        var results = task.Result;
        foreach (var result in results)
        {
            Debug.Log($"Encoded mesh into {result.data.Length} bytes with {result.indexCount} indices and {result.vertexCount} vertices.");
            result.Dispose();
        }
    }
}
