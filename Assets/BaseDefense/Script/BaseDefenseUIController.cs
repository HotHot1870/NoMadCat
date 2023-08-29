using System.Collections;
using System.Collections.Generic;
using ExtendedButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseDefenseUIController : MonoBehaviour
{
    [Header("ShootingPanel")]
    [SerializeField] private GameObject m_ShootPanel;
    [SerializeField] private TMP_Text m_AmmoText;
    [SerializeField] private Button m_ReloadBtn;
    [SerializeField] private Button m_OptionBtn;
    [SerializeField] private Button2D m_AimBtn;    
    [SerializeField] private Button2D m_ShootBtn;
    [SerializeField] private Button2D m_LookDownBtn;
    [SerializeField] private Button2D m_LookUpBtn;

    
    [Header("OptionPanel")]
    [SerializeField] private GameObject m_OptionPanel;



    private void Start() {
        var crosshairController = BaseDefenseManager.GetInstance().GetCrosshairController();

        m_AimBtn.onDown.AddListener(crosshairController.OnAimBtnDown);
        m_AimBtn.onUp.AddListener(crosshairController.OnAimBtnUp);
        m_AimBtn.onExit.AddListener(crosshairController.OnAimBtnUp);

        var gunShootController = BaseDefenseManager.GetInstance().GetGunShootController();

        m_ReloadBtn.onClick.AddListener(gunShootController.OnClickReload);

        m_ShootBtn.onDown.AddListener(gunShootController.OnShootBtnDown);
        m_ShootBtn.onUp.AddListener(gunShootController.OnShootBtnUp);
        m_ShootBtn.onExit.AddListener(gunShootController.OnShootBtnUp);
        
        m_OptionBtn.onClick.AddListener(()=>{
            m_OptionPanel.SetActive(true);
        });

        var cameraController = BaseDefenseManager.GetInstance().GetCameraController();

        m_LookDownBtn.onDown.AddListener(() =>
            cameraController.OnClickLookDownBtn(
                OnClickLookDown
            ));

        m_LookUpBtn.onDown.AddListener(() =>
            cameraController.OnClickLookUpBtn(
                OnClickLookUp
            ));
            
        m_LookUpBtn.gameObject.SetActive(false);
    }

    private void OnClickLookUp(){
        m_ShootPanel.SetActive(true);
        m_LookUpBtn.gameObject.SetActive(false);
    }

    private void OnClickLookDown(){
        m_ShootPanel.SetActive(false);
        m_LookUpBtn.gameObject.SetActive(true);
    }
}
