using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniGLTF
{
    public struct SparseBase
    {
        public readonly Vector3[] Positions;
        public readonly Vector3[] Normals;

        public SparseBase(Vector3[] positions, Vector3[] normals)
        {
            Positions = positions;
            Normals = normals;
        }
    }

    public static class BlendShapeExporter
    {
        public static gltfMorphTarget Export(glTF gltf, int gltfBuffer, Vector3[] positions, Vector3[] normals, SparseBase? sparseBase)
        {
            var accessorCount = positions.Length;
            if (normals != null && positions.Length != normals.Length)
            {
                throw new Exception();
            }

            bool useSparse = sparseBase.HasValue;
            if (sparseBase.HasValue)
            {
                var sparseIndices = Enumerable.Range(0, positions.Length).Where(x => positions[x] != Vector3.zero).ToArray();
                if (sparseIndices.Length == 0)
                {
                    useSparse = false;
                }
            }

            if (useSparse)
            {
                // positions
                var positionAccessorIndex = -1;
                var sparseIndices = Enumerable.Range(0, positions.Length).Where(x => positions[x] != Vector3.zero).ToArray();
                if (sparseIndices.Length > 0)
                {
                    Debug.LogFormat("Sparse {0}/{1}", sparseIndices.Length, positions.Length);
                    var sparseIndicesViewIndex = gltf.ExtendBufferAndGetViewIndex(gltfBuffer, sparseIndices);
                    positionAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(gltfBuffer, accessorCount, positions, sparseIndices, sparseIndicesViewIndex, glBufferTarget.NONE);
                }

                // normals
                var normalAccessorIndex = -1;
                if (normals != null)
                {
                    var sparseNormalIndices = Enumerable.Range(0, positions.Length).Where(x => normals[x] != Vector3.zero).ToArray();
                    if (sparseNormalIndices.Length > 0)
                    {
                        var sparseNormalIndicesViewIndex = gltf.ExtendBufferAndGetViewIndex(gltfBuffer, sparseNormalIndices);
                        normalAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(gltfBuffer, accessorCount, normals, sparseNormalIndices, sparseNormalIndicesViewIndex, glBufferTarget.NONE);
                    }
                }

                return new gltfMorphTarget
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                };
            }
            else
            {
                // position
                var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(gltfBuffer, positions, glBufferTarget.ARRAY_BUFFER);
                gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
                gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

                // normal
                var normalAccessorIndex = -1;
                if (normals != null)
                {
                    normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(gltfBuffer, normals, glBufferTarget.ARRAY_BUFFER);
                }

                return new gltfMorphTarget
                {
                    POSITION = positionAccessorIndex,
                    NORMAL = normalAccessorIndex,
                };
            }
        }
    }
}
