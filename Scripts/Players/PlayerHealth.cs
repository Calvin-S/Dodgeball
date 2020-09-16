using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
public class PlayerHealth : NetworkBehaviour
{
    public int maxHealth = 100;
	private int currentHealth;
    public bool isSinglePlayer = true;
    [SyncVar] public string displayName = "";
	public HealthBar healthBar;
    public HealthBar shieldBar;
    [SerializeField] private TMP_Text displayUI;
    public bool isDead;

    public int maxShield = 100;
    [SerializeField] private int shield = 0;

    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        if (displayName == "")
            displayName = PlayerName.playerName;
		currentHealth = maxHealth;
		healthBar.SetMaxHealth(maxHealth);
        shieldBar.SetMaxHealth(maxShield);
        shieldBar.SetHealth(shield);
        if (displayUI == null)
            Debug.LogError("please assign a UI for name display");
        else
            displayUI.text = displayName;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSinglePlayer && currentHealth <= 0) 
            Destroy(gameObject);
        else if (!isSinglePlayer && isLocalPlayer && currentHealth <= 0) {
            death();
            CmdDeath();
        }

    }

    public int getShield() { return shield; }
    public int getHealth() { return currentHealth;}

    public void addShield(int s) {
        if (s < 0)
            return;
        if (s + shield > maxShield)
            shield = maxShield;
        else
            shield = s + shield;
        shieldBar.SetHealth(shield);
    }

    public void setShield(int s) {
        shield = s;
        shieldBar.SetHealth(shield);
    }
    
	public void TakeDamage(int damage)
	{
		currentHealth -= damage;
		healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0 && !isSinglePlayer) {
            death();
            if (isServer)
                RpcDeath();
            else 
                CmdDeath();
        }
	}

    public void Heal(int healing) {
        currentHealth = currentHealth + healing > maxHealth ? maxHealth : currentHealth + healing;
        healthBar.SetHealth(currentHealth);
    }

    public void disableHealthBar() {
        gameObject.transform.Find("Canvas").gameObject.SetActive(false);
    }

    public void EnableHealthBar() {
        gameObject.transform.Find("Canvas").gameObject.SetActive(true);
    }

    [Command]
    public void CmdDeath() {
        RpcDeath();
    }

    [ClientRpc]
    void RpcDeath() {
        if (!isLocalPlayer) {
            death();
        }
    }

    public void death() {
        if (isDead) return;
        else {
            gameObject.GetComponent<PlayerAbilities>().death();
            gameObject.GetComponent<PlayerHealth>().disableHealthBar();
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            gameObject.GetComponent<PlayerShoot>().enabled = false;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<Rigidbody2D>().Sleep();
            if (gameObject.GetComponent<PolygonCollider2D>() != null)
                gameObject.GetComponent<PolygonCollider2D>().enabled = false;
            else if (gameObject.GetComponent<BoxCollider2D>() != null) 
                gameObject.GetComponent<BoxCollider2D>().enabled = false;

            if (isLocalPlayer) {
                GameObject.Find("Main Camera").GetComponent<CameraFollow>().follow = false;
                GameObject.Find("Main Camera").GetComponent<CameraFollow>().cantMove = false;
                CmdDeath();
            }
            NetworkManage nm = NetworkManager.singleton as NetworkManage;
            nm.addDeath();
            this.enabled = false;
            isDead = true;
        }
    }
}
