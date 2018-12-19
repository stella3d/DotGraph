using System;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class BasicGraph : IDisposable
{
    public NativeArray<int> VertexIds;
    
    /// <summary>
    /// The edges in our graph.  Each int is an index into the vertices arrays
    /// </summary>
    public NativeArray<int2> Edges;

    public int order;
    public int size;
    
    Random m_Random;
    
    /// <summary>
    /// Create a new graph
    /// </summary>
    /// <param name="order">the number of vertices in the graph</param>
    /// <param name="size">the number of edges in the graph</param>
    public BasicGraph(int order, int size)
    {
        m_Random = new Random();
        m_Random.InitState();
        
        VertexIds = new NativeArray<int>(order, Allocator.Persistent);
        Edges = new NativeArray<int2>(size, Allocator.Persistent);
        this.order = order;
        this.size = size;
    }

    public void Dispose()
    {
        VertexIds.Dispose();
        Edges.Dispose();
    }

    public void RandomizeEdges()
    {
        for (int e = 0; e < size; e++)
        {
            var x = VertexIds[UnityEngine.Random.Range(0, order)];
            var y = VertexIds[UnityEngine.Random.Range(0, order)];
            var edge = new int2(x, y);
            while (edge.x == edge.y)
            {
                x = VertexIds[UnityEngine.Random.Range(0, order)];
                y = VertexIds[UnityEngine.Random.Range(0, order)];
                edge = new int2(x, y);
            }

            Edges[e] = edge;
        }
    }
}