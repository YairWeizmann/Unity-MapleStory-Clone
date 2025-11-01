using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Mono.CSharp;
using System.Diagnostics.Tracing;
using UnityEditor.Rendering;
public class WeopenParent : NetworkBehaviour
{
    [Header("Game Objects")]
    public Transform arm;

    [Header("Scripts")]
    PlayerMovement PlayerMovementScript;

    [Header("Facing Right")]
    private bool isFacingRight;

    private float angle;
    private Vector3 cursorPos = new Vector3();
    private Vector3 direction = new Vector3();


    private Camera PlayerCamera;

    bool ShouldMove = true;
    private void Start()
    {
        if (IsOwner)
            GetComponents();
        //   GetCameraFromSibling();


    }

    public void GetComponents()
    {
        PlayerMovementScript = GetComponentInParent<PlayerMovement>();

        Transform ParentTransform = transform.parent;

        if (ParentTransform != null)
        {
            GameObject PlayerParent = ParentTransform.gameObject;
            NetworkObject playernet = PlayerParent.GetComponent<NetworkObject>();

            PlayerCamera = PlayerParent.GetComponentInChildren<Camera>();

        }

        RequestChangeInOwnerShipClientRpc();
    }


    public void GetCameraFromSibling()
    {
        GameObject PlayerObject = GetRootComponenet().gameObject;

        if (PlayerObject.transform.parent != null)
        {
            GameObject PlayerParent = PlayerObject.transform.parent.gameObject;

            if (IsOwner && IsLocalPlayer)
            {
                // Assign the camera specific to this player
                PlayerCamera = GetComponentInChildren<Camera>();
            }
        }


    }
    public GameObject GetRootComponenet()
    {
        Transform CurrentTransform = transform;

        while (transform.parent != null)
        {
            CurrentTransform = transform.parent;
            if (transform.parent == null)
                return transform.gameObject;
        }
        return transform.gameObject;
    }

    [ClientRpc]
    public void RequestChangeInOwnerShipClientRpc()
    {
        if (IsClient)
        {
            ChangeArmOwnerShipServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeArmOwnerShipServerRpc()
    {
        NetworkObject PlayerNetObject = GetRootComponent<NetworkObject>();

        if (PlayerNetObject != null)
        {


            ulong PlayerNetId = PlayerNetObject.OwnerClientId;


            NetworkObject ThisArmNet = GetComponent<NetworkObject>();

            ThisArmNet.ChangeOwnership(PlayerNetId);


        }
    }

    public NetworkObject GetRootComponent<NetworkObject>() where NetworkObject : Component
    {
        Transform currentTransform = transform;

        // Traverse up the hierarchy until the root
        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;


            if (currentTransform.parent == null)
                return currentTransform.GetComponent<NetworkObject>();

        }

        return null;
        // Now currentTransform is the root transform
    }

    void Update()
    {

        // Only control the arm if this is the owner


        if (!IsOwner)
            return;

        if (Input.GetKeyUp(KeyCode.M))
            ShouldMove = !ShouldMove;

        if (ShouldMove)
        {
            cursorPos = PlayerCamera.ScreenToWorldPoint(Input.mousePosition);
            cursorPos.z = 0; // Ensure the cursor position is on the same plane

            // Calculate the direction from the arm to the cursor
            direction = cursorPos - arm.position;
            direction.z = 0f; // Ensure no rotation in the z-axis (2D)

            // Calculate the angle in radians
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Determine the player's direction
            if (PlayerMovementScript != null)
            {
                isFacingRight = PlayerMovementScript.isFacingRight;

                // Flip the arm's scale and rotation based on the player's facing direction
                if (isFacingRight)
                {
                    arm.localScale = new Vector3(-1f, 1f, 1f); // Normal arm scale for right-facing
                    arm.rotation = Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
                    arm.localScale = new Vector3(1f, 1f, 1f); // Flipped arm scale for left-facing
                    arm.rotation = Quaternion.Euler(0f, 0f, angle + 180f); // Adjust rotation for left-facing
                }
            }

        }

    }

}
