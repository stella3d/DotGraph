using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct InitializeSubGraphJob : IJob
{
    [ReadOnly]
    public NativeList<int> InputVertices;
    
    [WriteOnly]
    public NativeArray<int> VertexIDs;

    [ReadOnly]
    public NativeList<int2> InputEdgesAsIDs;
    
    [WriteOnly]
    public NativeArray<int2> EdgesAsIndices;

    public NativeHashMap<int, int> VertexIdToIndex;

    public void Execute()
    {
        VertexIdToIndex.Clear();
        for (int v = 0; v < InputVertices.Length; v++)
        {
            var inputID = InputVertices[v];
            VertexIDs[v] = inputID;
            VertexIdToIndex.TryAdd(inputID, v);
        }
        
        for (int e = 0; e < InputEdgesAsIDs.Length; e++)
        {
            var inputEdge = InputEdgesAsIDs[e];
            var vertexIdA = inputEdge.x;
            var vertexIdB = inputEdge.y;

            // translate the representation of global vertex ids to subgraph vertex indices
            int vertexIndexA, vertexIndexB;
            VertexIdToIndex.TryGetValue(vertexIdA, out vertexIndexA);
            VertexIdToIndex.TryGetValue(vertexIdB, out vertexIndexB);

            EdgesAsIndices[e] = new int2(vertexIndexA, vertexIndexB);
        }
    }
}