using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;
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
    
    public static void GenerateGraphInput(NativeArray<int2> edges, int vertexCount)
    {
        var endingVertexIndex = vertexCount - 1;
        for (int i = 0; i < edges.Length; i++)
        {
            var vertexAIndex = Random.Range(0, endingVertexIndex);
            var vertexBIndex = Random.Range(0, endingVertexIndex);
            while (vertexAIndex == vertexBIndex)
            {
                vertexBIndex = Random.Range(0, endingVertexIndex);
            }

            edges[i] = new int2(vertexAIndex, vertexBIndex);
        }
    }

    static readonly HashSet<int> k_SubGraphVertexSet = new HashSet<int>();
    static readonly HashSet<int> k_SubGraphEdgeIndexSet = new HashSet<int>();

    public static NativeArray<int2> RandomSubGraph(Graph graph, int subGraphEdgeCount, out HashSet<int> vertexSet)
    {
        k_SubGraphVertexSet.Clear();
        k_SubGraphEdgeIndexSet.Clear();
        var edgeList = new NativeArray<int2>(subGraphEdgeCount, Allocator.Temp);

        var graphEdgeCount = graph.size;
        var edges = graph.Edges;
        for (int i = 0; i < subGraphEdgeCount; i++)
        {
            var index = Random.Range(0, graphEdgeCount);
            while (k_SubGraphEdgeIndexSet.Contains(index))
            {
                index = Random.Range(0, graphEdgeCount);
            }
            
            k_SubGraphEdgeIndexSet.Add(index);

            var edge = edges[index];
            k_SubGraphVertexSet.Add(edge.x);
            k_SubGraphVertexSet.Add(edge.y);
            edgeList[i] = edge;
        }

        vertexSet = k_SubGraphVertexSet;
        return edgeList;
    }
}
