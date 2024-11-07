using AYellowpaper.SerializedCollections.KeysGenerators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerInputMap PlayerInput { get; private set; }
    //public EventHandler<string> ActionMapChange;


    public PlayerInputMap.GameActions gameAction;
    public PlayerInputMap.UIActions menuAction;
    [SerializedDictionary("GameMap", "Active"), SerializeField]
    private SerializedDictionary<GameActionMap, bool> gameActionState;

    [SerializedDictionary("MenuMap", "Active"), SerializeField]
    private SerializedDictionary<UIActionMap, bool> uiActionState;
    private void Awake()
    {
        PlayerInput = new PlayerInputMap();
        gameAction = PlayerInput.Game;
        menuAction = PlayerInput.UI;
    }
    public static Vector3 mousePosInWorld;
    void Start()
    {
        EnablePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        mousePosInWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosInWorld.z = 0;
    }
    #region Inputs
    public void EnablePlayer()
    {

        foreach (var action in PlayerInput)
        {
            action.Disable();
        }
        PlayerInput.Game.Enable();
        //ActionMapChange(this, "Game");

    }

    public void EnableUI()
    {
        foreach (var action in PlayerInput)
        {
            action.Disable();
        }
        PlayerInput.UI.Enable();

        //ActionMapChange(this, "UI");
    }

    public void EnableMap(object sender, string e)
    {
        switch (e)
        {
            case "Game":
                EnableGameAction();
                DisableMenuAction();
                break;
            case "UI":
                EnableMenuAction();
                DisableGameAction();
                break;
                
        }
    }

    private void EnableGameAction()
    {
    }
    private void DisableGameAction()
    {

    }
    private void EnableMenuAction()
    {

    }    
    private void DisableMenuAction()
    {

    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("GenerateInputsEnum")]
    private void GenerateInputsEnum()
    {
        var PlayerInput = new PlayerInputMap();
        Dictionary<string, List<string>> allInputs = new Dictionary<string, List<string>>();
        foreach (InputAction action in PlayerInput)
        {
            string actionMapName = action.actionMap.name;
            if (!allInputs.ContainsKey(actionMapName))
            {
                allInputs.Add(actionMapName, new List<string>());
            }
            allInputs[actionMapName].Add(action.name);
        }
        foreach (KeyValuePair<string, List<string>> entry in allInputs)
        {
            EnumFileGenerator.GenerateEnumFile(entry.Key+"ActionMap", entry.Value);
        }
    }

#endif
}
