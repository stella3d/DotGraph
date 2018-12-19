using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class GraphInitializeFromScratchTest : MonoBehaviour, IMonoBehaviourTest
{
	[SerializeField]
	int m_GlobalVertexCount = 256;
	
	[SerializeField]
	int m_EdgeCount = 512;

	Graph m_Graph;
	
	JobHandle m_CreationJobHandle;

	public bool IsTestFinished { get { return m_Finished; } }

	bool m_Finished;

	void Awake ()
	{
		m_Graph = new Graph(m_GlobalVertexCount, m_EdgeCount);
		
		TestUtils.GenerateGraphInput(m_Graph.Edges, m_GlobalVertexCount);
	}

	void Update ()
	{
		if (Time.frameCount < 2)
			return;
		
		Assert.IsTrue(m_Graph.Edges.IsCreated);
		Assert.AreEqual(m_EdgeCount, m_Graph.Edges.Length);
		foreach (var edge in m_Graph.Edges)
		{
			Assert.IsTrue(edge.x >= 0);
			Assert.IsTrue(edge.y >= 0);
			Assert.IsTrue(edge.x < m_GlobalVertexCount);
			Assert.IsTrue(edge.y < m_GlobalVertexCount);
		}

		m_Finished = true;
		enabled = false;
	}

	void OnDestroy()
	{
		m_Graph?.Dispose();
	}
}