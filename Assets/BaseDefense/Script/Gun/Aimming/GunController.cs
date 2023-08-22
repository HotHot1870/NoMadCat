using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtendedButtons;
using TMPro;
using System.Linq;
using BaseDefenseNameSpace;
using UnityEngine.Rendering.Universal;

public class GunController : MonoBehaviour
{
    private GunScriptable m_SelectedGun = null;
    [SerializeField] private SpriteRenderer m_FPSImage;
    [SerializeField] private GameObject m_Self;
    [SerializeField] private Vector3 m_GunFpsImagePos = new Vector3(8, -4.5f, 0);



    [Header("Aim effect for gun")]
    [SerializeField] private Transform m_GunModel;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI m_AmmoText;
    private float m_CurrentAmmo = 0;
    private Dictionary<int, float> m_GunsClipAmmo = new Dictionary<int, float>(); // how many ammo left on gun when switching

    [Header("Shooting")]
    [SerializeField] private GameObject m_ShotPointPrefab; // indicate where the shot land 
    [SerializeField] private Transform m_ShotDotParent;
    [SerializeField] private AudioSource m_ShootAudioSource;
    [SerializeField] private Button2D m_ShootBtn;
    private float m_CurrentShootCoolDown = 0; // must be 0 or less to shoot 
    private Coroutine m_SemiAutoShootCoroutine = null;

    [Header("Reload")]
    [SerializeField] private Button m_ReloadBtn;

    [Header("Switch Weapon")]
    [SerializeField] private List<WeaponToBeSwitch> m_AllWeaponSlot = new List<WeaponToBeSwitch>();
    private int m_CurrentWeaponSlotIndex = 0;




    private void Start()
    {
        BaseDefenseManager.GetInstance().m_ChangeToShootAction += ShowWeaponModel;
        BaseDefenseManager.GetInstance().m_ChangeFromShootAction += HideWeaponModel;
        BaseDefenseManager.GetInstance().m_UpdateAction += ShootCoolDown;
        MainGameManager.GetInstance().AddNewAudioSource(m_ShootAudioSource);

        var allSelectedWeapon = MainGameManager.GetInstance().GetAllSelectedWeapon();

        int soltIndex = 0;
        for (int i = 0; i < allSelectedWeapon.Count; i++)
        {
            int index = i;
            if (allSelectedWeapon[i] != null)
            {
                m_AllWeaponSlot[soltIndex].m_Gun = allSelectedWeapon[i];
                m_AllWeaponSlot[soltIndex].m_SpriteRenderer.sprite = allSelectedWeapon[i].DisplaySprite;
                m_AllWeaponSlot[soltIndex].m_SlotIndex = index;
                m_GunsClipAmmo.Add(i, 0);
            }else{
                m_AllWeaponSlot[soltIndex].m_Gun = null;
                m_AllWeaponSlot[soltIndex].m_SpriteRenderer.color = Color.clear;
                m_AllWeaponSlot[soltIndex].m_SlotIndex = 0;
            }
            soltIndex++;

            // TODO : check slot owned in main game manager 
            if (soltIndex >= m_AllWeaponSlot.Count)
                break;

        }
        if (allSelectedWeapon == null || allSelectedWeapon.Count <= 0)
        {
            Debug.LogError("No Selected Weapon");
        }
        else
        {
            // select first usable gun
            for (int i = 0; i < m_AllWeaponSlot.Count; i++)
            {
                if(m_AllWeaponSlot[i].m_Gun != null){
                    BaseDefenseManager.GetInstance().SwitchSelectedWeapon(m_AllWeaponSlot[i].m_Gun,i);
                    m_CurrentWeaponSlotIndex = i;
                    break;
                }
            }
        }


        m_SemiAutoShootCoroutine = null;

        m_ReloadBtn.onClick.AddListener(() =>
        {
            if (IsFullClipAmmo())
                return;

            GunReloadControllerConfig gunReloadConfig = new GunReloadControllerConfig
            {
                GunScriptable = m_SelectedGun,
                GainAmmo = GainAmmo,
                SetAmmoToFull = SetClipAmmoToFull,
                SetAmmoToZero = SetClipAmmoToZero,
                IsFullClipAmmo = IsFullClipAmmo,
            };
            BaseDefenseManager.GetInstance().StartReload(gunReloadConfig);
        });



        m_ShootBtn.onDown.AddListener(() =>
        {

            if (m_CurrentAmmo <= 0)
            {
                m_ShootAudioSource.PlayOneShot(m_SelectedGun.OutOfAmmoSound);
            }
            else
            {
                m_SemiAutoShootCoroutine = null;
                if (m_SemiAutoShootCoroutine == null && m_SelectedGun.IsSemiAuto)
                {
                    m_SemiAutoShootCoroutine = StartCoroutine(SemiAutoShoot());
                    return;
                }
                Shoot();
            }

        });
        m_ShootBtn.onUp.AddListener(() =>
        {
            if (m_SemiAutoShootCoroutine != null)
            {
                StopCoroutine(m_SemiAutoShootCoroutine);
                m_SemiAutoShootCoroutine = null;
            }
        });

        m_ShootBtn.onExit.AddListener(() =>
        {
            if (m_SemiAutoShootCoroutine != null)
            {
                StopCoroutine(m_SemiAutoShootCoroutine);
                m_SemiAutoShootCoroutine = null;
            }
        });

        ChangeAmmoCount(0, true);
        m_FPSImage.sprite = m_SelectedGun.FPSSprite;
        //var crossHairworldPos = Camera.main.ScreenToWorldPoint(m_CrossHair.position);
        var screenCenter = new Vector3(Screen.width/2f,Screen.height/2f,0);
        //MoveCrossHair( screenCenter );
    }

