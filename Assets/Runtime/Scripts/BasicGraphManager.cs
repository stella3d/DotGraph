using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BasicGraphManager : MonoBehaviour 
{
	[SerializeField]
	int m_VertexCount = 1024;
	
	//[SerializeField]
	int m_EdgeCount = 2048;
	
	
	[SerializeField]
	[Range(6, 16)]
	[Tooltip("The graph will have edges of count 2 to this power")]
	int m_EdgeCountPower = 11;
	
	[SerializeField]
	[Range(1, 10)]
	int m_SeedRadius = 5;
	
	NativeArray<float> m_EdgeMagnitudes;
	NativeArray<float3> m_VerticesSeed;
	
	NativeArray<int> m_VertexDegrees;
	
	NativeArray<int2> m_MinAndMaxOutput;

	BasicGraph m_Graph;
	
	JobHandle m_CreationJobHandle;
	JobHandle m_MagnitudeJobHandle;
	JobHandle m_DegreeCalculationJobHandle;
	
	VertexDegreeCalculationJob m_DegreeCalculationJob;
	
	int m_PreviousVertexCount;
	int m_PreviousEdgeCount;
	
	void Awake ()
	{
		m_EdgeCount = (int)Mathf.Pow(2, m_EdgeCountPower);
		Debug.Log("Edge count: " + m_EdgeCount);
		Debug.Log("Vertex count: " + m_VertexCount);
		
		m_Graph = new BasicGraph(m_VertexCount, m_EdgeCount);
		m_EdgeMagnitudes = new NativeArray<float>(m_VertexCount, Allocator.Persistent);
		m_VerticesSeed = new NativeArray<float3>(m_VertexCount, Allocator.Persistent);
		m_VertexDegrees = new NativeArray<int>(m_VertexCount, Allocator.Persistent);
		m_MinAndMaxOutput = new NativeArray<int2>(1, Allocator.Persistent);

		for (int i = 0; i < m_VertexCount - 1; i++)
		{
			m_VerticesSeed[i] = new float3(Random.onUnitSphere * m_SeedRadius);
		}

		m_Graph.RandomizeEdges();
		
		m_DegreeCalculationJob = new VertexDegreeCalculationJob();

		m_CreationJobHandle = new JobHandle();
		
		m_PreviousVertexCount = m_VertexCount;
		m_PreviousEdgeCount = m_EdgeCount;

		var firstEdge = m_Graph.Edges[0];
		var secondEdge = m_Graph.Edges[1];
		Debug.LogFormat("first edge : {0} <-> {1}", firstEdge.x, firstEdge.y);
		Debug.LogFormat("second edge : {0} <-> {1}", secondEdge.x, secondEdge.y);
	}

	void Update ()
	{
		if (Time.frameCount % 6 != 0)
			return;
		
		if (!m_DegreeCalculationJobHandle.IsCompleted)
			m_DegreeCalculationJobHandle.Complete();

		if (m_MinAndMaxOutput.IsCreated)
		{
			var minMaxData = m_MinAndMaxOutput;
			Debug.LogFormat("min degree {0}  max degree {1}", minMaxData[0].x, minMaxData[0].y);
			
			var firstVertex = m_VertexDegrees[0];
			var secondVertex = m_VertexDegrees[1];
			Debug.LogFormat("vertex degrees : {0} , {1}, {2}", firstVertex, secondVertex, m_VertexDegrees[2]);
			
			m_MinAndMaxOutput[0] = new int2(int.MaxValue, 0);
			RegenerateGraph();
		}

		if (m_VertexCount != m_PreviousVertexCount || m_EdgeCount != m_PreviousEdgeCount)
		{
			RegenerateGraph();
		}

		m_DegreeCalculationJob = new VertexDegreeCalculationJob()
		{
			GraphEdges = m_Graph.Edges,
			VertexDegrees = m_VertexDegrees,
			MinAndMaxOutput = m_MinAndMaxOutput
		};

		m_DegreeCalculationJobHandle = m_DegreeCalculationJob.Schedule(m_CreationJobHandle);

		m_PreviousVertexCount = m_VertexCount;
		m_PreviousEdgeCount = m_EdgeCount;
	}

	public void RegenerateGraph()
	{
		m_CreationJobHandle.Complete();
		m_DegreeCalculationJobHandle.Complete();
		
		m_Graph.RandomizeEdges();

		Debug.Log("regenerating random graph edges");
	}

	void OnDestroy()
	{
		if(!m_DegreeCalculationJobHandle.IsCompleted)
			m_DegreeCalculationJobHandle.Complete();
		
		m_Graph?.Dispose();
		m_VerticesSeed.Dispose();
		m_EdgeMagnitudes.Dispose();
		m_VertexDegrees.Dispose();
	}
}
