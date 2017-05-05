﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class TargetingComponent : EntityComponent, ISignalListener
    {
        static readonly FixPoint TARGETING_UPDATE_MIN_FREQUENCY = FixPoint.Two / FixPoint.Ten;
        static readonly FixPoint TARGETING_UPDATE_MAX_FREQUENCY = FixPoint.Two;

        SignalListenerContext m_listener_context;
        Entity m_current_target;
        UpdateTargetingTask m_task;

        #region GETTER
        public Entity GetCurrentTarget()
        {
            if (m_current_target == null)
                SelectDefaultTarget();
            return m_current_target;
        }
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            m_listener_context = SignalListenerContext.CreateForEntityComponent(GetLogicWorld().GenerateSignalListenerID(), ParentObject.ID, m_component_type_id);
        }

        protected override void OnDestruct()
        {
            SignalListenerContext.Recycle(m_listener_context);
            m_listener_context = null;
            m_current_target = null;
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }
        #endregion

        #region ISignalListener
        public void ReceiveSignal(ISignalGenerator generator, int signal_type, System.Object signal = null)
        {
            switch (signal_type)
            {
            case SignalType.Die:
                OnTargetDie(generator as Entity);
                break;
            default:
                break;
            }
        }

        void OnTargetDie(Entity target)
        {
            if (target != m_current_target)
                return;
            StopTargeting();
        }

        public void OnGeneratorDestroyed(ISignalGenerator generator)
        {
        }
        #endregion

        public void SelectDefaultTarget()
        {
            //TODO 根据游戏配置和某种规则，找范围内的某个可攻击的敌人
            Entity target = null;
            StartTargeting(target);
        }

        public void StartTargeting(Entity target)
        {
            if (m_current_target != null && target.ID == m_current_target.ID)
                return;
            if (ObjectUtil.IsDead(target))
                return;
            StopTargeting();
            m_current_target = target;
            target.AddListener(SignalType.Die, m_listener_context);
            ScheduleTargeting(FixPoint.Zero);
        }

        public void StopTargeting()
        {
            if (m_current_target == null)
                return;
            m_current_target.RemoveListener(SignalType.Die, m_listener_context.ID);
            m_current_target = null;
            m_task.Cancel();
        }

        public void ScheduleTargeting(FixPoint delay)
        {
            if (m_task == null)
            {
                m_task = LogicTask.Create<UpdateTargetingTask>();
                m_task.Construct(this);
            }
            LogicWorld logic_world = GetLogicWorld();
            var task_scheduler = logic_world.GetTaskScheduler();
            task_scheduler.Schedule(m_task, logic_world.GetCurrentTime(), delay);
        }

        public void UpdateTargeting()
        {
            PositionComponent position_cmp = ParentObject.GetComponent(PositionComponent.ID) as PositionComponent;
            LocomotorComponent locomotor_cmp = ParentObject.GetComponent(LocomotorComponent.ID) as LocomotorComponent;
            SkillManagerComponent skill_cmp = ParentObject.GetComponent(SkillManagerComponent.ID) as SkillManagerComponent;
            Skill skill = skill_cmp.GetDefaultSkill();
            if (skill == null)
                return;

            if (!skill.IsReady() && locomotor_cmp != null && locomotor_cmp.IsMoving)
            {
                FixPoint delay = skill.GetNextReadyTime();
                if (delay == FixPoint.Zero)
                    delay = FixPoint.PrecisionFP;
                ScheduleTargeting(delay);
                return;
            }

            bool move_required = false;
            FixPoint max_range = skill.GetSkillDefinitionComponent().MaxRange;
            PositionComponent target_position_cmp = m_current_target.GetComponent(PositionComponent.ID) as PositionComponent;
            Vector3FP direction = target_position_cmp.CurrentPosition - position_cmp.CurrentPosition;
            if (max_range > 0)
            {
                FixPoint distance = direction.FastLength() - target_position_cmp.Radius - position_cmp.Radius;  //ZZWTODO 多处距离计算
                if (distance > max_range)
                {
                    move_required = true;
                    if (locomotor_cmp == null)
                    {
                        //ZZWTODO
                        ScheduleTargeting(TARGETING_UPDATE_MAX_FREQUENCY);
                    }
                    else if (!locomotor_cmp.IsEnable())
                    {
                        ScheduleTargeting(TARGETING_UPDATE_MIN_FREQUENCY);
                    }
                    else
                    {
                        FixPoint delay = (distance - max_range) / locomotor_cmp.MaxSpeed;
                        if (delay > TARGETING_UPDATE_MAX_FREQUENCY)
                            delay = TARGETING_UPDATE_MAX_FREQUENCY;
                        else if (delay < TARGETING_UPDATE_MIN_FREQUENCY)
                            delay = TARGETING_UPDATE_MIN_FREQUENCY;
                        PathFindingComponent pathfinding_component = ParentObject.GetComponent(PathFindingComponent.ID) as PathFindingComponent;
                        if (pathfinding_component != null)
                        {
                            if (pathfinding_component.FindPath(target_position_cmp.CurrentPosition))
                                locomotor_cmp.GetMovementProvider().FinishMovementWhenTargetInRange(target_position_cmp, max_range);
                        }
                        else
                        {
                            List<Vector3FP> path = new List<Vector3FP>();
                            path.Add(position_cmp.CurrentPosition);
                            path.Add(target_position_cmp.CurrentPosition);
                            locomotor_cmp.MoveAlongPath(path);
                            locomotor_cmp.GetMovementProvider().FinishMovementWhenTargetInRange(target_position_cmp, max_range);
                        }
                        ScheduleTargeting(delay);
                    }
                }
            }
            if (!move_required)
            {
                if (skill.CheckActivate() == CastSkillResult.Success)
                {
                    position_cmp.SetAngle(direction);
                    skill.Activate(GetCurrentTime());
                    FixPoint delay = skill.GetSkillDefinitionComponent().CooldownTime + FixPoint.PrecisionFP;
                    if (delay > TARGETING_UPDATE_MAX_FREQUENCY)
                        delay = TARGETING_UPDATE_MAX_FREQUENCY;
                    ScheduleTargeting(delay);
                }
                else
                {
                    FixPoint delay = skill.GetNextReadyTime();
                    if (delay < TARGETING_UPDATE_MIN_FREQUENCY)
                        delay = TARGETING_UPDATE_MIN_FREQUENCY;
                    ScheduleTargeting(delay);
                }
            }
        }
    }

    class UpdateTargetingTask : Task<LogicWorld>
    {
        TargetingComponent m_component;

        public void Construct(TargetingComponent component)
        {
            m_component = component;
        }

        public override void OnReset()
        {
            m_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.UpdateTargeting();
        }
    }
}