using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector2 direction;
    private Vector2 orientation;
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

    [SerializeField] private GameObject harpoonBallPrefab;
    [SerializeField] private GameObject akBallPrefab;

    private int fireTimer = 0;

    [Header("Statistique")]
    private float damage = 1;
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    [SerializeField] private float life;
    public float Life
    {
        get { return life; }
        set { life = value; }
    }

    [SerializeField] private int money;

    [SerializeField] private WeaponType weaponType = WeaponType.NOTHING;

    public enum WeaponType
    {
        NOTHING,
        HAMMER,
        SPEAR,
        HARPOON,
        AK
    }



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
            orientation = Vector2.right * Input.GetAxisRaw("Horizontal");
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
            orientation = Vector2.up * Input.GetAxisRaw("Vertical");
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
            fireTimer = 0;
        }

        if (Input.GetButton("Fire"))
        {
            direction = Vector2.zero;
            
            
            if (weaponType == WeaponType.HARPOON)
            {
                if (fireTimer == 0)
                {
                    Instantiate(harpoonBallPrefab, transform.position + Vector3.up * 0.17f, Quaternion.Euler(0, 0, Mathf.Rad2Deg * (Mathf.Acos(orientation.y) * Mathf.Abs(orientation.y) - Mathf.Asin(orientation.x) * Mathf.Abs(orientation.x))));
                    fireTimer++;
                    animator.SetTrigger("Attack");
                }
            } else if (weaponType == WeaponType.AK)
            {
                if (fireTimer == 0)
                {
                    Instantiate(akBallPrefab, transform.position + Vector3.up * 0.17f, Quaternion.Euler(0, 0, Mathf.Rad2Deg * (Mathf.Acos(orientation.y) * Mathf.Abs(orientation.y) - Mathf.Asin(orientation.x) * Mathf.Abs(orientation.x))));
                    fireTimer++;
                    animator.SetTrigger("Attack");
                }
                else if (fireTimer == 10)
                {
                    fireTimer = 0;
                }
                else
                {
                    fireTimer++;
                }
            }
            else
            {

                animator.SetTrigger("Attack");
            }
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
        if (weaponType != WeaponType.AK)
        {
            weaponType++;
        }

        switch (weaponType)
        {
            case WeaponType.NOTHING:
                animator.SetInteger("Weapon", 0);
                break;
            case WeaponType.HAMMER:
                animator.SetInteger("Weapon", 1);
                break;
            case WeaponType.SPEAR:
                animator.SetInteger("Weapon", 2);
                break;
            case WeaponType.HARPOON:
                animator.SetInteger("Weapon", 3);
                break;
            case WeaponType.AK:
                animator.SetInteger("Weapon", 4);
                break;
        }
    }
}
