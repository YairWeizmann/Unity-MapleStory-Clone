using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;
using EZCameraShake;

public class HolySpiritSkill : SkillBase
{
    [Header("Skill")]
    public string PlayerAnimatorSkillName = "PlayerHolyLight(Skill)";
    public GameObject HolySpiritSkillVfx;
    public float RayLength;
    public float SkillDamage;

    private bool Fly =false;
    [Header("Player")]
    GameObject PlayerGM;


    public void SkillDoes(GameObject PlayerGameObject)
    {
            PlayerMovement playerMovement = PlayerGameObject.GetComponent<PlayerMovement>();
            Animator  Playeranimator = PlayerGameObject.GetComponent<Animator>();
            PlayerGM = PlayerGameObject;
            Fly = true;

        StartCoroutine(SetSkillAnimation(Playeranimator));
            StartCoroutine(CanMove(1.5f, playerMovement));
            StartCoroutine(LaserCast(PlayerGameObject.transform));
        
    }
    public IEnumerator SetSkillAnimation(Animator PlayerAnimator)
    {
        PlayerAnimator.Play(PlayerAnimatorSkillName.ToString());
        yield return new WaitForSeconds(2f);

    }

    public IEnumerator CanMove(float CanMoveTime, PlayerMovement PlayerMoveScript)
    {
        PlayerMoveScript.enabled = false;

        
        yield return new WaitForSeconds(CanMoveTime);

        PlayerMoveScript.enabled = true;
    }
    public IEnumerator LaserCast(Transform PlayerPos)
    {
        yield return new WaitForSeconds(1F);

        Vector2 origin = PlayerPos.position;
        origin.y = origin.y - 0.25f;
        Vector2 direction = PlayerPos.transform.right;
        float rayLength = RayLength;

        SpawnLaserServerRpc();

        // Raycast to detect all enemies
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, rayLength, ~LayerMask.GetMask("IgnoreRaycast"));

        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Enemy")) // Check if the hit object has the "Enemy" tag
                {
                    Debug.Log("Hit object: " + hit.collider.gameObject.name);

                    EnemyLogic enemy = hit.collider.GetComponentInChildren<EnemyLogic>();
                    if (enemy != null)
                    {

                        enemy.EnemyTakeDamageServerRpc(SkillDamage, Vector2.zero, 0f);

                        enemy.SetPlayerWhoDamaged(PlayerGM);

                    }
                }
            }
        }
        else
        {
            Debug.Log("No enemies hit.");
        }

        Debug.DrawRay(origin, direction * rayLength, Color.red, 1f);
    }

    public void Update()
    {
        if (Fly)
        {
            StartFlying();
            Debug.Log("Flying");
            StartCoroutine(DontFly());
        }
    }

    public void StartFlying()
    {
      if(PlayerGM == null)
        {
            return;
        }

            Rigidbody2D playerRb = PlayerGM.GetComponent<Rigidbody2D>();
            PlayerMovement playerMovement = PlayerGM.GetComponent<PlayerMovement>();
        
       

        if (playerMovement != null)
        {
            playerMovement.CanMove = false; // Prevents the player from walking while flying
        }
        if(playerRb != null)
        {
            playerRb.gravityScale = 0; // Neutralize gravity to make the player float
            playerRb.velocity = new Vector2(0f, 0.3f); // Adjust the Y value for the desired flying speed

        }
        // Apply a constant upward force to simulate flying
       
    }

    public IEnumerator DontFly()
    {
        yield return new WaitForSeconds(1.5f);
        Fly = false;
        if (PlayerGM == null)
        {
            yield return null;
        }

        Rigidbody2D playerRb = PlayerGM.GetComponent<Rigidbody2D>();
        PlayerMovement playerMovement = PlayerGM.GetComponent<PlayerMovement>();

        playerMovement.CanMove = true;
        playerRb.gravityScale = 4f;

    }

    [ServerRpc(RequireOwnership =false)]
    public void SpawnLaserServerRpc()
    {

        GameObject LaserVfx = Instantiate(HolySpiritSkillVfx,PlayerGM.transform.position,Quaternion.identity);
        CameraShaker.Instance.ShakeOnce(1f, 2f, 0.25f, 0.5f);
        NetworkObject VfxNet = LaserVfx.GetComponent<NetworkObject>();
        VfxNet.Spawn();
    }


}
