using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct EdgeLengthCalculationJob : IJob
{
    [ReadOnly]
    public NativeArray<int2> Edges;
    
    [ReadOnly]
    public NativeArray<float3> VertexPositions;
    
    [WriteOnly]
    public NativeArray<float> EdgeSquareMagnitudes;
    
    public void Execute()
    {
        for (int e = 0; e < EdgeSquareMagnitudes.Length - 1; e++)
        {
            var edge = Edges[e];
            var vertexPositionX = VertexPositions[edge.x];
            var vertexPositionY = VertexPositions[edge.y];

            var d = vertexPositionY - vertexPositionX;
            EdgeSquareMagnitudes[e] = d.x * d.x + d.y * d.y + d.z * d.z;
        }
    }
}

[BurstCompile]
public struct BasicGraphRandomGenerationJob : IJob
{
    [WriteOnly]
    public NativeArray<float3> VertexPositions;
    
    [ReadOnly]
    public NativeArray<float3> VertexNoiseSeed;
    
    public void Execute()
    {
        for (int v = 0; v < VertexPositions.Length - 1; v++)
        {
            var seed = VertexNoiseSeed[v];
            VertexPositions[v] = seed + noise.srdnoise(seed.xy, seed.z);
        }
    }
}

[BurstCompile]
public struct DisjointUnionJob : IJob
{
    public NativeArray<int> GraphAVertexIds;
    public NativeArray<int2> GraphAEdges;
    
    public NativeArray<int> GraphBVertexIds;
    public NativeArray<int2> GraphBEdges;
    
    [WriteOnly]
    public NativeList<int> OutputGraphVertices;
    [WriteOnly]
    public NativeList<int2> OutputGraphEdges;
    
    public void Execute()
    {
        OutputGraphVertices.Clear();
        OutputGraphVertices.AddRange(GraphAVertexIds);
        OutputGraphVertices.AddRange(GraphBVertexIds);
        
        OutputGraphEdges.Clear();
        OutputGraphEdges.AddRange(GraphAEdges);
        OutputGraphEdges.AddRange(GraphBEdges);
    }
}

[BurstCompile]
public struct VertexDegreeCalculationJob : IJob
{
    [ReadOnly]
    public NativeArray<int2> GraphEdges;
    
    /// <summary>
    /// The number of edges each vertex belongs to
    /// </summary>
    public NativeArray<int> VertexDegrees;

    /// <summary>
    /// This array has length one. 0.x is minimum degree, 0.y is maximum degree
    /// </summary>
    public NativeArray<int2> MinAndMaxOutput;

    public void Execute()
    {
        for (var i = 0; i < VertexDegrees.Length; i++)
        {
            VertexDegrees[i] = 0;
        }
        
        for (var i = 0; i < GraphEdges.Length; i++)
        {
            var edge = GraphEdges[i];
            var vX = edge.x;
            var vY = edge.y;
            VertexDegrees[vX] = VertexDegrees[vX] + 1;                   
            VertexDegrees[vY] = VertexDegrees[vY] + 1;
        }

        for (var v = 0; v < VertexDegrees.Length; v++)
        {
            var degree = VertexDegrees[v];
            var old = MinAndMaxOutput[0];
            var oldMin = old.x;
            var oldMax = old.y;
            MinAndMaxOutput[0] = new int2(
                math.select(oldMin, degree, degree < oldMin),
                math.select(oldMax, degree, degree > oldMax));
        }
    }
}

