using System;
using Unity.Collections;
using Unity.Mathematics;

public class Graph : IDisposable
{
    /// <summary>
    /// How many vertices are in the graph
    /// </summary>
    public int order;
    
    /// <summary>
    /// How many edges are in the graph
    /// </summary>
    public int size;
    
    /// <summary>
    /// The edges in our graph.  Each int is an index into the vertices arrays
    /// </summary>
    public NativeArray<int2> Edges;
    
    /// <summary>
    /// Create a new graph
    /// </summary>
    /// <param name="order">the number of vertices in the graph</param>
    /// <param name="size">the number of edges in the graph</param>
    public Graph(int order, int size)
    {
        Edges = new NativeArray<int2>(size, Allocator.Persistent);
        this.order = order;
        this.size = size;
    }

    public void Dispose()
    {
        Edges.Dispose();
    }
}