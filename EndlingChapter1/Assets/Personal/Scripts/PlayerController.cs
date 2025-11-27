using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;

    [SerializeField] float stunDuration = 0.2f;
    [SerializeField] bool controllable = true;
    float stunTimer;

    Vector2 moveInput;
    Vector2 moveVelocity;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if(controllable == false)
        {
            moveInput = Vector2.zero;
            moveVelocity = Vector2.zero;
            UpdateAnimations();
            return;
        }

        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            moveInput = Vector2.zero;
            moveVelocity = Vector2.zero;
            UpdateAnimations();
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y);

        if (moveInput.sqrMagnitude > 1f)
            moveInput = moveInput.normalized;

        moveVelocity = moveInput * moveSpeed;

        UpdateAnimations();

        if (x > 0)
            FlipSprite(true);
        else if (x < 0)
            FlipSprite(false);
    }

    void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + moveVelocity * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    void UpdateAnimations()
    {
        if (animator == null)
            return;

        animator.SetFloat("Vertical", moveInput.y);
        animator.SetBool("Horizontal", moveInput.x != 0);
    }

    void FlipSprite(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void TriggerStun(float duration)
    {
        stunTimer = duration;
    }

    public void TriggerStun()
    {
        stunTimer = stunDuration;
    }

    public void SetControllable(bool value)
    {
        controllable = value;
    }
}
