using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using UnityEngine.Rendering;

public class PlayerGUIManger : NetworkBehaviour
{

    // Health / Hp Handler
    [Header("Health Sliders")]
    public Slider DownHealthSlider;
    public Slider UpHealthSlider;

    [Header("Health Handler")]
    public float maxHealth;
    [SerializeField] TextMeshProUGUI UpHealthText;
    [SerializeField] TextMeshProUGUI DownHealthText;

    [Header("Health NetWorking")]
    public float PlayerHealth;

    //Level Handler
   [Header("Level Handler")]
    public double maxXp;
   [HideInInspector] public float currentXp;
    public float currentLevel;
    private float maxLevel;


    [SerializeField] float AngelcircleRadius;
    [SerializeField] Transform AngelspawnPosition;
    [SerializeField] GameObject LvlUpAngelPrefab;

    //...

    //Stamina Handler
    [Header("Stamina Handler")]
    public float CurrentMana;
    [SerializeField] private float MaxMana;

    [SerializeField] private Slider ManaBar;
    [SerializeField] private TextMeshProUGUI ManaTxt;


    [Header("Armor Handler")]
    public int CurrentArmor;
    private int MaxArmor;
    [SerializeField] private Slider ArmorSlider;
    [SerializeField] private TextMeshProUGUI ArmorTxt;

    [Header("Canvas")]
    [SerializeField] GameObject GUIBAR;
    [SerializeField] GameObject PlayerInv;
    [SerializeField] GameObject PlayerCanvases;


    #region Unity Methods
    public void Start()
    {
        PlayerHealth = maxHealth;




        if (IsOwner)
        {
            SetStaminaUi();
        }


        ActivateUI(); // Activate Player UI (Both Up And Down)
    }

    #endregion


    #region Health/Hp
    public void SetHealthUi() //(Setting Health Ui For The Start)
    {

        DownHealthSlider.minValue = 0f;
        DownHealthSlider.maxValue = maxHealth;

        UpHealthSlider.minValue = 0f;
        UpHealthSlider.maxValue = maxHealth;

        UpdateHealthUI(PlayerHealth);


    }
    public void UpdateHealthUI(float health) // Update The Health UI (If Theres Changes)
    {
        DownHealthSlider.value = health;
        UpHealthSlider.value = health;

        DownHealthText.text = health.ToString() + " / " + maxHealth.ToString();
        UpHealthText.text = health.ToString();
    }

    

    private void OnHealthChanged(float oldValue, float newValue) // Local Method Event
    {
        UpdateHealthServerRpc(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHealthServerRpc(float health)
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            UpdateHealthUI(health);

    }


    private void ActiveUI()
    {
        if (IsOwner)
        {
            SetHealthUi();
            UpHealthSlider.gameObject.SetActive(false);

            PlayerCanvases.SetActive(true);
        }

        else
        {
            DownHealthSlider.gameObject.SetActive(false);
            UpHealthSlider.gameObject.SetActive(true);
            PlayerCanvases.SetActive(false);

        }
    }
  
 

   
    public void ActivateUI()
    {
        ActiveUI();
       
    }

    #endregion

    #region Level Handler
    public void IncreaseXp(float Xp)
    {
        currentXp += Xp;

        LevelUp(currentXp);
    }

    public void LevelUp(float currentXpCheck)
    {
        if (currentXpCheck > maxXp)
        {
            currentLevel = (currentLevel < maxLevel) ? currentLevel + 1 : currentLevel; // Check if Current Level Is Less Then Max Level

            maxXp *= 1.25f; // Up The Max Xp By 25%

            currentXp = 0f; // Reset CurrentXp

            SpawnLvlUp(); // Spawn The Level Up Angel
        }
    }
    public void SpawnLvlUp()
    {
        KillEnemiesInCircleServerRpc();
     //   CameraShaker.Instance.ShakeOnce(2f, 1f, 0.1f, 2f);

    }

    //Level Up Angel Logic
    [ServerRpc(RequireOwnership = false)]
    public void KillEnemiesInCircleServerRpc() //Kill Enemies Via Radius
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(AngelspawnPosition.position, AngelcircleRadius);

        SpawnAngelAnim();

        foreach (Collider2D hitCollider in hitColliders)
        {
            // Check if the collider has the tag "Enemy"
            if (hitCollider.CompareTag("Enemy"))
            {

                EnemyLogic enemyLogic = hitCollider.GetComponentInChildren<EnemyLogic>();
                if (enemyLogic != null && !enemyLogic.EnemyIsDead)
                {

                    enemyLogic.EnemyTakeDamageServerRpc(enemyLogic.EnemyHealth, transform.position, 0f); //Kill Enemy Immediate

                    enemyLogic.SetPlayerWhoDamaged(this.transform.root.gameObject);

                }
            }
        }
    }

    public void SpawnAngelAnim() //Spawn The Angel
    {
        GameObject LevelUpVfx = Instantiate(LvlUpAngelPrefab, AngelspawnPosition.position, Quaternion.identity);

        NetworkObject LevelUpVfxNet = LevelUpVfx.GetComponent<NetworkObject>();

        LevelUpVfxNet.Spawn(true);

        StartCoroutine(DespawnNetObject(LevelUpVfxNet));

    }
    public IEnumerator DespawnNetObject(NetworkObject obj) // Despawning
    {
        yield return new WaitForSeconds(2.5f);
        obj.Despawn(true);
    }

    // Level Up Angel Logic End...


    #endregion


    #region Stamina Handler

    public void SetStaminaUi()
    {
        ManaBar.minValue = 0f;
        ManaBar.maxValue = MaxMana;


        CurrentMana = MaxMana;
        ManaBar.value = MaxMana;
        ManaTxt.text = MaxMana.ToString() + " / " + MaxMana.ToString();
    }

    public void SubtractMana(float minusMana)
    {

        if(CurrentMana > minusMana)
        {
            CurrentMana -= minusMana;

            ManaBar.value = CurrentMana;

            ManaTxt.text = CurrentMana.ToString() + " / " + MaxMana.ToString();
        }
      
    }



    #endregion

    #region Armor Handler

    public void EquipArmor(GameObject ArmorItem)
    {
    //    int currentArmorValue = int.Parse(ArmorTxt.text);

       // ArmorScript ArmorProtScript = ArmorItem.GetComponent<ArmorScript>();

        // Add the new armor protection value
     //   currentArmorValue += ArmorProtScript.ArmorProt;

        // Update the text with the new armor value as a string
      //  ArmorTxt.text = currentArmorValue.ToString();

        // Update the slider value accordingly
      //  ArmorSlider.value = currentArmorValue;
    }


    #endregion
}
