using EZCameraShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowNight : MonoBehaviour
{
    [Header("Skill")]
    public string PlayerAnimatorSkillName = "ShadowNight(Skill)";
    public GameObject ShadowNightVfx;
    public float SkillDamage;
    public float RayLength;
    [Header("Player")]
    GameObject PlayerGM;

    public void SkillDoes(GameObject PlayerGameObject)
    {
        PlayerMovement playerMovement = PlayerGameObject.GetComponent<PlayerMovement>();
        Animator Playeranimator = PlayerGameObject.GetComponent<Animator>();
        PlayerGM = PlayerGameObject;

        StartCoroutine(SetSkillAnimation(Playeranimator));
        StartCoroutine(CanMove(0.5f, playerMovement));
        StartCoroutine(LaserCast(PlayerGM.transform));
    }
    public IEnumerator SetSkillAnimation(Animator PlayerAnimator)
    {
        PlayerAnimator.Play(PlayerAnimatorSkillName.ToString());
        yield return new WaitForSeconds(1f);

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
        yield return new WaitForSeconds(0.1f);

        Vector2 origin = PlayerPos.position;
        origin.y = origin.y - 0.25f;
        Vector2 direction = PlayerPos.transform.right;
        float rayLength = RayLength;


        // Raycast to detect all enemies
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, rayLength, ~LayerMask.GetMask("IgnoreRaycast"));
        Instantiate(ShadowNightVfx, PlayerPos.position, Quaternion.identity);
        // CameraShaker.Instance.ShakeOnce(4f, 1f, 0.1f, 0.35f);

        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Enemy")) // Check if the hit object has the "Enemy" tag
                {

                    EnemyLogic enemy = hit.collider.gameObject.GetComponentInChildren<EnemyLogic>();
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

    public void DestroyGameObject(GameObject Obj)
    {
        Destroy(Obj, 0.65f);
    }
}
