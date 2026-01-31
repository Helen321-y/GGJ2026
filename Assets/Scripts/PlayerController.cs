using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    [Header("Component")]
    private Rigidbody2D _rb;

    private Animator anim;
    
    private CapsuleCollider2D _collider2D;

    [SerializeField] private LayerMask whatIsGround;

    [Header("Movement")]
    private float _movementInputDirection;
    [SerializeField] private float speed = 7f;
    private bool _isFacingRight = true;
    private bool canMove = true;
    private float _facingDirection;
    
    [Header("Jump")]
    private float jumpForce = 16.0f;
    private bool canDoubleJump;
    private bool getDoubleJumpMask;

    [Header("Fast Fall")]
    [SerializeField] private float fastFallSpeed = -25f;      
    [SerializeField] private float normalFallClamp = -30f;    
    private bool hasFastFall = false;                         
    private bool fastFallEquipped = false;   

    [Header("Detections")]
    private bool isGrounded;
    [SerializeField] private float groundCheckDistance;



    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        _collider2D = GetComponent<CapsuleCollider2D>();

    }


    private void Update()
    {
        
        CollisionCheck();
        CheckMovementDirection();
        CheckInput();

        if(canMove)
        {
            CheckMovementDirection();
            ApplyMovement();
        }

        if (isGrounded)
        {
            canMove = true;

            if(getDoubleJumpMask)
                canDoubleJump = true;
            else
                canDoubleJump = false;
        }

        
        HandleFastFall();
        
    }


    private void CheckInput()
    {
        _movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            JumpTrigger();
        }
        
    }

    private void CheckMovementDirection()
    {
       if(canMove)
        {
            if(_isFacingRight && _movementInputDirection < 0)
            {
                Flip();
            }

            if(!_isFacingRight && _movementInputDirection > 0)
            {
                Flip();
            }
        } 
    }

    private void Flip()
    {
        _facingDirection = _facingDirection * -1;
        _isFacingRight = !_isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    private void ApplyMovement()
    {
        if (canMove)
        {
            float moveInput = speed * _movementInputDirection;
            _rb.velocity = new Vector2(moveInput, _rb.velocity.y);
        }
    }


    private void JumpTrigger()
    {
        if(isGrounded)
        {
            Jump();
        }
        else if (getDoubleJumpMask && canDoubleJump)
        {
            canDoubleJump = false;
            Jump();
            canMove = false;
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        isGrounded = false;
    }

    public void EnableDoubleJump()
    {
        getDoubleJumpMask = true;
        canDoubleJump = true;
    }

     private void HandleFastFall()
    {
       if (!hasFastFall || !fastFallEquipped) return;
    if (isGrounded) return;

    if (_rb.velocity.y < 0f)
    {
        if (_rb.velocity.y > fastFallSpeed)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, fastFallSpeed);
        }

        if (_rb.velocity.y < normalFallClamp)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, normalFallClamp);
        }
    }
    }

        public void UnlockFastFall()
    {
        hasFastFall = true;
        fastFallEquipped = true;
    }

    private void CollisionCheck()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - groundCheckDistance));
    }


}
