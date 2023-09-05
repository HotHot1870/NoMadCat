using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GunModelComtroller : MonoBehaviour
{
    [SerializeField] private Transform m_ModelAim;
    [SerializeField] private Transform m_ModelShake;
    [SerializeField] private Vector3 m_CrosshairOffsetStrength = Vector3.one;
    private GameObject m_GunModel;
    private Vector3 m_ModelStartPos;
    private Vector3 m_PosOffset = Vector3.zero;
    private Coroutine m_ShakeRecover = null;



    private void Start() {
        BaseDefenseManager.GetInstance().m_ChangeToShootAction += ShowFPSGunModel;
        BaseDefenseManager.GetInstance().m_ChangeFromShootAction += HideFPSGunModel;
        BaseDefenseManager.GetInstance().m_ShootUpdateAction += ShootUpdate;
        m_ModelStartPos = m_ModelAim.position;
    }

    private void ShootUpdate() {
        
        Ray ray = Camera.main.ScreenPointToRay(BaseDefenseManager.GetInstance().GetCrosshairPos());
        RaycastHit hit;
        // hit Environment
        if (Physics.Raycast(ray, out hit, 500, 1<<10))
        {
            m_ModelAim.LookAt(hit.point);
        }

        GunModelParentOffsetHandler();

    }

    public void ShakeGunByShoot(float shakeAmount){
        if(m_ShakeRecover != null){
            StopCoroutine(m_ShakeRecover);
        }
        m_ShakeRecover = StartCoroutine(ShakeGun(shakeAmount));
    }

    private IEnumerator ShakeGun(float shakeAmount){
        Vector3 randomRot = new Vector3(
            UnityEngine.Random.Range(-10f,10f),
            UnityEngine.Random.Range(-10f,10f),
            UnityEngine.Random.Range(-10f,10f)
        ) * shakeAmount ;

        Vector3 randomPos = new Vector3(
            UnityEngine.Random.Range(-0.5f,-0.5f),
            UnityEngine.Random.Range(0f,1f),
            UnityEngine.Random.Range(-1.5f,0f)
        ) * shakeAmount ;
        m_ModelShake.localEulerAngles = randomRot;
        m_ModelShake.localPosition = randomPos;

        float timePass = 0;
        float recoverTime = 0.15f;
        while (recoverTime>timePass)
        {
            timePass += Time.deltaTime;
            yield return null;
            // shake recover
            m_ModelShake.localEulerAngles = Vector3.Lerp(randomRot, Vector3.zero,timePass/recoverTime );
            m_ModelShake.localPosition = Vector3.Lerp(randomPos, Vector3.zero,timePass/recoverTime );
        }
        m_ModelShake.localEulerAngles = Vector3.zero;
        m_ModelShake.localPosition = Vector3.zero;
    }

    public void HideFPSGunModel(){
        m_ModelAim.gameObject.SetActive(false);
    }

    private void ShowFPSGunModel(){
        m_ModelAim.gameObject.SetActive(true);
    }

    public void ChangeGunModel(GunScriptable gun){
        if(m_GunModel != null){
            Destroy(m_GunModel);
        }
        m_GunModel = Instantiate(gun.FPSPrefab,m_ModelShake);
        var gunTrans = m_GunModel.transform;
        m_PosOffset = gun.FPSPos;
        gunTrans.localEulerAngles = gun.FPSRot;
        gunTrans.localScale = gun.FPSScale;
        
    }

    private void GunModelParentOffsetHandler(){
        GunModelOffset(BaseDefenseManager.GetInstance().GetCrosshairController().GetCrosshairToScreenOffsetNormalized());

    }

    private void GunModelOffset(Vector2 crosshairPosNormalized){
        // x offset 
        var startPos = new Vector3(
            m_ModelStartPos.x + Mathf.Clamp(crosshairPosNormalized.x,0,10f) *3f,
            m_ModelStartPos.y,
            m_ModelStartPos.z
        );

        m_ModelAim.position = startPos + new Vector3(
            crosshairPosNormalized.x * m_CrosshairOffsetStrength.x,
            crosshairPosNormalized.y * m_CrosshairOffsetStrength.y,
            crosshairPosNormalized.y*-1f * m_CrosshairOffsetStrength.z
        ) ;
        m_ModelAim.localPosition += m_PosOffset;

    }

}

