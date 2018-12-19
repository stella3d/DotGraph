using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicGraphManager))]
public class BasicGraphManagerInspector : Editor
{
    BasicGraphManager m_Graph;
    
    void OnEnable()
    {
        m_Graph = (BasicGraphManager) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("regenerate graph"))
        {
            m_Graph.RegenerateGraph();
        }
    }
}
