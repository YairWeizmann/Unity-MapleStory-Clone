using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class CameraFollower : NetworkBehaviour
{
   [SerializeField] private Transform playerTransform;
    public Vector3 cameraOffset = new Vector3();
    public float smoothFactor = 0.2f;

    public Camera PlayerCamera;

    // Bounds
 
    private void Start()
    {
        PlayerCamera = GetComponent<Camera>();


        if (!IsOwner)
        {
            // Disable the camera for other players to avoid conflicts
            
            PlayerCamera.enabled = false;
            return;
        } 
        if (playerTransform == null)
        {
            FindPlayerTransform();

        }
        else
            Debug.Log("Found Player Transform+ " + playerTransform);




    }

    private void LateUpdate()
    {

       

        if (playerTransform != null)
        {
            Vector3 targetPosition = playerTransform.position + cameraOffset;

            // Clamp target position to bounds
          
            // Smoothly interpolate between the current camera position and the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothFactor);


        }
    }


    public void FindPlayerTransform()
    {
        playerTransform = transform.root.transform;


        NetworkObject PlayerNet = playerTransform.GetComponent<NetworkObject>();

        if(PlayerCamera == null)
        {
            Debug.LogError("Camera Is Null Fuckkk");
        }
    }

 

    public GameObject GetRootGameObject()
    {
        Transform CurrentTransform = transform;

        while(CurrentTransform.parent != null)
        {
            CurrentTransform = CurrentTransform.parent;

            if (CurrentTransform.parent == null)
                return CurrentTransform.gameObject;
        }

        return CurrentTransform.gameObject;
    }

    private void OnDrawGizmos()
    {
        // Draw the camera bounds in the scene view for visualization
        Gizmos.color = Color.green;
     
    }
}
