using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class SubGraphInitializeFromGraphTest : MonoBehaviour, IMonoBehaviourTest
{
	[SerializeField]
	int m_GlobalVertexCount = 256;
	
	[SerializeField]
	int m_EdgeCount = 512;
	
	[SerializeField]
	int m_SubGraphEdgeCount = 256;

	Graph m_Graph;
	SubGraph m_SubGraphOne;

	public bool IsTestFinished { get { return m_Finished; } }

	bool m_Finished;

	static HashSet<int> s_SubGraphVertexSet; 
	
	NativeHashMap<int, int> m_VertexIdToLocalIndex;
	NativeArray<int2> m_Edges;

	InitializeSubGraphArrayJob m_InitJob;
	JobHandle m_InitJobHandle;

	void Awake ()
	{
		m_VertexIdToLocalIndex = new NativeHashMap<int, int>(m_GlobalVertexCount, Allocator.Persistent);

		m_Graph = new Graph(m_GlobalVertexCount, m_EdgeCount);
		TestUtils.GenerateGraphInput(m_Graph.Edges, m_GlobalVertexCount);
		// this next block is all just validating the graph itself, as in the graph init test
		Assert.IsTrue(m_Graph.Edges.IsCreated);
		Assert.AreEqual(m_EdgeCount, m_Graph.Edges.Length);
		foreach (var edge in m_Graph.Edges)
		{
			Assert.IsTrue(edge.x >= 0);
			Assert.IsTrue(edge.y >= 0);
			Assert.IsTrue(edge.x < m_GlobalVertexCount);
			Assert.IsTrue(edge.y < m_GlobalVertexCount);
			Assert.AreNotEqual(edge.x, edge.y);
		}
		
		// now we'll actually test initializing the subgraph of this graph
		m_Edges = TestUtils.RandomSubGraph(m_Graph, m_SubGraphEdgeCount, out s_SubGraphVertexSet);

		var subVertexArray = s_SubGraphVertexSet.ToArray();
		m_SubGraphOne = new SubGraph(subVertexArray, m_Edges);
		
		var subGraphEdges = m_SubGraphOne.EdgesByLocalIndex;
		Assert.IsTrue(subGraphEdges.IsCreated);
		Assert.AreEqual(m_SubGraphEdgeCount, subGraphEdges.Length);

		m_InitJob = new InitializeSubGraphArrayJob()
		{
			InputEdgesAsIDs = m_Edges,
			EdgesAsIndices = m_SubGraphOne.EdgesByLocalIndex,
			VertexIDs = m_SubGraphOne.VertexIds,
			VertexIdToIndex = m_VertexIdToLocalIndex
		};

		m_InitJobHandle = m_InitJob.Schedule();
	}

	void Update ()
	{
		if (Time.frameCount < 3)
			return;

		if (!m_InitJobHandle.IsCompleted)
			m_InitJobHandle.Complete();
		
		foreach (var edge in m_InitJob.EdgesAsIndices)
		{
			Assert.AreNotEqual(edge.x, edge.y);
		}
		
		var firstEdgeByIndex = m_InitJob.EdgesAsIndices[0];
		var firstEdgeById = m_InitJob.InputEdgesAsIDs[0];
		var lastEdgeByIndex = m_InitJob.EdgesAsIndices[m_InitJob.EdgesAsIndices.Length - 1];
		var lastEdgeById = m_InitJob.InputEdgesAsIDs[m_InitJob.InputEdgesAsIDs.Length - 1];
		
		Debug.LogFormat("first edge by id: {0} , by index: {1}", firstEdgeById, firstEdgeByIndex);
		Debug.LogFormat("last edge by id: {0} , by index: {1}", lastEdgeById, lastEdgeByIndex);

		int expectedIndexForVertexA, expectedIndexForVertexB;
		m_VertexIdToLocalIndex.TryGetValue(firstEdgeById.x, out expectedIndexForVertexA);
		m_VertexIdToLocalIndex.TryGetValue(firstEdgeById.y, out expectedIndexForVertexB);
		Assert.AreEqual(expectedIndexForVertexA, firstEdgeByIndex.x);
		Assert.AreEqual(expectedIndexForVertexB, firstEdgeByIndex.y);
		
		m_VertexIdToLocalIndex.TryGetValue(lastEdgeById.x, out expectedIndexForVertexA);
		m_VertexIdToLocalIndex.TryGetValue(lastEdgeById.y, out expectedIndexForVertexB);
		Assert.AreEqual(expectedIndexForVertexA, lastEdgeByIndex.x);
		Assert.AreEqual(expectedIndexForVertexB, lastEdgeByIndex.y);

		m_Finished = true;
		enabled = false;
	}

	void OnDestroy()
	{
		m_Graph?.Dispose();
		m_SubGraphOne?.Dispose();
		m_VertexIdToLocalIndex.Dispose();
		m_Edges.Dispose();	
	}
}