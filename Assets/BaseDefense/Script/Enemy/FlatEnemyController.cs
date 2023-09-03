using System;
using System.Collections;
using System.Collections.Generic;
//using System.IO.Ports;
using UnityEngine;

public class FlatEnemyController : EnemyController
{
    [SerializeField] private GameObject m_Self;
    [SerializeField] private Vector3 m_Destination;
    private bool m_CanAttack = false;
    private float m_AttackDelay = 0;

    public void Init(EnemyScriptable scriptable, Vector3 destination){
        Scriptable = scriptable;
        m_Destination = destination;
        CurHp = Scriptable.MaxHp;
        m_AttackDelay = scriptable.AttackDelay;
    }

    private void Start() {
        
    }

    private void Update() {
        if( IsDead )
            return;

        if(Vector2.Distance( new Vector2(m_Self.transform.position.x,m_Self.transform.position.z) , m_Destination)<0.25f){
           // close enough for attack 
           m_CanAttack = true;
        }else{
            // move
            float moveDistance = Scriptable.MoveSpeed * Time.deltaTime;
            m_Self.transform.position = Vector3.MoveTowards(
                m_Self.transform.position, m_Destination, moveDistance);
        }

        // attack wall handler
        if(m_CanAttack){
            if(m_AttackDelay <=0){
                // attack
                
            }else{
                // wait
                m_AttackDelay -= Time.deltaTime;
            }
        }
    }

    protected override void OnDead(){
        m_OnDead?.Invoke();
        Destroy(m_Self,1);
    }

}
