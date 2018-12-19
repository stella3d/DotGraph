using Unity.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public static class TestUtils
{
    public static void GenerateSubGraphInput(NativeList<int> vertices, int localVertexCount, int globalVertexCount, 
        NativeList<int2> edges, int edgeCount)
    {
        vertices.Clear();
        edges.Clear();
        
        // for now, just choose every other vertex for the subgraph
        for (int i = 0; i < globalVertexCount; i+=2)
        {
            vertices.Add(i);
        }

        var endingSubGraphIndex = localVertexCount - 1;
        for (int i = 0; i < edgeCount; i++)
        {
            var vertexAIndex = Random.Range(0, endingSubGraphIndex);
            var vertexBIndex = Random.Range(0, endingSubGraphIndex);
            while (vertexAIndex == vertexBIndex)
            {
                vertexBIndex = Random.Range(0, endingSubGraphIndex);
            }

            var self = vertices[vertexAIndex];
            var other = vertices[vertexBIndex];
            edges.Add(new int2(self, other));
        }
    }
}