    private void HideWeaponModel()
    {
        m_FPSImage.gameObject.SetActive(false);
    }

    private void ShowWeaponModel()
    {
        m_FPSImage.gameObject.SetActive(true);
    }

    private void ShootCoolDown()
    {
        // fire rate
        m_CurrentShootCoolDown -= Time.deltaTime;
    }

    public void SetSelectedGun(GunScriptable gun, int slotIndex)
    {
        if (m_SelectedGun != null)
            m_GunsClipAmmo[m_CurrentWeaponSlotIndex] = m_CurrentAmmo;

        m_SelectedGun = gun;
         m_CurrentWeaponSlotIndex = slotIndex;

        BaseDefenseManager.GetInstance().SetAccruacy(m_SelectedGun.Accuracy);
        m_SemiAutoShootCoroutine = null;
        ChangeAmmoCount(m_GunsClipAmmo[slotIndex], true);
        m_FPSImage.sprite = m_SelectedGun.FPSSprite;
    }
    private void GainAmmo(int changes)
    {
        ChangeAmmoCount(changes, false);
    }

    private bool IsFullClipAmmo()
    {
        return m_CurrentAmmo >= m_SelectedGun.ClipSize;
    }

    private void SetClipAmmoToZero()
    {
        ChangeAmmoCount(0, true);
    }

    private void SetClipAmmoToFull()
    {
        ChangeAmmoCount(m_SelectedGun.ClipSize, true);
    }
    private IEnumerator SemiAutoShoot()
    {
        while (m_CurrentAmmo > 0)
        {
            Shoot();
            yield return null;
        }

        m_ShootAudioSource.PlayOneShot(m_SelectedGun.OutOfAmmoSound);
    }



