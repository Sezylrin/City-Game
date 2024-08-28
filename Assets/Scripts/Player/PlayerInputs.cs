using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour, PCMInterface
{
    // Start is called before the first frame update

    private PlayerInputMap.PlayerActions playerActions;
    [field: SerializeField]
    public PlayerComponentManager PCM { get; set; }
    void Awake()
    {
        playerActions = GameManager.Instance.PlayerInput.Player;
    }
    private void OnEnable()
    {
        /*GameManager.Instance.EnablePlayer();
        playerActions.Move.performed += PCM.controller.OnMove;
        playerActions.Move.canceled += PCM.controller.OnMove;
        playerActions.Jump.performed += PCM.controller.OnJump;
        playerActions.Dash.performed += PCM.controller.OnDash;*/
    }

    private void OnDisable()
    {
        /*playerActions.Jump.performed -= PCM.controller.OnJump;
        playerActions.Move.performed -= PCM.controller.OnMove;
        playerActions.Move.canceled -= PCM.controller.OnMove;
        playerActions.Dash.performed -= PCM.controller.OnDash;
        playerActions.Disable();*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
