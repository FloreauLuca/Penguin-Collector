using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector2 direction;
    private Vector2 orientation;
    private Vector2 inputDirection;
    [SerializeField] private float playerSpeed;

    private Rigidbody2D rigidbody2D;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int currentRegion = -1;
    public int CurrentRegion
    {
        get { return currentRegion; }
        set { currentRegion = value; }
    }


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

    [SerializeField] private AudioClip shot;
    [SerializeField] private AudioClip pain;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        penguinList = new List<Penguin>();
        Reward(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.UiManagerScript.CurrentFormat.joystick.InputDirection == Vector3.zero)
        {
            inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        else
        {
            inputDirection = GameManager.Instance.UiManagerScript.CurrentFormat.joystick.InputDirection;
        }

        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
        {
            orientation = Vector2.right * (inputDirection.x > 0 ? 1 : -1);
            if (inputDirection.x > 0)
            {
                animator.SetInteger("Direction", 1);
            }
            else if (inputDirection.x < 0)
            {
                animator.SetInteger("Direction", 3);

            }
        }
        else if (Mathf.Abs(inputDirection.x) < Mathf.Abs(inputDirection.y))
        {
            orientation = Vector2.up * (inputDirection.y > 0 ? 1 : -1);
            if (inputDirection.y > 0)
            {
                animator.SetInteger("Direction", 0);
            }
            else if (inputDirection.y < 0)
            {
                animator.SetInteger("Direction", 2);
            }
        }

        if (inputDirection != Vector2.zero)
        {

            direction = Vector2.up * inputDirection.y * playerSpeed + Vector2.right * inputDirection.x * playerSpeed;
        }
        else
        {
            direction = Vector2.zero;

        }
        
        /*
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > Mathf.Abs(Input.GetAxisRaw("Vertical")))
            {
                direction = Vector2.right * Input.GetAxis("Horizontal") * playerSpeed;
                orientation = Vector2.right * (Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1);
                if (Input.GetAxis("Horizontal") > 0)
                {
                    animator.SetInteger("Direction", 1);
                }
                else if (Input.GetAxis("Horizontal") < 0)
                {
                    animator.SetInteger("Direction", 3);

                }
            }
            else if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < Mathf.Abs(Input.GetAxisRaw("Vertical")))
            {
                direction = Vector2.up * Input.GetAxis("Vertical") * playerSpeed;
                orientation = Vector2.up * (Input.GetAxisRaw("Vertical") > 0 ? 1 : -1);
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
        */
        
        if (Input.GetButton("Fire") || GameManager.Instance.UiManagerScript.CurrentFormat.button.ButtonDown)
        {
            Fire();
        }
        else
        {
            fireTimer = 0;
        }
    }

    public void Fire()
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
        }
        else if (weaponType == WeaponType.AK)
        {
            if (fireTimer == 0)
            {
                Instantiate(akBallPrefab, transform.position + Vector3.up * 0.17f, Quaternion.Euler(0, 0, Mathf.Rad2Deg * (Mathf.Acos(orientation.y) * Mathf.Abs(orientation.y) - Mathf.Asin(orientation.x) * Mathf.Abs(orientation.x))));
                GetComponent<AudioSource>().PlayOneShot(shot);
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

            if (fireTimer == 0)
            {
                fireTimer++;
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
            GetComponent<AudioSource>().PlayOneShot(pain);
            StartCoroutine(Invincibility());
            GameManager.Instance.UiManagerScript.DisplayLife((int)life);
        }
        if (life <= 0)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
            invincibility = true;
            rigidbody2D.velocity = Vector2.zero;
            GameManager.Instance.GameOver();
            this.enabled = false;
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