    private void Shoot()
    {
        if (m_CurrentShootCoolDown > 0)
            return;

        // shoot sound
        m_ShootAudioSource.PlayOneShot(m_SelectedGun.ShootSound);

        // move cross hair up 
        /*
        m_CrossHair.position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), 0)
            * Mathf.Lerp(0, Screen.height / 100, (100 - m_SelectedGun.RecoilControl) / 100);*/

        //CrossHairOutOfBoundPrevention();
/*
        float targetCrossHairScale = Mathf.InverseLerp(100, 0, m_CurrentAccruacy);
        float targetMaxRadius = Mathf.Lerp(0, m_CrossHairMaxSize / 2 - m_CrossHairMinSize / 2, targetCrossHairScale);*/
        for (int j = 0; j < m_SelectedGun.PelletPerShot; j++)
        {
            // random center to point distance
            
            float randomDistance = Random.Range(0, 100f-BaseDefenseManager.GetInstance().GetAccruacy());
            float randomAngle = Random.Range(0, 360f);
            Vector3 accuracyOffset = new Vector3(
                Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance,
                Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance,
                0
            );


            // spawn dot for player to see
            var shotPoint = Instantiate(m_ShotPointPrefab,m_ShotDotParent);
            shotPoint.GetComponent<RectTransform>().position = accuracyOffset + BaseDefenseManager.GetInstance().GetCrosshairPos();

            
            // acc lose on shoot            
            BaseDefenseManager.GetInstance().SetAccruacy(
                BaseDefenseManager.GetInstance().GetAccruacy()-100+m_SelectedGun.RecoilControl
            );
            /*
            shotPoint.transform.SetParent(m_ShootDotParent);
            var targetPosForPoint = Camera.main.ScreenToWorldPoint(m_CrossHair.position + accuracyOffset);
            shotPoint.GetComponent<RectTransform>().position = m_CrossHair.position + accuracyOffset;*/
            Destroy(shotPoint, 1);

            /*RaycastHit2D[] hits = Physics2D.RaycastAll(targetPosForPoint - Vector3.forward * targetPosForPoint.z, Vector2.zero);
            List<EnemyBodyPart> hitedEnemy = new List<EnemyBodyPart>();
            for (int i = 0; i < hits.Length; i++)
            {
                hits[i].collider.TryGetComponent<EnemyBodyPart>(out var enemyBodyPart);
                if (enemyBodyPart != null)
                {
                    hitedEnemy.Add(enemyBodyPart);
                }
            }
            if (hitedEnemy.Count > 0)
            {
                shotPoint.GetComponent<ShootDotController>().OnHit();
                var sortedEnemies = hitedEnemy.OrderBy(x => x.GetDistance()).ToList();
                sortedEnemies[0].OnHit(m_SelectedGun.Damage);
                if (sortedEnemies[0].IsCrit())
                {
                    shotPoint.GetComponent<ShootDotController>().OnCrit();
                }
            }
            else
            {
                shotPoint.GetComponent<ShootDotController>().OnMiss();
            }*/
        }



        //m_CurrentAccruacy -= (100 - m_SelectedGun.RecoilControl);

        m_CurrentShootCoolDown = 1 / m_SelectedGun.FireRate;
        ChangeAmmoCount(-1, false);
    }

    private void ChangeAmmoCount(float num, bool isSetAmmoCount = false)
    {
        if (isSetAmmoCount)
        {
            m_CurrentAmmo = num;
        }
        else
        {
            m_CurrentAmmo += num;
        }
        if (m_CurrentAmmo > m_SelectedGun.ClipSize)
        {
            m_CurrentAmmo = m_SelectedGun.ClipSize;
        }

        m_AmmoText.text = $"{m_CurrentAmmo} / {m_SelectedGun.ClipSize}";
    }


/*
    private void CrossHairOutOfBoundPrevention()
    {
        if (m_CrossHair.position.x < 0)
        {
            // Left out of bound
            m_CrossHair.position = new Vector3(0, m_CrossHair.position.y, 0);
        }

        if (m_CrossHair.position.x > Screen.width)
        {
            // Right out of bound
            m_CrossHair.position = new Vector3(Screen.width, m_CrossHair.position.y, 0);
        }


        if (m_CrossHair.position.y > Screen.height)
        {
            // Top out of bound
            m_CrossHair.position = new Vector3(m_CrossHair.position.x, Screen.height, 0);
        }


        if (m_CrossHair.position.y < 0)
        {
            // Down out of bound
            m_CrossHair.position = new Vector3(m_CrossHair.position.x, 0, 0);
        }
    }*/


    private void OnDestroy()
    {

    }
}
