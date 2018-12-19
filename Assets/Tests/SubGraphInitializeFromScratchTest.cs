using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class SubGraphInitializeFromScratchTest : MonoBehaviour, IMonoBehaviourTest
{
	[SerializeField]
	int m_GlobalVertexCount = 1024;
	
	[SerializeField]
	int m_SubGraphVertexCount = 512;
	
	[SerializeField]
	int m_SubGraphEdgeCount = 1024;

	SubGraph m_SubGraphOne;
	
	NativeList<int> m_InputVertices;
	NativeList<int2> m_InputEdgesById;
	NativeHashMap<int, int> m_VertexIdToLocalIndex;

	InitializeSubGraphJob m_InitJob;

	JobHandle m_CreationJobHandle;

	public bool IsTestFinished
	{
		get { return m_Finished; }
	}

	bool m_Finished;

	void Awake ()
	{
		m_InputVertices = new NativeList<int>(m_SubGraphVertexCount, Allocator.Persistent);
		m_InputEdgesById = new NativeList<int2>(m_SubGraphEdgeCount, Allocator.Persistent);
		m_VertexIdToLocalIndex = new NativeHashMap<int, int>(m_SubGraphVertexCount, Allocator.Persistent);
		
		TestUtils.GenerateSubGraphInput(m_InputVertices, m_SubGraphVertexCount, m_GlobalVertexCount, 
			m_InputEdgesById, m_SubGraphEdgeCount);
		
		m_SubGraphOne = new SubGraph(m_InputVertices, m_InputEdgesById);

		m_InitJob = new InitializeSubGraphJob()
		{
			InputVertices = m_InputVertices,
			InputEdgesAsIDs = m_InputEdgesById,
			EdgesAsIndices = m_SubGraphOne.EdgesByLocalIndex,
			VertexIDs = m_SubGraphOne.VertexIds,
			VertexIdToIndex = m_VertexIdToLocalIndex
		};

		m_CreationJobHandle = m_InitJob.Schedule();
	}

	void Update ()
	{
		if (Time.frameCount < 3)
			return;
		
		if(!m_CreationJobHandle.IsCompleted)
			m_CreationJobHandle.Complete();

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
		if(!m_CreationJobHandle.IsCompleted)
			m_CreationJobHandle.Complete();
		
		m_SubGraphOne?.Dispose();

		m_InputVertices.Dispose();
		m_InputEdgesById.Dispose();
		m_VertexIdToLocalIndex.Dispose();
	}

}