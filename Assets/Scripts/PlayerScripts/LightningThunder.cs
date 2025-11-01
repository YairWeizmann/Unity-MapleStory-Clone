using EZCameraShake;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
public class LightningThunder : SkillBase
{
    [Header("Skill")]
    public string PlayerAnimatorSkillName = "LightningThunder(Skill)";
    public GameObject LightningThunderVfx;
    public float RayLength;
    public float SkillDamage;

    [Header("Player")]
    GameObject PlayerGM;
    public void SkillDoes(GameObject PlayerGameObject)
    {
        PlayerMovement playerMovement = PlayerGameObject.GetComponent<PlayerMovement>();
        Animator Playeranimator = PlayerGameObject.GetComponent<Animator>();
        PlayerGM = PlayerGameObject;

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
        PlayerMoveScript.PlayerRb.velocity = Vector2.zero;
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


        // Raycast to detect all enemies
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, rayLength, ~LayerMask.GetMask("IgnoreRaycast"));

        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Enemy")) // Check if the hit object has the "Enemy" tag
                {

                    EnemyLogic enemy = hit.collider.gameObject.GetComponentInChildren<EnemyLogic>();
                    if (enemy != null)
                    {

                        GameObject vfx = Instantiate(LightningThunderVfx, enemy.transform.position, Quaternion.identity);
                      //  CameraShaker.Instance.ShakeOnce(3f, 1f, 0.1f, 0.35f);

                        DestroyGameObject(vfx)
                            ;

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

    

    [ServerRpc(RequireOwnership = false)]
    public void SpawnLightningOnEnemy(NetworkObject obj)
    {
        GameObject Enemy = obj.gameObject;

        GameObject vfx = Instantiate(LightningThunderVfx, Enemy.transform.position, Quaternion.identity);

        NetworkObject networkObject = vfx.GetComponent<NetworkObject>();

        networkObject.Spawn(true);
    }

    public void DestroyGameObject(GameObject Obj)
    {
        Destroy(Obj, 0.65f);
    }

}
