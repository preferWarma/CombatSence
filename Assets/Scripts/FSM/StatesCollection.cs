// ReSharper disable InconsistentNaming

using UnityEngine;

namespace FSM
{
    public class IdleState : IState
    {
        private readonly FSMManager manager;  // 状态机
        private readonly Parameter parameter;    // 参数
        private float timer; // 计时器

        public IdleState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("Idle");
            timer = 0f;
        }

        public void OnUpdate()
        {
            timer += Time.deltaTime;

            if (parameter.getHit)
            {
                manager.TransitionState(StateType.Hit);
            }

            // 发现玩家，切换反应状态
            if (parameter.target &&
                parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x)
            {
                manager.TransitionState(StateType.React);
            }
            
            if (timer >= parameter.idleTime) // 停留时间截止后
            {
                manager.TransitionState(StateType.Patrol);   // 切换巡逻
            }
        }

        public void OnExit()
        {
            timer = 0;
        }
    }
    
    public class PatrolState : IState
    {
        private readonly FSMManager manager;
        private readonly Parameter parameter;
        private int patrolIndex;    // 巡逻点索引
        
        
        public PatrolState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("Walk");
        }

        public void OnUpdate()
        {
            manager.FlipTo(parameter.patrolPoints[patrolIndex]);   // 改变朝向
            
            if (parameter.getHit)
            {
                manager.TransitionState(StateType.Hit);
            }
            
            // 发现玩家，切换反应状态
            if (parameter.target &&
                parameter.target.position.x >= parameter.chasePoints[0].position.x &&
                parameter.target.position.x <= parameter.chasePoints[1].position.x)
            {
                manager.TransitionState(StateType.React);
            }
            
            // 移动到目标
            manager.transform.position = Vector2.MoveTowards(manager.transform.position,
                parameter.patrolPoints[patrolIndex].position, parameter.moveSpeed * Time.deltaTime);
            // 到达后切换回停留状态
            if (Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolIndex].position) < 0.1f)
            {
                manager.TransitionState(StateType.Idle);
            }
        }

        public void OnExit()
        {
            patrolIndex = (patrolIndex + 1) % parameter.patrolPoints.Length;
        }
    }
    
    public class ChaseState : IState
    {
        private readonly FSMManager manager;
        private readonly Parameter parameter;

        public ChaseState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("Walk");
        }

        public void OnUpdate()
        {
            if (parameter.getHit)
            {
                manager.TransitionState(StateType.Hit);
            }
            
            if (parameter.target)
            {
                manager.FlipTo(parameter.target);    // 朝向目标
                manager.transform.position = Vector2.MoveTowards(manager.transform.position,
                    parameter.target.position, parameter.moveSpeed * Time.deltaTime);   // 移动
            }
            
            // 目标脱离范围
            if (!parameter.target || 
                     manager.transform.position.x < parameter.chasePoints[0].position.x ||
                     manager.transform.position.x > parameter.chasePoints[1].position.x)
            {
                manager.TransitionState(StateType.Idle);
            }
            
            // 检测到玩家在攻击范围内
            if (Physics2D.OverlapCircle(parameter.attackPoint.position, parameter.attackRadius, parameter.targetLayer))
            {
                manager.TransitionState(StateType.Attack);   // 切换为攻击状态
            }
            
        }

        public void OnExit()
        {
        }
    }
    
    public class ReactState : IState
    {
        private readonly FSMManager manager;
        private readonly Parameter parameter;
        private AnimatorStateInfo info; // 用于控制播放进度
        
        public ReactState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("React");
        }

        public void OnUpdate()
        {
            if (parameter.getHit)
            {
                manager.TransitionState(StateType.Hit);
            }
            
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime > 0.95f)    //动画播放完成一次后
            {
                manager.TransitionState(StateType.Chase);
            }
        }

        public void OnExit()
        {
        }
    }
    
    public class AttackState : IState
    {
        private readonly FSMManager manager;
        private readonly Parameter parameter;
        private AnimatorStateInfo info;

        public AttackState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("Attack");
        }

        public void OnUpdate()
        {
            if (parameter.getHit)
            {
                manager.TransitionState(StateType.Hit);
            }
            
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            if (info.normalizedTime >= 0.95f)    // 播放完一次动画后就切换
            {
                manager.TransitionState(StateType.Chase);
            }
        }

        public void OnExit()
        {
        }
    }
    
    public class HitState : IState
    {
        private readonly FSMManager manager;
        private readonly Parameter parameter;
        private AnimatorStateInfo info; // 用于控制播放进度
        
        public HitState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("Hit");
            parameter.health--;
        }

        public void OnUpdate()
        {
            info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            
            if (parameter.health <= 0)
            {
                manager.TransitionState(StateType.Death);
            }
            else // 受伤后直接将攻击目标设置为Player并切换追击
            {
                if (info.normalizedTime > 0.95f)
                {
                    parameter.target = GameObject.FindWithTag("Player").transform;
                    manager.TransitionState(StateType.Chase);
                }
            }
        }

        public void OnExit()
        {
            parameter.getHit = false;
        }
    }
    
    public class DeadState : IState
    {
        private readonly FSMManager manager;
        private readonly Parameter parameter;
        private float timer = 3f;
        
        public DeadState(FSMManager manager)
        {
            this.manager = manager;
            parameter = manager.parameter;
        }
        
        public void OnEnter()
        {
            parameter.animator.Play("Dead");
        }

        public void OnUpdate()
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                Object.Destroy(manager.gameObject);
            }
        }

        public void OnExit()
        {
        }
    }
}