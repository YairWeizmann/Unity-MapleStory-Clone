using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
public class GameManagment : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab1;
    [SerializeField] private GameObject enemyPrefab2;

    private int Rnd;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Rnd = Random.Range(0, 2);



            SpawnEnemy(Vector3.zero);

        }
    }

    public void SpawnEnemy(Vector3 Position)
    {
        if (IsServer)
        {
            if (Rnd == 1)
            {
                GameObject enemy = Instantiate(enemyPrefab1, Position, Quaternion.identity);
                NetworkObject networkObject = enemy.GetComponent<NetworkObject>();

                networkObject.Spawn(true);
            }
            else if (Rnd == 0)
            {
                GameObject enemy = Instantiate(enemyPrefab2, Position, Quaternion.identity);
                NetworkObject networkObject = enemy.GetComponent<NetworkObject>();

                networkObject.Spawn(true);
            }
        
        }
           
       


    }
}
