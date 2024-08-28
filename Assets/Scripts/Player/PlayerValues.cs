using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerValues : MonoBehaviour, PCMInterface
{
    // Start is called before the first frame update
    public PlayerComponentManager PCM {  get; set; }
    [field: SerializeField]
    public Transform CamForward { get; private set; }
    [field: SerializeField]
    public Transform BodyForward { get; private set; }

    public GameObject DebugObj;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCamForward();
    }

    private void UpdateCamForward()
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        CamForward.forward = forward.normalized;
        DebugObj.transform.LookAt(transform.position + CamForward.forward);
    }

    
}
