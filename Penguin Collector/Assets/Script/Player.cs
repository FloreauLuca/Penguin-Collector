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

    private int currentRoom = -1;
    public int CurrentRoom
    {
        get { return currentRoom; }
        set { currentRoom = value; }
    }

    private bool invincibility;

    private List<Penguin> penguinList;
    public List<Penguin> PenguinList
    {
        get { return penguinList; }
        set { penguinList = value; }
    }

    

    [Header("Statistique")]
    public float damage;
    public float life;
    public int money;




    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        penguinList = new List<Penguin>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal"))>0)
        {
            direction = Vector2.right * Input.GetAxis("Horizontal") * playerSpeed;
            if (Input.GetAxis("Horizontal") > 0)
            {
                animator.SetInteger("Direction", 1);
            } else if (Input.GetAxis("Horizontal") < 0)
            {
                animator.SetInteger("Direction", 3);

            }
        } else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0)
        {
            direction = Vector2.up * Input.GetAxis("Vertical") * playerSpeed;
            if (Input.GetAxis("Vertical") > 0)
            {
                animator.SetInteger("Direction", 0);
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                animator.SetInteger("Direction", 2);
            }
        }
        else
        {
            direction = Vector2.zero;
        }
        
        if (Input.GetButtonDown("Fire"))
        {
            animator.SetTrigger("Attack");
        }
    }

    private void FixedUpdate()
    {
        rigidbody2D.velocity = direction;
        animator.SetBool("Walk", rigidbody2D.velocity.magnitude > 0.1);
    }

    public void Hit(float hitDamage)
    {
        if (!invincibility)
        {
            life -= hitDamage;
        }

        if (life < 0)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
            invincibility = true;
            rigidbody2D.velocity = Vector2.zero;
            this.enabled = false;
        }
        else
        {
            StartCoroutine(Invincibility());
        }
    }

    IEnumerator Invincibility()
    {
        invincibility = true;
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);
        yield return new WaitForSeconds(1f);
        invincibility = false;
        spriteRenderer.color = new Color(1, 1, 1, 1);

    }

    public void Reward(int price)
    {
        money += price;
        if (money < 5)
        {
            animator.SetInteger("Weapon", money);
        }
    }
}
