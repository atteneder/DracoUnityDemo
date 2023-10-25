// SPDX-FileCopyrightText: 2023 Unity Technologies and the Draco for Unity authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateTestMesh : MonoBehaviour
{
    public Mesh mesh;


    public Transform[] bones;

    // Start is called before the first frame update
    void Start()
    {
        const int vCount = 3;

        mesh = new Mesh();

        mesh.SetVertexBufferParams(vCount, new[] {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.BlendIndices,VertexAttributeFormat.UInt16,4,1),
            new VertexAttributeDescriptor(VertexAttribute.BlendWeight,VertexAttributeFormat.Float32,4,2),
        });

        mesh.SetIndexBufferParams(3, IndexFormat.UInt16);

        var vDataPos = new NativeArray<float3>(vCount, Allocator.Temp);
        var vDataBlendIndices = new NativeArray<ushort>(vCount * 4, Allocator.Temp);
        var vDataBlendWeights = new NativeArray<float4>(vCount, Allocator.Temp);

        vDataPos[0] = new float3(0, 0, 0);
        vDataPos[1] = new float3(1, 0, 0);
        vDataPos[2] = new float3(0, 1, 0);

        vDataBlendIndices[0] = 0;
        vDataBlendIndices[1] = 0;
        vDataBlendIndices[2] = 0;
        vDataBlendIndices[3] = 0;

        vDataBlendIndices[4] = 1;
        vDataBlendIndices[5] = 0;
        vDataBlendIndices[6] = 0;
        vDataBlendIndices[7] = 0;

        vDataBlendIndices[8] = 2;
        vDataBlendIndices[9] = 0;
        vDataBlendIndices[10] = 0;
        vDataBlendIndices[11] = 0;

        vDataBlendWeights[0] = new float4(1, 0, 0, 0);
        vDataBlendWeights[1] = new float4(1, 0, 0, 0);
        vDataBlendWeights[2] = new float4(1, 0, 0, 0);

        mesh.SetVertexBufferData(vDataPos, 0, 0, vCount);
        mesh.SetVertexBufferData(vDataBlendIndices, 0, 0, vCount * 4, 1);
        mesh.SetVertexBufferData(vDataBlendWeights, 0, 0, vCount, 2);

        var indices = new NativeArray<ushort>(vCount, Allocator.Temp);
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        mesh.SetIndexBufferData(indices, 0, 0, vCount);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, vCount, MeshTopology.Triangles));

        var bindPoses = new Matrix4x4[3];
        bindPoses[0] = Matrix4x4.Translate(vDataPos[0]).inverse;
        bindPoses[1] = Matrix4x4.Translate(vDataPos[1]).inverse;
        bindPoses[2] = Matrix4x4.Translate(vDataPos[2]).inverse;
        mesh.bindposes = bindPoses;

        vDataPos.Dispose();
        vDataBlendIndices.Dispose();
        vDataBlendWeights.Dispose();
        indices.Dispose();

        var smr = gameObject.GetComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = mesh;
        smr.bones = bones;
    }
}
