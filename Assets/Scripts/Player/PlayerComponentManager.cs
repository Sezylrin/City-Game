using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerComponentManager : MonoBehaviour
{
    [field: SerializeField]
    public PlayerController controller {  get; private set; }
    [field: SerializeField]
    public PlayerInputs inputs {  get; private set; }
    [field: SerializeField]
    public PlayerValues values { get; private set; }

    public void SetValues()
    {
        controller = GetComponentInChildren<PlayerController>();
        inputs = GetComponentInChildren<PlayerInputs>();
        values = GetComponentInChildren<PlayerValues>();
        SetPCM();
    }

    private void SetPCM()
    {
        PCMInterface[] interfaces = gameObject.GetComponentsInChildren<PCMInterface>();
        foreach (PCMInterface intf in interfaces)
        {
            intf.PCM = this;
        }
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        /*string assetPath = "Assets/Prefabs/Player/Player.prefab";

        GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);

        contentsRoot.GetComponentInChildren<PlayerComponentManager>().SetValues();

        PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(contentsRoot);*/
    }
}
