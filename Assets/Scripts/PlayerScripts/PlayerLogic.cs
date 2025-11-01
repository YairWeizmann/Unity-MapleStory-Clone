using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Mathematics;
using static QuestsManager;

public class PlayerLogic : NetworkBehaviour
{
    [Header("Componenets")]
    private Rigidbody2D PlayerRb; //Player RigidBody
    private Animator PlayerAnimator;


    [Header("Scripts")]
    private PlayerMovement PlayerMovmentScript;
    private WeaponScript PlayerWeaponScript;
    private PlayerGUIManger PlayerGUIMangerScript;
    private PlayerStats PlayerStatsScript;

    [Header("Enemy Componenets")]
    private Transform EnemyPos; // Enemy Position For KnockBack Handling

    [Header("KnockBack & Hit Handler")]
    private float CurrentEnemyknockBackForce; // For The Player


    [Header("Player Logic")]
    private bool IsInvicible = false;
    private int hitLayerIndex;

    [Header("Player Quests")]
    public List<QuestsBase> activeQuests = new List<QuestsBase>(); // List to hold active quests



    [Header("Player Arm")]
    [SerializeField] private GameObject PlayerArmPrefab;

    [HideInInspector] public NetworkVariable<NetworkObjectReference> playerArmReferenceNetworkVar = new NetworkVariable<NetworkObjectReference>();

    [Header("Portals Manager")]
   [HideInInspector] public bool ShouldEnterPortal;
    private SceneLoaderManager SceneLoaderManagerScript;

    [Header("Loading Screen")]
    [HideInInspector] public GameObject LoadingScreen;
    
    
    private void Start()
    {
        GetPlayerComponenets();

   

        if(IsOwner)
            SetPlayerArmServerRpc();

    }


    //Collision Methods
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyPos = collision.GetComponentInChildren<Transform>();

            EnemyLogic EnemyLogicScript = collision.GetComponent<EnemyLogic>();

            CurrentEnemyknockBackForce = EnemyLogicScript.EnemyKnockBackForce;

            PlayerKnockBack();


            if (!IsInvicible)
                PlayerTakeDamageServerRpc(EnemyLogicScript.AttackDamage);

        }

       
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            ShouldEnterPortal = false;
            Debug.Log(ShouldEnterPortal);

        }
    }



    //Logic Methods

    public void GetPlayerComponenets()
    {
        PlayerRb = GetComponent<Rigidbody2D>(); // Player Rb

        PlayerMovmentScript = GetComponent<PlayerMovement>(); // Player Movement Script

        PlayerGUIMangerScript = GetComponent<PlayerGUIManger>();

        PlayerStatsScript = GetComponent<PlayerStats>();

        SceneLoaderManagerScript = FindObjectOfType<SceneLoaderManager>();


        PlayerAnimator = GetComponent<Animator>(); // Player Animator


        hitLayerIndex = PlayerAnimator.GetLayerIndex("GotHitLayer"); // Player Hit Layer (For Red Effect);

    }

    

    [ServerRpc (RequireOwnership = false)]
    public void SetPlayerArmServerRpc() //Spawn The Player Arm In The Player Hiearachy
    {
        GameObject PlayerArmSpawned = Instantiate(PlayerArmPrefab , transform.position , quaternion.identity);
       

        if (PlayerArmPrefab == null || PlayerArmSpawned == null)
        {
            Debug.LogError("No Arm");

            return;
        }


        if (PlayerArmSpawned != null)
        {
            NetworkObject PlayerArmNet = PlayerArmSpawned.GetComponent<NetworkObject>();

            NetworkObject PlayerNet = gameObject.GetComponent<NetworkObject>();

            PlayerArmNet.Spawn(true);

            playerArmReferenceNetworkVar.Value = PlayerArmNet;

            PlayerArmSpawned.transform.SetParent(transform.gameObject.transform, true);

          

            SyncItemTransformClientRpc(PlayerArmNet.NetworkObjectId, PlayerNet.NetworkObjectId);



            
        }
    }

    [ClientRpc]
    public void SyncItemTransformClientRpc(ulong itemNetworkId, ulong parentNetworkId) //Let the clients know the correct position of the Equipped Item
    {
        NetworkObject itemNet = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkId];
        NetworkObject parentNet = NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentNetworkId];

        if (itemNet != null && parentNet != null)
        {
            itemNet.transform.SetParent(parentNet.transform, false);
            itemNet.transform.localPosition = Vector3.zero;
            itemNet.transform.localRotation = quaternion.identity;


        }

      
    }
    

    //Timer Methods



    public void HitLayerBackToZero()
    {
        PlayerAnimator.SetLayerWeight(hitLayerIndex, 0);

    }
    public void PlayerCanMove()
    {
        PlayerMovmentScript.CanMove = true;
    }



    // Damage Methods
    public void PlayerKnockBack()
    {
        Vector2 knockBackDirection = (transform.position - EnemyPos.position).normalized;

        PlayerAnimator.SetLayerWeight(hitLayerIndex, 1);

        Invoke("HitLayerBackToZero", 0.35f);

        PlayerMovmentScript.CanMove = false;

        PlayerRb.AddForce(knockBackDirection * CurrentEnemyknockBackForce, ForceMode2D.Impulse);



        Invoke(nameof(PlayerCanMove), 0.5f);

    }


    public IEnumerator PlayerShouldTakeDamage()
    {
        IsInvicible = true;
        yield return new WaitForSeconds(0.5f);
        IsInvicible = false;
    }

 
    //Server Methods



    [ServerRpc(RequireOwnership = false)]
    public void PlayerTakeDamageServerRpc(float EnemyDamage)
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            float PlayerHealth = PlayerGUIMangerScript.PlayerHealth; // Get The Player Health From The Gui Script

            if (PlayerHealth > 0)
            {
                PlayerHealth -= EnemyDamage;
                PlayerGUIMangerScript.PlayerHealth = PlayerHealth;
                PlayerGUIMangerScript.UpdateHealthServerRpc(PlayerHealth);
                StartCoroutine(PlayerShouldTakeDamage());
            }

            else if (PlayerHealth - EnemyDamage <= 0)
            {
                PlayerDeathClientRpc(true);
            }
        }
        
    }

    
    [ClientRpc(RequireOwnership = false)]
    public void PlayerDeathClientRpc(bool IsDead)
    {
        if (IsDead)
        {
            PlayerMovmentScript.enabled = false;

            PlayerAnimator.Play("PlayerDie");
            PlayerRb.velocity = Vector2.zero;

            Invoke(nameof(DespawnNetObjectServerRpc), 0.55f);

        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void DespawnNetObjectServerRpc()
    {
        if (IsServer)
        {
            NetworkObject ThisGameObjectNet = gameObject.GetComponent<NetworkObject>();

            if (ThisGameObjectNet != null)
            {
                ThisGameObjectNet.Despawn(true);
                Destroy(this.gameObject, 0.1f);
            }
        }
    }

    #region ShopMarketRelated



    #endregion


    #region LoadingScreen
    public void EnableLoadingScreen()
    {
        LoadingScreen.SetActive(true);
    }
    public void DisableLoadingScreenAnim()
    {
        Animator LoadScreenAnim = LoadingScreen.GetComponent<Animator>();
        LoadScreenAnim.Play("UnloadingScreen");
        Invoke(nameof(DisableLoadingScreen), 2f);
    }
    public void DisableLoadingScreen()
    {
        LoadingScreen.SetActive(false);
    }
    #endregion
}
