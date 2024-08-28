using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager Instance { get; private set; }
    public PlayerInputMap PlayerInput { get; private set; }

    public EventHandler OnPlayerEnable;
    public EventHandler OnPlayerDisable;

    public Transform player {  get; private set; }

    #region Unity Functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        PlayerInput = new PlayerInputMap();
    }
    void Start()
    {
        if (Instance != this) return;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Utility

    #endregion

    #region Inputs
    public void EnablePlayer()
    {
        PlayerInput.Player.Enable();
        PlayerInput.UI.Disable();
        OnPlayerEnable?.Invoke(this, EventArgs.Empty);
    }

    public void EnableUI()
    {
        PlayerInput.Player.Disable();
        PlayerInput.UI.Enable();
        OnPlayerDisable?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    public Vector2 CurrentScreenSize()
    {
        return new Vector2(Screen.width, Screen.height);
    }
}
