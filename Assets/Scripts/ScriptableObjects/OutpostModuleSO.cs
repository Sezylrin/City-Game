using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OutpostModule", menuName = "ScriptableObjects/Outpost/OutpostModule", order = 2)]
public class OutpostModuleSO : ScriptableObject
{
    public string Name;

    public SerializableType<OutpostModule> module;

}
