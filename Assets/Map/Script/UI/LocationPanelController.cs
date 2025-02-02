using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtendedButtons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationPanelController : MonoBehaviour
{
    [SerializeField] private GameObject m_Self; 
    [SerializeField] private TMP_Text m_LocationName;   
    [SerializeField] private Transform m_EnemyBlockParent;  
    [SerializeField] private GameObject m_EnemyBlockPrefab; 
    [SerializeField] private Button2D m_DefenceBtn;
    [SerializeField] private Button2D m_CancelLocationDetailBtn;

    [Header("Mutation")]
    [SerializeField] private Image m_HpImage; 
    [SerializeField] private TMP_Text m_HpPresentage;  
    [SerializeField] private Image m_DamageImage; 
    [SerializeField] private TMP_Text m_DamagePresentage;  
    [SerializeField] private Image m_SpeedImage; 
    [SerializeField] private TMP_Text m_SpeedPresentage;  

    

    void Start(){

        
        MainGameManager.GetInstance().AddOnClickBaseAction(m_DefenceBtn, m_DefenceBtn.GetComponent<RectTransform>());
        m_DefenceBtn.onClick.AddListener(MapManager.GetInstance().GetMapUIController().OnClickDefence);


        
        MainGameManager.GetInstance().AddOnClickBaseAction(m_CancelLocationDetailBtn, m_CancelLocationDetailBtn.GetComponent<RectTransform>());
        m_CancelLocationDetailBtn.onClick.AddListener(CancelLocationDetail);
    }

    public void TurnOffPanel(){
        m_Self.SetActive(false);
    }

    
    private void CancelLocationDetail() {
        TurnOffPanel();
        MapManager.GetInstance().GetLocationController().SetLocationCameraPiority(0);
        
    }

    

    public bool ShouldShowLocationDetail(){
        return !m_Self.activeSelf;
    }

    public void Init(){
        MapLocationScriptable locationData = MapManager.GetInstance().GetLocationController().GetScriptable();
        if(locationData==null || m_Self.activeSelf)
            return;


        // mutation
        m_HpImage.color = Color.Lerp(Color.white,Color.red, Mathf.Clamp01(locationData.HealthMutation/100f) );
        m_HpPresentage.text=100f+locationData.HealthMutation+"%";  
        m_DamageImage.color = Color.Lerp(Color.white,Color.red, Mathf.Clamp01(locationData.DamageMutation/100f) );
        m_DamagePresentage.text=100f+locationData.DamageMutation+"%";    
        m_SpeedImage.color = Color.Lerp(Color.white,Color.red, Mathf.Clamp01(locationData.SpeedMutation/100f) );; 
        m_SpeedPresentage.text=100f+locationData.SpeedMutation+"%";   
    
        MapManager.GetInstance().GetLocationController().SetLocationCameraPiority(10);

        m_Self.SetActive(true);
        m_DefenceBtn.gameObject.SetActive( !MapManager.GetInstance().GetLocationController().ShouldShowCorruption() );
        m_LocationName.text = locationData.DisplayName;

        // enemy list
        
        var allenemy = MainGameManager.GetInstance().GetAllEnemy();
        var allEnemyId = locationData.NormalWaveEnemy.Union<int>(locationData.FinalWaveEnemy).ToList<int>();
        
        for (int i = 0; i < m_EnemyBlockParent.childCount; i++)
        {
            Destroy(m_EnemyBlockParent.GetChild(i).gameObject);
        }
        foreach (var item in allEnemyId.Distinct())
        {
            var newEnemyBlock = Instantiate(m_EnemyBlockPrefab,m_EnemyBlockParent );
            var enemyScriptable = allenemy.Find(x=>x.Id==item);
            newEnemyBlock.GetComponent<EnemyBlockController>().Init(enemyScriptable);
        }
    }
}
