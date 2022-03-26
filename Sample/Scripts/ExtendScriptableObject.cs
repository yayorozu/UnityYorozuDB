using System;
using System.Collections.Generic;
using UnityEngine;

public class ExtendScriptableObject : ScriptableObject
{
    [SerializeField]
    public int[] IntArray;
    
    [SerializeField]
    public List<string> StringList;

    [SerializeField]
    public List<ClassSample> ClassList;
    
    [Serializable]
    public class ClassSample
    {
        [SerializeField]
        private int IntA;
        [SerializeField]
        private int IntB;
    }
}