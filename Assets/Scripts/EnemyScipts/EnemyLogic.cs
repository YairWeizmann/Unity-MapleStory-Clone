using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using System.Linq;
using static QuestsManager;
using TMPro;

public enum EnemyType
{
    BlueSnail,
    GreenMushroom
}
public class EnemyLogic : NetworkBehaviour
{
    [Header ("Enemy Type")]
    public EnemyType enemyType;

    [Header("Enemy Attack")]
    public float AttackDamage;

    [Header("Enemy Health")]
    public float EnemyHealth;

    [Header("Enemy Xp")]
    public int EnemyXp;

    [Header("Private")]
    public NetworkVariable<bool> GotHit = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
   [HideInInspector] public bool EnemyIsDead = false;

    [Header("Animators")]
    private Animator EnemyAnimator;
    private int EnemyHitLayerIndex;

    [Header("Enemy Componenets")]
    private Rigidbody2D EnemyRb;
    private SpriteRenderer spriteRenderer;

   [Header("Effects")]
   [SerializeField] private Material FlashMatriel;
   [SerializeField] private TextMeshProUGUI PlayerDamageText; 
   private Material originalMatriel;
   [SerializeField] Canvas ShowDamageCanvas;


    [Header("KnockBack")]
    public float EnemyKnockBackForce; // For The Player

    [Header("Enemy Item Drops")]
    List<GameObject> ItemDrops = new List<GameObject>();

    [Header("Mob Drop")]
    public GameObject MobDrop;

    [Header("Scripts")]
    EnemyMovement EnemyMovementScript;


    //Local Host Methods (SinglePlayer)

    private void Start()
    {
        GetComponents();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

    

        if (!GotHit.Value && !EnemyIsDead)
            EnemyAnimator.Play("EnemyMoving");

    }
    private void GetComponents()
    {
        EnemyAnimator = GetComponentInChildren<Animator>();

        EnemyMovementScript = GetComponent<EnemyMovement>();

        EnemyRb = GetComponentInParent<Rigidbody2D>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ShowDamageCanvas = GetComponentInChildren<Canvas>();

        originalMatriel = spriteRenderer.material;

        EnemyHitLayerIndex = EnemyAnimator.GetLayerIndex("CombatLayer");
    }
    public void SetPlayerWhoDamaged(GameObject PlayerObj)
    {
        PlayerLogic PlayerLogicScript = PlayerObj.GetComponent<PlayerLogic>();
        CheckForActiveKillingQuest(this, PlayerLogicScript);

    }


    [ServerRpc(RequireOwnership = false)]
    public void EnemyTakeDamageServerRpc(float PlayerDamage, Vector3 playerPosition ,float PlayerKnockBackForce)
    {
        GotHit.Value = true;
        NetworkObject EnemyNet = GetComponentInParent<NetworkObject>();
        if (EnemyNet.IsSpawned)
        {
            EnemyHealth -= PlayerDamage;

            if (EnemyHealth <= 0)
            {
                EnemyIsDead = true;
                EnemyDies();

               
            }

            else if (GotHit.Value && !EnemyIsDead)
            {
                EnemyKnockBackServerRpc(playerPosition, PlayerKnockBackForce);
                SetDamageText(PlayerDamage);
                StartCoroutine(SetFlash());
            }
        }

     
    }

    public void EnemyDies() // Local Method For Enemy Death
    {
        if (EnemyIsDead)
        {
            EnemyMovementScript.enabled = false;

            EnemyAnimator.Play("EnemyDie");



            float DieAnimationTime = 0.35f;

            Invoke(nameof(EnemyDiesServerRpc),DieAnimationTime);

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnemyKnockBackServerRpc(Vector3 PlayerPos , float PlayerKnockBackForce)
    {
        EnemyAnimator.SetLayerWeight(EnemyHitLayerIndex, 1);

        EnemyAnimator.Play("EnemySquash");

        Invoke(nameof(HitLayerBackToZero), 0.35f);

        EnemyAnimator.Play("EnemyGotHit");

            EnemyMovementScript.enabled = false;

            Vector2 direction = (transform.position - PlayerPos).normalized;

            // Add an upward component to the direction for the jump effect
            Vector2 knockbackDirection = new Vector2(direction.x, direction.y + 0.65f).normalized;

            EnemyRb.AddForce(knockbackDirection * PlayerKnockBackForce, ForceMode2D.Impulse);

            Invoke(nameof(EnableMovement), 0.4f);
        
           
        
    }


    private IEnumerator SetFlash()
    {
        spriteRenderer.material = FlashMatriel;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.material = originalMatriel;
    }

    public void SetDamageText(float PlayerDamage)
    {
        TextMeshProUGUI playerDamageSpawn = Instantiate(PlayerDamageText,transform.position,Quaternion.identity);
        Debug.Log("Spanwed");
        playerDamageSpawn.transform.SetParent(ShowDamageCanvas.transform,false);
        playerDamageSpawn.text = PlayerDamage.ToString();
        DestoryGameObjectTime(playerDamageSpawn.gameObject, 0.45f);
    }

    public IEnumerator DestoryGameObjectTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    public void HitLayerBackToZero()
    {
        EnemyAnimator.SetLayerWeight(EnemyHitLayerIndex, 0);

    }




    [ServerRpc(RequireOwnership =false)]

    public void SpawnMobDropServerRpc()
    {
        GameObject MobDropSpawn = Instantiate(MobDrop, transform.position,Quaternion.identity);

        var networkObject = MobDropSpawn.GetComponent<NetworkObject>();

        if(networkObject != null)
        networkObject.Spawn(true);  // Spawn with owner

    }


    [ServerRpc(RequireOwnership = false)]

    public void EnemyDiesServerRpc()
    {
        if (IsServer)
        {
            NetworkObject thisEnemyNet = GetComponentInParent<NetworkObject>();
            if (thisEnemyNet.IsSpawned)
            {
                SpawnMobDropServerRpc();
                Invoke(nameof(DespawnNetObjectEnemy),0.15f);
            }
            else
                Debug.Log("Not Spawed");

        }
    }


    public void DespawnNetObjectEnemy()
    {
        NetworkObject enemyNet = GetComponentInParent<NetworkObject>();
        if (enemyNet.IsSpawned)
        {
            enemyNet.Despawn(true); // Only despawn if still spawned
        }
    }

    
   


    public void EnableMovement()
    {
        EnemyMovementScript.enabled = true;
        GotHit.Value = false;
    }

    #region Quests

    public void CheckForActiveKillingQuest(EnemyLogic enemyLogic , PlayerLogic PlayerLogicScript)
    {
        if (PlayerLogicScript.activeQuests.Any(quest => quest is KillQuest))
        {
            EnemyType KilledEnemyType = enemyLogic.enemyType;

            foreach (var quest in PlayerLogicScript.activeQuests)
            {
                if (quest is KillQuest killQuest && killQuest.enemyToKill == KilledEnemyType)
                {
                    killQuest.KilledEnemies(EnemyIsDead);
                    break;
                }
            }

        }
    }

    #endregion
}
