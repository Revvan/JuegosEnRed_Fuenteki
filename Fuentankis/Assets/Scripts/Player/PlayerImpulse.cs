using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerImpulse : MonoBehaviourPun
{
    [SerializeField] float chargeDuration = 1f;
    [SerializeField] float impulseForce = 10f;
    [SerializeField] float impulseDuration = 0.5f;
    [SerializeField] ShieldSystem shieldSystem;
    [SerializeField] Key keyToCharge;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] GameObject playerChar;
    float currentTime = 0;

    void Update()
    {
        if (shieldSystem.isShieldActive && photonView.IsMine)
        {
            if (Keyboard.current[keyToCharge].isPressed)
            {
                currentTime += Time.deltaTime;
                Debug.Log("cargando");
            }
            else
            {
                if (currentTime >= chargeDuration)
                {
                    currentTime = 0;
                    
                    if (playerMovement != null)
                    {
                        playerMovement.StartImpulse(playerChar.transform.up * impulseForce, impulseDuration);
                        //playerMovement.impulseForce = impulseForce;
                        Debug.Log(playerChar.transform.up * impulseForce);
                    }
                    
                }
            }
        }
        
    }
}
