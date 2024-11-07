using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
public enum Resources 
{
    Supply,
    food,
    organics,

    Metal,
    Plastic,
    military,

}
public class Outpost : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private List<OutpostModule> moduleList;

    [SerializedDictionary("Resources","StoredAmount"),SerializeField]
    private SerializedDictionary<Resources,int> localResources;



    public OutpostModuleSO debugSO;
    void Start()
    {
        ConstructModule(debugSO);
        foreach (Resources resource in Enum.GetValues(typeof(Resources)))
        {
            localResources.Add(resource, 0);

        }
    }

    public void InitiateOutpost(OutpostSO outpostSO)
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConstructModule(OutpostModuleSO ModuleSO)
    {
        OutpostModule module = gameObject.AddComponent(ModuleSO.module) as OutpostModule;
        moduleList.Add(module);
    }
}
