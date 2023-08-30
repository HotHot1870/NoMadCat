using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseDefenseNameSpace;
using Unity.Collections;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] private Transform m_EnemyParent;
    [SerializeField] private WavesScriptable m_WavesData ;
    [SerializeField] private Transform m_GroundSpawnerCenter;
    [SerializeField] private float m_GroundSpawnerWidth = 40;

    private int m_WaveCount = 0;
    private float m_TimePassed = 0;
    private List<GameObject> m_AllSpawnedEnemy = new List<GameObject>();

    private void Start() {
        StartNormalWave();
    }

    private void StartNormalWave(){
        float dangerValue = m_WavesData.NormalWavesDangerValue;
        while (dangerValue >0)
        {
            var targetEnemy = m_WavesData.NormalWaveEnemy[Random.Range(0,m_WavesData.NormalWaveEnemy.Count)];
            StartCoroutine( SpawnEnemy(Random.Range(0f,10f), targetEnemy) );  
            dangerValue -= targetEnemy.DangerValue;
        }
        m_WaveCount++;

    }


    private IEnumerator SpawnEnemy(float delay, EnemyScriptable enemyData){
        float passedTime = 0;
        while (passedTime < delay)
        {
            passedTime += Time.deltaTime;
            yield return null;
        }

        var newEnemy = Instantiate(enemyData.Prefab, m_EnemyParent);
        newEnemy.transform.position = m_GroundSpawnerCenter.position + 
            Random.Range(m_GroundSpawnerWidth*-1f,m_GroundSpawnerWidth) * Vector3.right;


    }



}
