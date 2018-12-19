using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class GraphTests 
{
    [UnityTest]
    public IEnumerator SubGraphInitializeFromScratch() 
    {
        yield return new MonoBehaviourTest<SubGraphInitializeFromScratchTest>();
    }
}
