﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class LocomotorComponent : EntityComponent, IMovementCallback
    {
        //需要备份的初始数据
        //运行数据
        FixPoint m_current_max_speed = FixPoint.Zero;
        IMovementProvider m_movement_provider = null;
        bool m_is_moving = false;
        LocomoterTask m_task;

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
            }
        }

        public IMovementProvider GetMovementProvider()
        {
            return m_movement_provider;
        }
        #endregion

        #region 初始化/销毁
        public override void OnDeletePending()
        {
            StopMoving();
        }

        protected override void OnDestruct()
        {
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

        public bool MoveAlongPath(List<Vector3FP> path)
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
            StartMoving();
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
                m_task = LogicTask.Create<LocomoterTask>();
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
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StartMoving, ParentObject.ID, from_command ? 0 : SimpleRenderMessage.NotFromCommand);
        }

        protected void OnMovementStopped(bool from_command)
        {
            ParentObject.SendSignal(SignalType.StopMoving);
            GetLogicWorld().AddSimpleRenderMessage(RenderMessageType.StopMoving, ParentObject.ID, from_command ? 0 : SimpleRenderMessage.NotFromCommand);
        }

        public void UpdatePosition(FixPoint delta_time)
        {
            m_movement_provider.Update(delta_time);
        }

        protected override void OnDisable()
        {
            StopMoving();
        }
        #endregion
    }

    class LocomoterTask : Task<LogicWorld>
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
            m_component.UpdatePosition(delta_time);
        }
    }
}