using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using EZCameraShake;
using static QuestsManager;
using System.Linq;

public class PlayerAttackingLogic : NetworkBehaviour
{
    [Header("VfxSlash")]
    private GameObject SlashParticle;
    public GameObject DefaultSlashParticle;


    [Header("Player Scripts")]
    private PlayerMovement PlayerMovementScript;
    private PlayerLogic PlayerLogicScript;
    private WeaponScript PlayerWeaopon;
    private PlayerGUIManger PlayerGUIMangerScript;
    private PlayerStats PlayerStatsScript;



    [Header("Player Animators")]
    private Animator PlayerAnimator;



    [Header("AttackingDistance")]
    public Transform AttackingTransform;
    public Vector2 attackDistance = new Vector2(1.0f, 1.0f);


    [Header("Player Logic")]
    public bool CurrentlyAttacking = false;
    private float attackingCoolDown;

    [Header("Player Stats Logic")]
    public float PlayerknockBackForce; // For The Enemy

    private float PlayerAttackingDamage;




    private void Start()
    {
        GetComponenets();
        SetComponenets();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;


        Swing();
        AttackingTimer();
    }

    public void AttackingTimer()
    {
        if (CurrentlyAttacking)
        {

            attackingCoolDown -= Time.deltaTime;
            if (attackingCoolDown <= 0)
            {
                CurrentlyAttacking = false;
                attackingCoolDown = 0.1f;
            }

        }
    }

    private void GetComponenets()
    {
        PlayerLogicScript = GetComponent<PlayerLogic>();

        PlayerGUIMangerScript = GetComponent<PlayerGUIManger>();

        PlayerAnimator = GetComponent<Animator>();

        PlayerStatsScript = GetComponent<PlayerStats>();

        PlayerMovementScript = GetComponent<PlayerMovement>();

        PlayerWeaopon = GetComponentInChildren<WeaponScript>();

        
    }

    private void SetComponenets()
    {
        PlayerAttackingDamage = PlayerStatsScript.currentDamage;
    }

    public void IntiliazeWeapon(GameObject Weapon)
    {
      WeaponScript PlayerWeaopon = Weapon.GetComponent<WeaponScript>();

        if (PlayerWeaopon != null)
        {
            SlashParticle = PlayerWeaopon.WeaopenVfx; // Setting The Vfx

            PlayerStatsScript.currentDamage = PlayerWeaopon.WeaponDamage; // Setting Player Damage + The Weaopen Damage

            PlayerAttackingDamage += PlayerWeaopon.WeaponDamage;

            attackingCoolDown = PlayerWeaopon.WeaponAttackSpeed; // Seting The CoolDown

            attackDistance = PlayerWeaopon.WeaopenAttackDistance; //Setting The Weapon Attack Distance

            PlayerknockBackForce = PlayerWeaopon.WeaponKnockBack; //Setting The KnockBack Of The Weapon
        }
    }

    #region Attacking
    public void Swing()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !CurrentlyAttacking)
        {
            
            CurrentlyAttacking = true;

            Quaternion slashRotation = AttackingTransform.rotation;

            

            ServerSpawnSlashEffectServerRpc(AttackingTransform.position, AttackingTransform.rotation); // Spawn Slash Particles

            EnemyInsideRange(); // Check if Theres Enemy Inside The Attacking Distance

            AttackingTimer(); // Attack CoolDown

        }

    }

    [ServerRpc (RequireOwnership = false)]
    private void ServerSpawnSlashEffectServerRpc(Vector3 position, Quaternion rotation) //Spawning The Slash Particle VIA NetWork
    {

        GameObject SwordSlash1Spawn = Instantiate(SlashParticle != null ? SlashParticle : DefaultSlashParticle, position, rotation);

            var networkObject = SwordSlash1Spawn.GetComponent<NetworkObject>();
            networkObject.Spawn(true);  // Spawn with owner

            StartCoroutine(DespawnNetObject(SwordSlash1Spawn));
       

     
    }



    public void EnemyInsideRange()
        {
        if (CurrentlyAttacking)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(AttackingTransform.position, attackDistance, 0f);


            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyLogic enemyLogic = enemy.GetComponent<EnemyLogic>();
                bool EnemyInRange = enemyLogic != null && !enemyLogic.EnemyIsDead;

                if (EnemyInRange)
                {
                    //   CameraShaker.Instance.ShakeOnce(0.55f, 0.35f, 0.1f, 0.75f);
                    DamageEnemy(enemyLogic);
                    enemyLogic.SetPlayerWhoDamaged(this.transform.root.gameObject);

                    break;
                }

            }
        }
        else
            Debug.Log("Not Found");
    }

    #endregion

    #region Net Related

    public void DamageEnemy(EnemyLogic enemyLogic)
    {
        float knockbackForce = PlayerknockBackForce;


        if (!enemyLogic.EnemyIsDead)
            enemyLogic.EnemyTakeDamageServerRpc(PlayerAttackingDamage, transform.position, knockbackForce);
    }

    public IEnumerator DespawnNetObject(GameObject NetObject)
    {

        yield return new WaitForSeconds(0.3f);

        DestroyImmediate(NetObject, true);
    }

    #endregion

    #region DrawingGizmos
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawCube(AttackingTransform.position,new Vector3(0.5f,0.5f,0.5f));

    }

    #endregion
}