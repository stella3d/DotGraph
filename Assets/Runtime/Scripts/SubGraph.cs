using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class SubGraph : IDisposable
{
    public NativeArray<int> VertexIds;
    
    /// <summary>
    /// The edges in our graph.  Each int is an index into the VertexIDs array
    /// </summary>
    public NativeArray<int2> EdgesByLocalIndex;
    
    /// <summary>
    /// The edges in our graph.  Each int is a vertex id in the root graph
    /// </summary>
    public NativeArray<int2> EdgesByGlobalId;

    public int order;
    public int size;
    
    /// <summary>
    /// Create a new graph
    /// </summary>
    /// <param name="order">the number of vertices in the graph</param>
    /// <param name="size">the number of edges in the graph</param>
    public SubGraph(int order, int size)
    {
        VertexIds = new NativeArray<int>(order, Allocator.Persistent);
        EdgesByLocalIndex = new NativeArray<int2>(size, Allocator.Persistent);
        this.order = order;
        this.size = size;
    }
    
    public SubGraph(NativeList<int> vertexIDs, NativeList<int2> edges)
    {
        order = vertexIDs.Length;
        size = edges.Length;
        VertexIds = new NativeArray<int>(order, Allocator.Persistent);
        EdgesByLocalIndex = new NativeArray<int2>(size, Allocator.Persistent);
    }
    
    public SubGraph(int[] vertexIDs, NativeArray<int2> edges)
    {
        order = vertexIDs.Length;
        size = edges.Length;
        VertexIds = new NativeArray<int>(vertexIDs, Allocator.Persistent);
        
        // copy the edges represented as root vertex IDs
        EdgesByGlobalId = new NativeArray<int2>(edges, Allocator.Persistent);
        // then allocate enough space to translate them into local indices
        EdgesByLocalIndex = new NativeArray<int2>(edges.Length, Allocator.Persistent);
    }

    public void Dispose()
    {
        VertexIds.Dispose();
        EdgesByLocalIndex.Dispose();
    }
}

