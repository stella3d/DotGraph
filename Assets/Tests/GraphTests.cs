using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class GraphTests 
{
    [UnityTest]
    public IEnumerator GraphInitializeFromScratch() 
    {
        yield return new MonoBehaviourTest<GraphInitializeFromScratchTest>();
    }
    
    [UnityTest]
    public IEnumerator SubGraphInitializeFromScratch() 
    {
        yield return new MonoBehaviourTest<SubGraphInitializeFromScratchTest>();
    }
    
    [UnityTest]
    public IEnumerator SubGraphInitializeFromGraph() 
    {
        yield return new MonoBehaviourTest<SubGraphInitializeFromGraphTest>();
    }
}
