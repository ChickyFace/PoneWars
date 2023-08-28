using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="PoneScriptableObject",menuName = "ScriptableObjects/PoneScriptableObject")]
public class PoneScriptableObjects : ScriptableObject
{

    public string poneName;

    public LayerMask poneLayer;

}
