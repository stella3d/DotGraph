using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class MultiGraphManager : MonoBehaviour 
{
	[SerializeField]
	int m_TotalVertexCount = 1024 * 16;
	
	[SerializeField]
	int m_SubgraphVertexCount = 2048;
	
	[SerializeField]
	int m_SubgraphEdgeCount = 4096;
	
	[SerializeField]
	int m_CountNoise = 256;
	
	
	NativeArray<int> m_TotalVertexSet;
	NativeArray<int2> m_MinAndMaxOutput;

	NativeList<int> m_VerticesUnion;
	NativeList<int2> m_EdgesUnion;
	
	NativeList<int> m_VertexDegrees;

	BasicGraph m_GraphA;
	BasicGraph m_GraphB;
	
	
	JobHandle m_DegreeJobHandle;
	JobHandle m_DisjointUnionJobHandle;
	
	DisjointUnionJob m_DisjointUnionJob;
	
	int m_PreviousVertexCount;
	int m_PreviousEdgeCount;
	
	void Awake ()
	{
		m_GraphA = new BasicGraph(m_SubgraphVertexCount, m_SubgraphEdgeCount);
		m_GraphB = new BasicGraph(m_SubgraphVertexCount, m_SubgraphEdgeCount);
		m_TotalVertexSet = new NativeArray<int>(m_TotalVertexCount, Allocator.Persistent);
		m_MinAndMaxOutput = new NativeArray<int2>(1, Allocator.Persistent);
		
		m_VertexDegrees = new NativeList<int>(m_SubgraphVertexCount * 2, Allocator.Persistent);
		m_VerticesUnion = new NativeList<int>(m_SubgraphVertexCount * 2, Allocator.Persistent);
		m_EdgesUnion = new NativeList<int2>(m_SubgraphEdgeCount * 2, Allocator.Persistent);
		
		AddRandomVertices(m_GraphA);
		AddRandomVertices(m_GraphB);
		m_GraphA.RandomizeEdges();
		m_GraphB.RandomizeEdges();
		
		m_DisjointUnionJob = new DisjointUnionJob();

		m_PreviousVertexCount = m_TotalVertexCount;
		m_PreviousEdgeCount = m_SubgraphEdgeCount;
	}

	void Update ()
	{
		if (Time.frameCount % 30 != 0 || Time.frameCount > 200)
			return;
		
		m_DegreeJobHandle.Complete();
		
		if (m_MinAndMaxOutput.IsCreated && m_VertexDegrees.Length > 0)
		{
			var minMaxData = m_MinAndMaxOutput;
			Debug.LogFormat("min degree {0}  max degree {1}", minMaxData[0].x, minMaxData[0].y);
			
			var firstVertex = m_VertexDegrees[0];
			var secondVertex = m_VertexDegrees[1];
			Debug.LogFormat("vertex degrees : {0} , {1}, {2}", firstVertex, secondVertex, m_VertexDegrees[2]);
			m_MinAndMaxOutput[0] = new int2(int.MaxValue, 0);
		}

		RegenerateGraph();

		m_DisjointUnionJob = new DisjointUnionJob()
		{
			GraphAEdges = m_GraphA.Edges,
			GraphBEdges = m_GraphB.Edges,
			GraphAVertexIds = m_GraphA.VertexIds,
			GraphBVertexIds = m_GraphB.VertexIds,
			OutputGraphVertices = m_VerticesUnion,
			OutputGraphEdges = m_EdgesUnion
		};

		m_DisjointUnionJobHandle = m_DisjointUnionJob.Schedule(m_DisjointUnionJobHandle);

		m_PreviousVertexCount = m_TotalVertexCount;
	}

	void LateUpdate()
	{
		if (Time.frameCount % 30 != 0 || Time.frameCount > 200)
			return;
		
		m_DisjointUnionJobHandle.Complete();
		
		var degreeJob = new VertexDegreeCalculationJob()
		{
			VertexDegrees = m_VertexDegrees.ToDeferredJobArray(),
			GraphEdges = m_EdgesUnion,
			MinAndMaxOutput = m_MinAndMaxOutput
		};


		m_DegreeJobHandle = degreeJob.Schedule(m_DisjointUnionJobHandle);
	}

	void AddRandomVertices(BasicGraph graph, int min = 0, int max = -1)
	{
		if (max == -1)
			max = m_TotalVertexCount;
		if (min < 0)
			min = 0;
		
		for (var i = 0; i < graph.VertexIds.Length; i++)
		{
			graph.VertexIds[i] = Random.Range(min, max);
		}
	}

	public void RegenerateGraph()
	{
		//Debug.Log("regenerating random graph edges");

		m_DisjointUnionJobHandle.Complete();

		AddRandomVertices(m_GraphA);
		AddRandomVertices(m_GraphB);
		
		m_GraphA.RandomizeEdges();
		m_GraphB.RandomizeEdges();
	}

	void OnDestroy()
	{
		if(!m_DisjointUnionJobHandle.IsCompleted)
			m_DisjointUnionJobHandle.Complete();
		
		m_VerticesUnion.Dispose();
		m_EdgesUnion.Dispose();
		m_GraphA?.Dispose();
		m_GraphB?.Dispose();
		m_MinAndMaxOutput.Dispose();
	}
}
