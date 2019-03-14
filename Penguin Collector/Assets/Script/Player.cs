using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector2 direction;
    [SerializeField] private float playerSpeed;

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = Vector2.up * Input.GetAxis("Vertical") * playerSpeed + Vector2.right * Input.GetAxis("Horizontal") * playerSpeed;
        animator.SetFloat("Speed", rigidbody2D.velocity.magnitude);
        spriteRenderer.flipX = direction.x < 0;
    }

    private void FixedUpdate()
    {
        rigidbody2D.velocity = direction;
    }
}
