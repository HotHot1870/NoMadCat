using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppeteerController : EnemyControllerBase
{ 
    [SerializeField] private GameObject m_Self;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private float m_AttackStartUp = 0.45f;
    [SerializeField] private GameObject m_Ghost;
    [SerializeField] private GameObject m_PuppetPrefab;
    [SerializeField] private EnemyScriptable m_GhostScriptable;
    [SerializeField] private EnemyScriptable m_PuppetScriptable;
    [SerializeField] private GameObject m_Electic;
    [SerializeField] private float m_AttackDelayPerSpawn = 1f;
    private PuppetController m_PuppetController=null;
    protected float m_AttackDelay = 0;
    private int m_SpawnCount = 0 ;

    public override void Init(EnemyControllerInitConfig config)
    {
        base.Init(config);
        
        m_AttackDelay = config.scriptable.AttackDelay + m_AttackStartUp;
        // spawn puppet
        var puppet = Instantiate(m_PuppetPrefab,m_Self.transform.parent);   
        
        var enemyConfig = new EnemyControllerInitConfig{
            scriptable = m_PuppetScriptable,
            destination = config.destination ,
            cameraPos = CameraPos,
            spawnPos = config.spawnPos,
            camera = MainCamera,
            spawnId = BaseDefenceManager.GetInstance().GetEnemySpawnId()
        };

        m_PuppetController = puppet.GetComponent<PuppetController>();
        m_PuppetController.Init(enemyConfig);
        m_PuppetController.AddOnDeadAction(DestroyElectic);
    }
    
    private IEnumerator Start() {
        m_Self.transform.LookAt(new Vector3(CameraPos.x,m_Self.transform.position.y,CameraPos.z));
        
        m_Self.transform.position += Vector3.up*4.75f ;
        m_Self.transform.position += m_Self.transform.forward * 22f ;
        m_Animator.Play("Idle");

        yield return null;
        
        var electicController = m_Electic.GetComponent<ElecticController>();
        electicController.m_StartPos = m_Self;
        electicController.m_EndPos.transform.SetParent(m_PuppetController.transform);
        electicController.m_EndPos.transform.localPosition = Vector3.zero + Vector3.up*0.75f;

        m_Self.transform.LookAt(new Vector3(CameraPos.x,m_Self.transform.position.y,CameraPos.z));
        
    }

    public void DestroyElectic(){
        if(m_Electic != null)
            Destroy(m_Electic);
    }
    
    private void Update() {
        if(BaseDefenceManager.GetInstance().GetCurHp()<=0)
            this.enabled = false;
            
        if( IsThisDead )
            return;

        if(m_PuppetController != null){
            if(!m_PuppetController.IsDead()){
                // puppet is alive , stay in mid air
                
            }
        }else{
            // attack wall handler
            if(m_AttackDelay <=0){
                // attack
                StartCoroutine(Attack());
                
            }else{
                // wait
                m_AttackDelay -= Time.deltaTime;
                
            }
            
        }
    }
    

    
    public IEnumerator Attack(){
        m_Animator.speed = 1;
        m_AttackDelay = Scriptable.AttackDelay + m_AttackStartUp + m_AttackDelayPerSpawn * m_SpawnCount;
        yield return new WaitForSeconds(m_AttackStartUp);
        if(IsThisDead)
            yield break;
        // spawn ghost
        var ghost = Instantiate(m_Ghost,this.transform.parent);   
        m_SpawnCount ++;
        
        var enemyConfig = new EnemyControllerInitConfig{
            scriptable = m_GhostScriptable,
            destination = CameraPos + Vector3.forward + Vector3.down * 0.5f ,
            cameraPos = CameraPos,
            spawnPos = m_Self.transform.position,
            camera = MainCamera,
            spawnId = BaseDefenceManager.GetInstance().GetEnemySpawnId()
        };

        ghost.GetComponent<GhostController>().Init(enemyConfig);
    }

    public void SetGhostScriptable(EnemyScriptable enemyScriptable){
        m_GhostScriptable = enemyScriptable;
    }
    public void SetPuppetScriptable(EnemyScriptable enemyScriptable){
        m_PuppetScriptable = enemyScriptable;
    }
    
    protected override void OnDead(){
        base.OnDead();
        
        DestroyElectic();

        // remove puppet shield
        if(m_PuppetController != null){
            m_PuppetController.OnPuppeteerDead();
        }

        m_Animator.speed = 1;
        Destroy(m_Self,1);
    }
}
