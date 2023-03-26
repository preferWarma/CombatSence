// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable Unity.InefficientPropertyAccess

namespace FSM
{
    public enum StateType
    {
        Idle, Patrol, Chase, React, Attack, Hit, Death
    }

    [Serializable]
    public class Parameter
    {
        [Header("基本参数")]
        [Tooltip("生命值")] public int health;
        [Tooltip("移动速度")] public float moveSpeed;
        [Tooltip("追击速度")] public float chaseSpeed;  
        [Tooltip("停止时间")] public float idleTime;
        [Tooltip("巡逻范围")] public Transform[] patrolPoints;
        [Tooltip("追击范围")] public Transform[] chasePoints;
        
        [Header("攻击参数")]
        [Tooltip("攻击目标")] public Transform target;
        [Tooltip("攻击目标层级")] public LayerMask targetLayer;
        [Tooltip("攻击检测范围圆心")] public Transform attackPoint;
        [Tooltip("检测范围半径")] public float attackRadius;
        [Tooltip("是否是受击状态")] public bool getHit;
        
        [Header("动画器参数")]
        [Tooltip("动画器组件")] public Animator animator;
        
    }
    
    public class FSMManager : MonoBehaviour
    {
        [Tooltip("参数")] public Parameter parameter = new();
        
        private IState currentState;    // 当前状态
        private readonly Dictionary<StateType, IState> states = new();


        private void Awake()
        {
            parameter.animator = GetComponent<Animator>();  // 获取当前动画
        }

        private void Start()
        {
            // 状态机注册
            states.Add(StateType.Idle, new IdleState(this));
            states.Add(StateType.Attack, new AttackState(this));
            states.Add(StateType.Chase, new ChaseState(this));
            states.Add(StateType.Patrol, new PatrolState(this));
            states.Add(StateType.React, new ReactState(this));
            states.Add(StateType.Hit, new HitState(this));
            states.Add(StateType.Death, new DeadState(this));
            
            TransitionState(StateType.Idle);    // 设置初始状态
            
        }

        private void Update()
        {
            currentState.OnUpdate();
            if (Input.GetKeyDown(KeyCode.Return))
            {
                parameter.getHit = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                parameter.target = other.transform;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                parameter.target = null;
            }
        }

        public void TransitionState(StateType stateType)    // 状态切换
        {
            currentState?.OnExit();
            
            currentState = states[stateType];
            currentState.OnEnter();
        }

        public void FlipTo(Transform target) // 改变朝向
        {
            if (!target) return;
            if (transform.position.x > target.transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if(transform.position.x < target.transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        private void OnDrawGizmos() // 屏幕上绘制图像
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(parameter.attackPoint.position, parameter.attackRadius);
        }
    }
}