using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class OutpostSO : ScriptableObject
{
    [CreateAssetMenu(fileName = "OutpostStats", menuName = "ScriptableObjects/Outpost/OutpostStats", order = 1)]
    public class OutpostModuleSO : ScriptableObject
    {
        [SerializedDictionary("Resources", "StoredAmount")]
        public SerializedDictionary<Resources, int> localResources;

    }

}
