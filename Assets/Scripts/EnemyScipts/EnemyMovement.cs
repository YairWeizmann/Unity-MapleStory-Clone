using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class EnemyMovement : NetworkBehaviour
{
    [Header("Moving")]
    public float enemyMoveSpeed;

    [Header("Components")]
    private Transform EnemyPos;

    [Header ("Animations")]
    private Animator EnemyAnimator;
    private int hitLayerIndex;


    [Header("Enemy Points")]
    [SerializeField] Transform PointA;
    [SerializeField] Transform PointB;


    [Header("Player References")]
    private Transform PlayerPos;

    [Header("Enemy AI")]
    [HideInInspector] public bool SawPlayer;


    private bool movingTowardsA=true;

    private void Start()
    {
        EnemyPos = GetComponent<Transform>();
        EnemyAnimator = GetComponentInChildren<Animator>();

    }

    
   private void Update()
    {
        
        EnemyPatroling();
    }

    public void FacingWhichDir(bool IsFacingRight)
    {

        if (IsFacingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Assuming 2D rotation

        }
        else
            transform.rotation = Quaternion.Euler(0f, 180, 0f);

    }
  

    public void EnemyPatroling()
    {
        bool ReachedPointA = Vector3.Distance(EnemyPos.transform.position, PointA.transform.position) < 0.1f;

        bool ReachedPointB = Vector3.Distance(EnemyPos.transform.position, PointB.transform.position) < 0.1f;

        bool ShouldFollowPlayer = SawPlayer && PlayerPos != null;

        if (ShouldFollowPlayer)
        {

            Vector3 targetPosition = new Vector3(PlayerPos.position.x, EnemyPos.position.y, EnemyPos.position.z);
            EnemyPos.position = Vector2.MoveTowards(EnemyPos.position, targetPosition, enemyMoveSpeed * Time.deltaTime);

            float directionToPlayer = Mathf.Sign(PlayerPos.position.x - EnemyPos.position.x);

            // Update the enemy's facing direction
            FacingWhichDir(directionToPlayer  < 0);
            return;
        }
        ;

        if (movingTowardsA)
        {
            EnemyPos.position += Vector3.left * enemyMoveSpeed * Time.deltaTime;

            if (ReachedPointA)
            {
                movingTowardsA = false;
                FacingWhichDir(false);
                
            }
        }
        else if(!movingTowardsA) 
        {
            EnemyPos.position += Vector3.right * enemyMoveSpeed * Time.deltaTime;

            if (ReachedPointB)
            {
                movingTowardsA = true;
                FacingWhichDir(true);
               
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SawPlayer = true;
            PlayerPos = collision.GetComponent<Transform>();
        }
    }

    public void OnDrawGizmos()
    {


        Gizmos.DrawSphere(PointA.position, 0.4f);

        Gizmos.DrawSphere(PointB.position, 0.4f);


        Gizmos.DrawLine(PointA.position, PointB.position);
    }
}
