using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using Unity.VisualScripting;
public class PlayerMovement : NetworkBehaviour
{
    [Header("Moving")]
   [HideInInspector] public float moveInput = 0;
    public float LadderSpeed;


   [HideInInspector] public Rigidbody2D PlayerRb;

    private float moveSpeed;
    [HideInInspector] public bool CanMove = true;


    [Header("Dashing")]
    public float PlayerDashPower;
    [SerializeField] private float PlayerDashTimer;


    [Header("Crawling")]
    private bool IsCrawling = false;

    [Header("Components")]
    [HideInInspector] public Animator PlayerAnimator;
    private CircleCollider2D PlayerCircleCollider2D;

    [Header("Jumping")]
    public float jumpingForce;
    public Transform GroundedPosition;
    private bool isJumping = false;
    [HideInInspector] public bool isGrounded;

    [Header("Scripts")]
    PlayerLogic PlayerLogicScript;
    PlayerAttackingLogic playerAttackingLogicScript;
    PlayerGUIManger PlayerGUIMangerScript;
    PlayerStats PlayerStatsScript;


    [Header("Holders")]
    private Vector2 currentCircleOffset;
    private float currentRadius;

    [Header("Game Objects")]
    [SerializeField] private GameObject PlayerVisual;
    [HideInInspector] public List<SpriteRenderer> PlayerArmor;

    Camera PlayerCamera;

    [HideInInspector] public bool isFacingRight;

    private bool IsOnLadder;

    [Header("Dashing Particles")]
    [HideInInspector] public GameObject DashUpParticle;
    [HideInInspector] public GameObject DashForwardsParticle;

    [Header("Player Vfxs")]
    [SerializeField] private GameObject JumpParticle;

    private void Start()
    {
        GetComponenets();

        moveSpeed = PlayerStatsScript.currentSpeed;

        currentCircleOffset = PlayerCircleCollider2D.offset; // For Crawling

        currentRadius = PlayerCircleCollider2D.radius; //For Crawling

    }

