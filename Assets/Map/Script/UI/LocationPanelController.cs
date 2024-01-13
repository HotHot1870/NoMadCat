using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationPanelController : MonoBehaviour
{
    [SerializeField] private GameObject m_Self; 
    [SerializeField] private TMP_Text m_LocationName;   
    [SerializeField] private Transform m_EnemyBloackParent;  
    [SerializeField] private GameObject m_EnemyBloackPrefab; 
    [SerializeField] private Button m_DefenceBtn;
    [SerializeField] private Button m_CancelLocationDetailBtn;

    

    void Start(){
        m_DefenceBtn.onClick.AddListener(MapManager.GetInstance().GetMapUIController().OnClickDefence);
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

        
        MapManager.GetInstance().GetLocationController().SetLocationCameraPiority(10);

        m_Self.SetActive(true);
        m_DefenceBtn.gameObject.SetActive( !MapManager.GetInstance().GetLocationController().ShouldShowCorruption() );
        m_LocationName.text = locationData.DisplayName;

        // TODO : enemy list
        
        var allenemy = MainGameManager.GetInstance().GetAllEnemy();
        var allEnemyId = locationData.NormalWaveEnemy.Union<int>(locationData.FinalWaveEnemy).ToList<int>();
        
        for (int i = 0; i < m_EnemyBloackParent.childCount; i++)
        {
            Destroy(m_EnemyBloackParent.GetChild(i).gameObject);
        }
        foreach (var item in allEnemyId.Distinct())
        {
            var newEnemyBlock = Instantiate(m_EnemyBloackPrefab,m_EnemyBloackParent );
            var enemyScriptable = allenemy.Find(x=>x.Id==item);
            newEnemyBlock.GetComponent<EnemyBlockController>().Init(enemyScriptable);
        }

    }
}
