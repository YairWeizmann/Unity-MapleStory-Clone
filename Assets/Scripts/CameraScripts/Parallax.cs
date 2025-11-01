using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Parallax : NetworkBehaviour
{
    public Transform[] layers; // Array of layers (BigBigMountain, PrettyBigBuilding, etc.)
    public float[] parallaxSpeeds; // Array of parallax speeds for each layer

    private Transform cam; // Reference to the main camera

    private Vector3 previousCamPos; // Store the camera's previous position

    void Start()
    {

        if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient != null)
        {
            // Assign the camera after ensuring the client is initialized
            AssignCamera();
        }
        else
        {
            // If not connected yet, listen for the connection event
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }


    private void OnClientConnected(ulong clientId)
    {
        // If the client is local, assign the camera
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            AssignCamera();
        }
    }

    private void AssignCamera()
    {
        GameObject cameraObject = GameObject.FindWithTag("MainCamera");

        if (cameraObject != null && IsLocalPlayer)
        {
            cam = cameraObject.transform;
        }
        else
        {
            Debug.Log("Player Camera not found for the Parallax script.");
            return;
        }

        // Store the previous frame's camera position
        previousCamPos = cam.position;


        // Optionally, you can remove the callback if it's no longer needed
      //  NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }


    void Update()
    {
        // Loop through each layer


        if (previousCamPos != null && cam != null)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                // Calculate the parallax effect
                float parallax = (previousCamPos.x - cam.position.x) * parallaxSpeeds[i];

                // Set the new position for each layer
                float targetPosX = layers[i].position.x + parallax;

                // Update the position of the layer
                layers[i].position = new Vector3(targetPosX, layers[i].position.y, layers[i].position.z);
            }

            // Update the previous camera position
            previousCamPos = cam.position;
        }
    
    }
}