    void Update()
    {
        if (!IsOwner) // If its the Local Player
            return;

        if (CanMove)
        {
            HandleCrawlingInput();
            MoveInput();
            MovePlayer();
            CheckMousPos();
            Jumping();
            PlayerDashJump();
            PlayerDashing();
            
        }
        isGrounded = Physics2D.OverlapCircle(GroundedPosition.position, 0.1f, LayerMask.GetMask("Ground"));
        if (IsOnLadder)
        {
            LadderMovement();
        }


        SetAnimations();
    }



    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            IsOnLadder = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            IsOnLadder = false;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsOnLadder && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            transform.position = new Vector3(transform.position.x, collision.gameObject.transform.position.y + 0.6f, transform.position.z);
        }
    }
    public void OnLadder()
    {
        Collider2D RopeCollider = Physics2D.OverlapCircle(transform.position, 0.4f, LayerMask.GetMask("Ladder"));

        transform.position = RopeCollider.bounds.center;

        PlayerRb.velocity = Vector2.zero;

        CanMove = false;

        PlayerRb.isKinematic = false;


    }

    public void LadderMovement()
    {
        bool Grounded = Physics2D.OverlapCircle(GroundedPosition.position, 0.1f, LayerMask.GetMask("Ground"));


        if (Input.GetKey(KeyCode.W) && IsOnLadder)
        {
            PlayerRb.velocity = new Vector2(0f, LadderSpeed); 

          
        }
        else if (Input.GetKey(KeyCode.W) && IsOnLadder)
        {
            PlayerRb.velocity = new Vector2(0f, -LadderSpeed);

            

        }




    }

   
    public void HandleCrawlingInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            IsCrawling = !IsCrawling;

            if (IsCrawling)
            {
                PlayerAnimator.Play("PlayerCrawl");
                PlayerCircleCollider2D.offset = new Vector2(0f, -0.1f);
                PlayerCircleCollider2D.radius = 0.000001f;
            }
            else
            {
                PlayerCircleCollider2D.offset = currentCircleOffset;
                PlayerCircleCollider2D.radius = currentRadius;
            }
        }
    }

    public void CheckMousPos()
    {
        Vector3 mousePosition = PlayerCamera.ScreenToWorldPoint(Input.mousePosition);

        if (mousePosition.x > transform.position.x)
        {
            isFacingRight = true;
            SetPlayerDirection(isFacingRight);
        }

        
        else
        {
            isFacingRight = false;
            SetPlayerDirection(isFacingRight);
        }
    }

    public void MoveInput()
    {
        moveInput = Input.GetAxis("Horizontal");
    }

    public void MovePlayer()
    {
        float speed = IsCrawling ? PlayerStatsScript.currentCrawlSpeed : moveSpeed;
        PlayerRb.velocity = new Vector2(moveInput * speed, PlayerRb.velocity.y);
    }

    public void Jumping()
    {
        bool ShouldJump = Input.GetKeyDown(KeyCode.Space);
        isGrounded = Physics2D.OverlapCircle(GroundedPosition.position, 0.1f, LayerMask.GetMask("Ground"));
        if (isGrounded && ShouldJump)
        {
            PlayerRb.AddForce(Vector2.up.normalized * jumpingForce, ForceMode2D.Impulse);
            StartCoroutine(JumpingParticle());

        }

        isJumping = !isGrounded;
    }


    public void PlayerDashJump()
    {
        bool ShouldDashJump = Input.GetKeyDown(KeyCode.LeftControl) && isGrounded;

        bool EnoughManaToDashJump = PlayerGUIMangerScript.CurrentMana > PlayerDashPower * 1.5f;

        if(ShouldDashJump && EnoughManaToDashJump)
        {
            Vector2 DashJumpPower = new Vector2(0f, PlayerDashPower * 3f);

            DashUpParticle = PlayerStatsScript.DashUpParticle;


            SpawnDashParticleUpServerRpc();

            PlayerRb.AddForce(DashJumpPower, ForceMode2D.Impulse);

            PlayerGUIMangerScript.SubtractMana(math.abs(PlayerDashPower));

        }

    }



    public void PlayerDashing()
    {

        bool ShouldDash = Input.GetKeyDown(KeyCode.LeftControl) && isJumping && !isGrounded;

        bool EnoughManaToDash = PlayerGUIMangerScript.CurrentMana > PlayerDashPower * 2f;

        if (ShouldDash && EnoughManaToDash)
        {
            float dashDirection;

            DashForwardsParticle = PlayerStatsScript.DashForwardParticle;

            if (isFacingRight)
            {
                dashDirection = 1;
            }
            else
                dashDirection = -1f;

            CanMove = false;

            SpawnDashParticleForwardServerRpc();

            PlayerRb.velocity = new Vector2(PlayerDashPower * dashDirection, PlayerRb.velocity.y);

            PlayerGUIMangerScript.SubtractMana(PlayerDashPower * 2f);

            StartCoroutine(CanMoveTimer());
        }
    }


    public IEnumerator CanMoveTimer()
    {

        yield return new WaitUntil(() => isGrounded);

        CanMove = true;
    }

    public void SetAnimations()
    {
        if (playerAttackingLogicScript.CurrentlyAttacking)
        {
            return;
        }

        bool ShouldWalkAnim = (moveInput != 0) && !isJumping && !IsCrawling;
        bool ShouldCrawlAnim = (moveInput != 0) && IsCrawling;

        if (IsCrawling && !ShouldCrawlAnim)
        {
            PlayerAnimator.Play("PlayerCrawl");
        }
        else if (ShouldCrawlAnim)
        {
            PlayerAnimator.Play("PlayerCrawlingMoving");
        }
        else if (ShouldWalkAnim)
        {
            PlayerAnimator.Play("PlayerWalk");
        }
        else if (isJumping)
        {
            PlayerAnimator.Play("PlayerJump");

        }
        else
        {
            PlayerAnimator.Play("PlayerIdle");
        }
    }


    public IEnumerator JumpingParticle()
    {
        JumpParticle.SetActive(true);
        yield return new WaitForSeconds(0.35f);
        JumpParticle.SetActive(false);
    }
    public void SetPlayerDirection(bool isFacingRight)
    {

        // Flip the sprites based on player direction
       
        // Flip the attacking transform based on player direction

        if (isFacingRight)
        {
            PlayerVisual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            playerAttackingLogicScript.AttackingTransform.rotation = Quaternion.Euler(0f, 0f, 0f);

            foreach(SpriteRenderer S in PlayerArmor)
            {
                S.flipX = true;
                S.gameObject.layer = LayerMask.NameToLayer("Player");

            }

        }
        else
        {
            // Facing left: flip the scale to negative
            PlayerVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 

            playerAttackingLogicScript.AttackingTransform.rotation = Quaternion.Euler(0f,180f,0f);

            foreach (SpriteRenderer S in PlayerArmor)
            {
                S.flipX = false;
                S.gameObject.layer = LayerMask.NameToLayer("Player");
            }
        }

        // Apply the updated scale
        SetAttackingTransform();

        
    }


    public void SetAttackingTransform()
    {
        Vector3 localScale = playerAttackingLogicScript.AttackingTransform.localScale;

        Vector2 localPosition = playerAttackingLogicScript.AttackingTransform.localPosition;

        localScale.x = isFacingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        localPosition.x = isFacingRight ? Mathf.Abs(localPosition.x) : -Mathf.Abs(localPosition.x);

        // Apply the updated values
        playerAttackingLogicScript.AttackingTransform.localScale = localScale;
        playerAttackingLogicScript.AttackingTransform.localPosition = localPosition;

    }

    public void GetComponenets()
    {
        PlayerLogicScript = GetComponent<PlayerLogic>();

        PlayerGUIMangerScript = GetComponent<PlayerGUIManger>();

        PlayerStatsScript = GetComponent<PlayerStats>();


        playerAttackingLogicScript = GetComponent<PlayerAttackingLogic>();
        PlayerRb = GetComponent<Rigidbody2D>();

        PlayerCircleCollider2D = GetComponent<CircleCollider2D>();

        PlayerAnimator = GetComponentInChildren<Animator>();

        PlayerCamera = GetComponentInChildren<Camera>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnDashParticleUpServerRpc()
    {
        GameObject PlayerDashUpParticle = Instantiate(DashUpParticle, transform.position, Quaternion.identity);


        NetworkObject DashJumpNet = PlayerDashUpParticle.GetComponent<NetworkObject>();

        DashJumpNet.Spawn();

       StartCoroutine(DespawnNetObject(DashJumpNet, 0.5f));
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnDashParticleForwardServerRpc()
    {
        GameObject PlayerDashForwardParticle = Instantiate(DashForwardsParticle, transform.position, Quaternion.identity);


        NetworkObject DashForwardNet = PlayerDashForwardParticle.GetComponent<NetworkObject>();

        DashForwardNet.Spawn();

       StartCoroutine(DespawnNetObject(DashForwardNet, 0.5f));
    }

    public IEnumerator DespawnNetObject(NetworkObject obj , float time)
    {
        yield return new WaitForSeconds(time);

        obj.Despawn(true);

        Destroy(obj.gameObject);
    }
    

}
