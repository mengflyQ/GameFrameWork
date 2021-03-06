﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class LocomotorComponent : EntityComponent, IMovementCallback, INeedTaskService
    {
        //需要备份的初始数据
        bool m_avoid_obstacle = true;
        //运行数据
        FixPoint m_current_max_speed = FixPoint.Zero;
        IMovementProvider m_movement_provider = null;
        bool m_is_moving = false;
        int m_animation_block_cnt = 0;
        ComponentCommonTask m_task;
        FixPoint m_previous_max_speed = FixPoint.MinusOne;
        FixPoint m_locomotor_speed_rate = FixPoint.One;
        UpdateLocomotorSpeedRateTask m_update_locomotor_speed_rate_task = null;

        #region GETTER
        public bool IsMoving
        {
            get { return m_is_moving; }
        }

        public FixPoint MaxSpeed
        {
            get { return m_current_max_speed; }
            set
            {
                m_current_max_speed = value;
                if (m_movement_provider != null)
                    m_movement_provider.SetMaxSpeed(m_current_max_speed);
                if (m_previous_max_speed >= FixPoint.Zero)
                    StartLocomotorSpeedRateTask();
            }
        }

        public FixPoint LocomotorSpeedRate
        {
            get { return m_locomotor_speed_rate; }
        }

        public IMovementProvider GetMovementProvider()
        {
            return m_movement_provider;
        }

        public bool IsAnimationBlocked
        {
            get { return m_animation_block_cnt > 0; }
        }
        #endregion

        #region 初始化/销毁
        protected override void PostInitializeComponent()
        {
            m_previous_max_speed = m_current_max_speed;
            CalculateLocomotorSpeedrate();
        }

        public override void OnDeletePending()
        {
            StopMoving();
        }

        protected override void OnDestruct()
        {
            DeleteLocomotorSpeedRateTask();
            if (m_task != null)
            {
                m_task.Cancel();
                LogicTask.Recycle(m_task);
                m_task = null;
            }
        }

        void ClearMovementProvider()
        {
            if (m_movement_provider != null)
            {
                RecyclableObject.Recycle(m_movement_provider);
                m_movement_provider = null;
            }
        }
        #endregion

        #region 接口操作
        public bool MoveByDirection(Vector3FP direction)
        {
            if (!IsEnable())
                return false;
            if (m_movement_provider as MovementByDirection == null)
            {
                ClearMovementProvider();
                m_movement_provider = RecyclableObject.Create<MovementByDirection>();
                m_movement_provider.SetCallback(this);
                m_movement_provider.SetMaxSpeed(m_current_max_speed);
            }
            m_movement_provider.MoveByDirection(direction);
            StartMoving();
            return true;
        }

        public bool MoveAlongPath(List<Vector3FP> path, bool from_command)
        {
            //不可以保存path
            if (!IsEnable())
                return false;
            if (m_movement_provider as MovementAlongPath == null)
            {
                ClearMovementProvider();
                m_movement_provider = RecyclableObject.Create<MovementAlongPath>();
                m_movement_provider.SetCallback(this);
                m_movement_provider.SetMaxSpeed(m_current_max_speed);
            }
            m_movement_provider.MoveAlongPath(path);
            StartMoving(from_command);
            return true;
        }

        public void StopMoving(bool from_command = false)
        {
            if (!m_is_moving)
                return;
            m_task.Cancel();
            m_is_moving = false;
            OnMovementStopped(from_command);
        }
        #endregion

        #region IMovementCallback
        public ILogicOwnerInfo GetOwnerInfo()
        {
            return this;
        }

        public bool AvoidObstacle()
        {
            return m_avoid_obstacle;
        }

        public void MovementFinished()
        {
            StopMoving();
        }
        #endregion

        #region 实现
        void StartMoving(bool from_command = true)
        {
            if (m_task == null)
            {
                m_task = LogicTask.Create<ComponentCommonTask>();
                m_task.Construct(this);
            }
            var schedeler = GetLogicWorld().GetTaskScheduler();
            schedeler.Schedule(m_task, GetCurrentTime(), LOGIC_UPDATE_INTERVAL, LOGIC_UPDATE_INTERVAL);
            if (!m_is_moving)
            {
                m_is_moving = true;
                OnMovementStarted(from_command);
            }
        }

        protected void OnMovementStarted(bool from_command)
        {
            ParentObject.SendSignal(SignalType.StartMoving);
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStartMoving(ParentObject.ID, IsAnimationBlocked, from_command ? 0 : LocomoteRenderMessage.NotFromCommand);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        protected void OnMovementStopped(bool from_command)
        {
            ParentObject.SendSignal(SignalType.StopMoving);
#if COMBAT_CLIENT
            LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
            msg.ConstructAsStopMoving(ParentObject.ID, IsAnimationBlocked, from_command ? 0 : LocomoteRenderMessage.NotFromCommand);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        public void OnTaskService(FixPoint delta_time)
        {
            m_movement_provider.Update(delta_time);
        }

        protected override void OnDisable()
        {
            StopMoving();
        }
        #endregion

        #region Block Animation
        public void BlockAnimation()
        {
            ++m_animation_block_cnt;
        }

        public void UnblockAnimation()
        {
            --m_animation_block_cnt;
#if COMBAT_CLIENT
            if (m_animation_block_cnt == 0 && m_is_moving)
            {
                LocomoteRenderMessage msg = RenderMessage.Create<LocomoteRenderMessage>();
                msg.ConstructAsStartMoving(ParentObject.ID, IsAnimationBlocked, LocomoteRenderMessage.UnblockAnimation);
                GetLogicWorld().AddRenderMessage(msg);
            }
#endif
        }
        #endregion

        #region 速度对动作的影响
        void StartLocomotorSpeedRateTask()
        {
            if (m_update_locomotor_speed_rate_task != null)
                return;
            var schedeler = GetLogicWorld().GetTaskScheduler();
            if (schedeler == null)
                return; //ZZWTOOD 世界销毁的时候
            m_update_locomotor_speed_rate_task = LogicTask.Create<UpdateLocomotorSpeedRateTask>();
            m_update_locomotor_speed_rate_task.Construct(this);
            schedeler.Schedule(m_update_locomotor_speed_rate_task, GetCurrentTime(), FixPoint.PrecisionFP);
        }

        void DeleteLocomotorSpeedRateTask()
        {
            if (m_update_locomotor_speed_rate_task == null)
                return;
            m_update_locomotor_speed_rate_task.Cancel();
            LogicTask.Recycle(m_update_locomotor_speed_rate_task);
            m_update_locomotor_speed_rate_task = null;
        }

        public void UpdateLocomotorSpeedRate()
        {
            DeleteLocomotorSpeedRateTask();
            if (m_previous_max_speed == m_current_max_speed)
                return;
            m_previous_max_speed = m_current_max_speed;
            CalculateLocomotorSpeedrate();
#if COMBAT_CLIENT
            ChangeLocomotorSpeedRenderMessage msg = RenderMessage.Create<ChangeLocomotorSpeedRenderMessage>();
            msg.Construct(ParentObject.ID, m_locomotor_speed_rate);
            GetLogicWorld().AddRenderMessage(msg);
#endif
        }

        void CalculateLocomotorSpeedrate()
        {
            Entity owner_entity = GetOwnerEntity();
            Attribute attribute = EntityUtil.GetAttribute(owner_entity, PredefinedAttribute.MaxSpeed);
            if (attribute == null)
                return;
            m_locomotor_speed_rate = m_current_max_speed / attribute.BaseValue;
        }
        #endregion
    }

    public class UpdateLocomotorSpeedRateTask : Task<LogicWorld>
    {
        LocomotorComponent m_component;

        public void Construct(LocomotorComponent component)
        {
            m_component = component;
        }

        public override void OnReset()
        {
            m_component = null;
        }

        public override void Run(LogicWorld logic_world, FixPoint current_time, FixPoint delta_time)
        {
            m_component.UpdateLocomotorSpeedRate();
        }
    }
}