﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
namespace Combat
{
    public class ComponentPaitialGenerator
    {
        //控制整个组件的标志位
        const int GENERATE_InitializeVariable = 1 << 0;//自动生成public override void InitializeVariable(Dictionary<string, string> variables)
        const int GENERATE_GetVariable        = 1 << 1;//自动生成public override bool GetVariable(int id, out FixPoint value)
        const int GENERATE_SetVariable        = 1 << 2;//自动生成public override bool SetVariable(int id, FixPoint value)
        const int GENERATE_CSharpAttribute    = 1 << 3;//自动生成CSharp属性

        const int Flag_LogicComponent = GENERATE_InitializeVariable | GENERATE_GetVariable | GENERATE_SetVariable | GENERATE_CSharpAttribute;
        const int Flag_RenderComponent = GENERATE_InitializeVariable | GENERATE_CSharpAttribute;

        //控制单个变量的标志位
        const int VARIABLE_INIT    = 1 << 10;//在函数InitializeVariable()中存在
        const int VARIABLE_GET     = 1 << 11;//在函数GetVariable()中存在
        const int VARIABLE_SET     = 1 << 12;//在函数SetVariable()中存在
        const int CS_ATTRIBUTE_GET = 1 << 13;//需要get的CSharp属性
        const int CS_ATTRIBUTE_SET = 1 << 14;//需要set的CSharp属性
        const int TRANSFORM2CRCID  = 1 << 15;

        const int Flag_Variable_GetSet_Attribute_GetSet = VARIABLE_INIT | VARIABLE_GET | VARIABLE_SET | CS_ATTRIBUTE_GET | CS_ATTRIBUTE_SET;
        const int Flag_Variable_GetSet_Attribute_Get = VARIABLE_INIT | VARIABLE_GET | VARIABLE_SET | CS_ATTRIBUTE_GET;//这个应该做为默认值
        const int Flag_Variable_Get_Attribute_Get = VARIABLE_INIT | VARIABLE_GET | CS_ATTRIBUTE_GET;
        const int Flag_Variable_GetSet = VARIABLE_INIT | VARIABLE_GET | VARIABLE_SET;
        const int Flag_Variable_Get = VARIABLE_INIT | VARIABLE_GET;
        const int Flag_Attribute_GetSet = VARIABLE_INIT | CS_ATTRIBUTE_GET | CS_ATTRIBUTE_SET;
        const int Flag_Attribute_Get = VARIABLE_INIT | CS_ATTRIBUTE_GET;
        
        static void InitLogicComponents()
        {
            //组件按类别然后按字母顺序，方便检查
            //变量名都以"VID_"开头
            m_logic = true;

            #region Object
            REGISTER_COMPONENT<LevelComponent>()
                .REGISTER_VARIABLE<int>("be_killed_experience", "VID_BeKilledExperience", "m_be_killed_experience", Flag_Attribute_Get)
                .REGISTER_VARIABLE<int>("max_level", "VID_MaxLevel", "m_max_level", Flag_Attribute_Get)
                .REGISTER_VARIABLE<string>("level_table", null, "m_experience_level_table")
                .REGISTER_VARIABLE<int>("level", "VID_CurrentLevel", "CurrentLevel", Flag_Variable_GetSet);
            REGISTER_COMPONENT<TurnManagerComponent>();
            #endregion

            #region Player
            REGISTER_COMPONENT<FactionComponent>()
                .REGISTER_VARIABLE_CRC<int>("faction", null, "m_faction");
            REGISTER_COMPONENT<PlayerAIComponent>();
            REGISTER_COMPONENT<PlayerGameplaySpecialComponent>();
            REGISTER_COMPONENT<PlayerTargetingComponent>();
            #endregion

            #region Entity
            REGISTER_COMPONENT<AIComponent>()
                .REGISTER_VARIABLE<int>("ai_tree_id", null, "m_bahavior_tree_id");
            REGISTER_COMPONENT<AttributeManagerComponent>();
            REGISTER_COMPONENT<DamagableComponent>()
                .REGISTER_VARIABLE<FixPoint>("max_health", "VID_MaxHealth", "MaxHealth", Flag_Variable_GetSet)
                .REGISTER_VARIABLE<FixPoint>("current_health", "VID_CurrentHealth", "CurrentHealth", Flag_Variable_GetSet);
            REGISTER_COMPONENT<DamageModificationComponent>();
            REGISTER_COMPONENT<DeathComponent>()
                .REGISTER_VARIABLE<int>("born_generator_id", null, "m_born_generator_cfgid")
                .REGISTER_VARIABLE<int>("die_generator_id", null, "m_die_generator_cfgid")
                .REGISTER_VARIABLE<int>("killer_generator_id", null, "m_killer_generator_cfgid")
                .REGISTER_VARIABLE<FixPoint>("life_time", null, "m_life_time")
                .REGISTER_VARIABLE<FixPoint>("hide_delay", null, "m_hide_delay")
                .REGISTER_VARIABLE<FixPoint>("delete_delay", null, "m_delete_delay")
                .REGISTER_VARIABLE<bool>("can_resurrect", null, "m_can_resurrect");
            REGISTER_COMPONENT<EffectManagerComponent>();
            REGISTER_COMPONENT<EffectRegionComponent>()
                .REGISTER_VARIABLE_CRC<int>("gathering_type", null, "m_target_gathering_param.m_type")
                .REGISTER_VARIABLE<FixPoint>("gathering_param1", null, "m_target_gathering_param.m_param1")
                .REGISTER_VARIABLE<FixPoint>("gathering_param2", null, "m_target_gathering_param.m_param2")
                .REGISTER_VARIABLE_CRC<int>("gathering_faction", null, "m_target_gathering_param.m_faction")
                .REGISTER_VARIABLE_CRC<int>("gathering_category", null, "m_target_gathering_param.m_category")
                .REGISTER_VARIABLE<int>("enter_generator", null, "m_enter_generator_cfgid")
                .REGISTER_VARIABLE<int>("period_generator", null, "m_period_generator_cfgid")
                .REGISTER_VARIABLE<FixPoint>("period", null, "m_period")
                .REGISTER_VARIABLE<FixPoint>("region_update_interval", null, "m_region_update_interval");
            REGISTER_COMPONENT<EntityDefinitionComponent>()
                .REGISTER_VARIABLE_CRC<int>("category1", null, "m_category_1")
                .REGISTER_VARIABLE_CRC<int>("category2", null, "m_category_2")
                .REGISTER_VARIABLE_CRC<int>("category3", null, "m_category_3");
            REGISTER_COMPONENT<EntityGameplaySpecilaComponent>();
            REGISTER_COMPONENT<LocomotorComponent>()
                .REGISTER_VARIABLE<FixPoint>("max_speed", "VID_MaxSpeed", "MaxSpeed", Flag_Variable_GetSet)
                .REGISTER_VARIABLE<bool>("avoid_obstacle", null, "m_avoid_obstacle");
            REGISTER_COMPONENT<ManaComponent>()
                .REGISTER_VARIABLE<FixPoint>("max_mana", null, "m_current_max_mana")
                .REGISTER_VARIABLE<FixPoint>("current_mana", null, "m_current_mana");
            REGISTER_COMPONENT<ObstacleComponent>()
                .REGISTER_VARIABLE<FixPoint>("ext_x", null, "m_extents.x")
                .REGISTER_VARIABLE<FixPoint>("ext_y", null, "m_extents.y")
                .REGISTER_VARIABLE<FixPoint>("ext_z", null, "m_extents.z");
            REGISTER_COMPONENT<PathFindingComponent>();
            REGISTER_COMPONENT<PositionComponent>()
                .REGISTER_VARIABLE<FixPoint>("x", "VID_X", "m_current_position.x")
                .REGISTER_VARIABLE<FixPoint>("y", "VID_Y", "m_current_position.y")
                .REGISTER_VARIABLE<FixPoint>("z", "VID_Z", "m_current_position.z")
                .REGISTER_VARIABLE<FixPoint>("angle", "VID_BaseAngle", "m_base_angle", Flag_Variable_GetSet)
                .REGISTER_VARIABLE<FixPoint>("radius", "VID_Radius", "m_radius")
                .REGISTER_VARIABLE<FixPoint>("height", "VID_Height", "m_height")
                .REGISTER_VARIABLE<bool>("base_rotatable", "VID_BaseRotatable", "m_base_rotatable", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("head_rotatable", "VID_HeadRotatable", "m_head_rotatable", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("collision_sender", null, "m_collision_sender")
                .REGISTER_VARIABLE<bool>("visible", "VID_Visible", "m_visible", Flag_Attribute_Get);
            REGISTER_COMPONENT<ProjectileComponent>()
                .REGISTER_VARIABLE<FixPoint>("speed", "VID_Speed", "m_speed", Flag_Attribute_Get)
                .REGISTER_VARIABLE<FixPoint>("life_time", null, "m_life_time")
                .REGISTER_VARIABLE_CRC<int>("track_mode", "VID_TrackMode", "m_track_mode", Flag_Attribute_Get)
                .REGISTER_VARIABLE_CRC<int>("trajectory_type", "VID_TrajectoryType", "m_trajectory_type", Flag_Attribute_Get)
                .REGISTER_VARIABLE<FixPoint>("extra_hight", null, "m_extra_hight")
                .REGISTER_VARIABLE<bool>("can_cross_obstacle", null, "m_can_cross_obstacle")
                .REGISTER_VARIABLE<bool>("pierce_entity", null, "m_pierce_entity")
                .REGISTER_VARIABLE_CRC<int>("collision_faction", null, "m_collision_faction")
                .REGISTER_VARIABLE<int>("collision_sound", null, "m_collision_sound_cfgid");
            REGISTER_COMPONENT<SimpleAIComponent>()
                .REGISTER_VARIABLE<FixPoint>("guard_range", null, "m_guard_range");
            REGISTER_COMPONENT<SkillManagerComponent>()
                .REGISTER_VARIABLE<FixPoint>("attack_speed_rate", "VID_AttackSpeedRate", "AttackSpeedRate", Flag_Variable_GetSet)
                .REGISTER_VARIABLE<FixPoint>("cooldown_reduce_rate", "VID_CooldownReduceRate", "CooldownReduceRate", Flag_Variable_GetSet);
            REGISTER_COMPONENT<SpawnObjectComponent>()
                .REGISTER_VARIABLE<int>("object_type_id", null, "m_object_type_id")
                .REGISTER_VARIABLE<int>("object_proto_id", null, "m_object_proto_id")
                .REGISTER_VARIABLE<FixPoint>("object_distance", null, "m_object_distance")
                .REGISTER_VARIABLE<int>("init_count", null, "m_init_count")
                .REGISTER_VARIABLE<int>("max_count", null, "m_max_count")
                .REGISTER_VARIABLE<FixPoint>("update_interval", null, "m_update_interval");
            REGISTER_COMPONENT<StateComponent>();
            REGISTER_COMPONENT<SummonedEntityComponent>()
                .REGISTER_VARIABLE<bool>("die_with_master", null, "m_die_with_master");
            REGISTER_COMPONENT<TargetingComponent>()
                .REGISTER_VARIABLE<bool>("attack_once", null, "m_attack_once");
            #endregion

            #region Skill
            REGISTER_COMPONENT<BehaviorTreeSkillComponent>()
                .REGISTER_VARIABLE<int>("bahavior_tree_id", null, "m_bahavior_tree_id");
            REGISTER_COMPONENT<CreateObjectSkillComponent>()
                .REGISTER_VARIABLE<FixPoint>("delay_time", null, "m_delay_time")
                .REGISTER_VARIABLE<int>("render_effect", null, "m_render_effect_cfgid")
                .REGISTER_VARIABLE<int>("object_type_id", null, "m_object_type_id")
                .REGISTER_VARIABLE<int>("object_proto_id", null, "m_object_proto_id")
                .REGISTER_VARIABLE<FixPoint>("object_life_time", null, "m_object_life_time")
                .REGISTER_VARIABLE<int>("generator_id", null, "m_generator_cfgid")
                .REGISTER_VARIABLE<FixPoint>("offset_x", null, "m_offset.x")
                .REGISTER_VARIABLE<FixPoint>("offset_y", null, "m_offset.y")
                .REGISTER_VARIABLE<FixPoint>("offset_z", null, "m_offset.z")
                .REGISTER_VARIABLE_CRC<int>("combo_type", null, "m_combo_type_crc")
                .REGISTER_VARIABLE<int>("combo_attack_cnt", null, "m_combo_attack_cnt")
                .REGISTER_VARIABLE<FixPoint>("combo_interval", null, "m_combo_interval");
            REGISTER_COMPONENT<DirectDamageSkillComponent>()
                .REGISTER_VARIABLE<FixPoint>("delay_time", null, "m_delay_time")
                .REGISTER_VARIABLE_CRC<int>("damage_type", null, "m_damage_type_id")
                .REGISTER_VARIABLE<Formula>("damage_amount", null, "m_damage_amount")
                .REGISTER_VARIABLE<int>("damage_render_effect", null, "m_damage_render_effect_cfgid")
                .REGISTER_VARIABLE<int>("damage_sound", null, "m_damage_sound_cfgid")
                .REGISTER_VARIABLE<int>("combo_attack_cnt", null, "m_combo_attack_cnt")
                .REGISTER_VARIABLE<FixPoint>("combo_interval", null, "m_combo_interval")
                .REGISTER_VARIABLE<int>("render_effect", null, "m_render_effect_cfgid");
            REGISTER_COMPONENT<EffectGeneratorSkillComponent>()
                .REGISTER_VARIABLE<FixPoint>("delay_time", null, "m_delay_time")
                .REGISTER_VARIABLE<int>("generator_id", null, "m_generator_cfgid")
                .REGISTER_VARIABLE<int>("render_effect", null, "m_render_effect_cfgid")
                .REGISTER_VARIABLE<FixPoint>("render_delay_time", null, "m_render_delay_time");
            REGISTER_COMPONENT<KillTargetSkillComponent>();
            REGISTER_COMPONENT<SkillDefinitionComponent>()
                .REGISTER_VARIABLE_CRC<int>("mana_type", "VID_ManaType", "m_mana_type", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("mana_cost", "VID_ManaCost", "m_mana_cost", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("min_range", "VID_MinRange", "m_min_range", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("max_range", "VID_MaxRange", "m_max_range", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("cooldown_time", "VID_CooldownTime", "m_cooldown_time", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("casting_time", "VID_CastingTime", "m_casting_time", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("inflict_time", "VID_InflictTime", "m_inflict_time", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("expiration_time", "VID_ExpirationTime", "m_expiration_time", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("normal_attack", "VID_NormalAttack", "m_normal_attack", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("starts_active", "VID_StartsActive", "m_starts_active", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("blocks_other_skills_when_active", "VID_BlocksOtherSkillsWhenActive", "m_blocks_other_skills_when_active", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("blocks_movement_when_active", "VID_BlocksMovementWhenActive", "m_blocks_movement_when_active", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("blocks_rotating_when_active", "VID_BlocksRotatingWhenActive", "m_blocks_rotating_when_active", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("deactivate_when_moving", "VID_DeactivateWhenMoving", "m_deactivate_when_moving", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("can_activate_while_moving", "VID_CanActivateWhileMoving", "m_can_activate_while_moving", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("moving_activating_must_have_target", "VID_MovingActivatingMustHaveTarget", "m_moving_activating_must_have_target", Flag_Attribute_Get)
                .REGISTER_VARIABLE<bool>("can_activate_when_disabled", "VID_CanActivateWhenDisabled", "m_can_activate_when_disabled", Flag_Attribute_Get)
                .REGISTER_VARIABLE_CRC<int>("gathering_type", null, "m_target_gathering_param.m_type")
                .REGISTER_VARIABLE<FixPoint>("gathering_param1", null, "m_target_gathering_param.m_param1")
                .REGISTER_VARIABLE<FixPoint>("gathering_param2", null, "m_target_gathering_param.m_param2")
                .REGISTER_VARIABLE_CRC<int>("gathering_faction", null, "m_target_gathering_param.m_faction")
                .REGISTER_VARIABLE_CRC<int>("gathering_category", null, "m_target_gathering_param.m_category")
                .REGISTER_VARIABLE<bool>("need_gather_targets", "VID_NeedGatherTargets", "m_need_gather_targets", Flag_Attribute_Get)
                .REGISTER_VARIABLE<int>("targets_min_count_for_activate", "VID_TargetsMinCountForActivate", "m_targets_min_count_for_activate", Flag_Attribute_Get)
                .REGISTER_VARIABLE_CRC<int>("external_data_type", "VID_ExternalDataType", "m_external_data_type", Flag_Attribute_Get)
                .REGISTER_VARIABLE_CRC<int>("auto_face", "VID_AutoFaceType", "m_auto_face_type", Flag_Attribute_Get)
                //.REGISTER_VARIABLE<int>("inflict_type", "VID_InflictType", "m_inflict_type", Flag_Attribute_Get)
                //.REGISTER_VARIABLE<string>("inflict_missile", null, "m_inflict_missile")
                //.REGISTER_VARIABLE<FixPoint>("inflict_missile_speed", "VID_InflictMissileSpeed", "m_inflict_missile_speed", Flag_Attribute_Get)
                //.REGISTER_VARIABLE<FixPoint>("impact_delay", "VID_ImpactDelay", "m_impact_delay", Flag_Attribute_Get)
                .REGISTER_VARIABLE<FixPoint>("aim_param1", "VID_AimParam1", "m_aim_param1", Flag_Attribute_Get)
                .REGISTER_VARIABLE<FixPoint>("aim_param2", "VID_AimParam2", "m_aim_param2", Flag_Attribute_Get)
                .REGISTER_VARIABLE<string>("icon", null, "m_icon")
                .REGISTER_VARIABLE_CRC<int>("auto_aim", null, "m_auto_aim_type")
                .REGISTER_VARIABLE<string>("casting_animation", null, "m_casting_animation")
                .REGISTER_VARIABLE<string>("main_animation", null, "m_main_animation")
                .REGISTER_VARIABLE<int>("main_animation_count", null, "m_main_animation_count")
                .REGISTER_VARIABLE<string>("expiration_animation", null, "m_expiration_animation")
                .REGISTER_VARIABLE<int>("main_render_effect", null, "m_main_render_effect_cfgid")
                .REGISTER_VARIABLE<int>("main_sound", null, "m_main_sound");
            REGISTER_COMPONENT<SpurtSkillComponent>()
                .REGISTER_VARIABLE<FixPoint>("distance", null, "m_distance")
                .REGISTER_VARIABLE<FixPoint>("time", null, "m_time")
                .REGISTER_VARIABLE<int>("collision_target_generator_id", null, "m_collision_target_generator_cfgid")
                .REGISTER_VARIABLE<bool>("backward", null, "m_backward");
            #endregion

            #region Effect
            REGISTER_COMPONENT<AddDamageModifierEffectComponent>();
            REGISTER_COMPONENT<AddManaEffectComponent>()
                .REGISTER_VARIABLE_CRC<int>("mana_type", null, "m_mana_type")
                .REGISTER_VARIABLE<Formula>("mana_amount", null, "m_mana_amount");
            REGISTER_COMPONENT<AddStateEffectComponent>()
                .REGISTER_VARIABLE_CRC<int>("state", null, "m_state");
            REGISTER_COMPONENT<ApplyGeneratorEffectComponent>()
                .REGISTER_VARIABLE<int>("generator_cfgid", null, "m_generator_cfgid")
                .REGISTER_VARIABLE<int>("combo_count", null, "m_combo_count")
                .REGISTER_VARIABLE<FixPoint>("combo_interval", null, "m_combo_interval");
            REGISTER_COMPONENT<BehaviorTreeEffectComponent>()
                .REGISTER_VARIABLE<int>("bahavior_tree_id", null, "m_bahavior_tree_id");
            REGISTER_COMPONENT<ChangePlayerFactionEffectComponent>()
                .REGISTER_VARIABLE_CRC<int>("faction", null, "m_faction")
                .REGISTER_VARIABLE<bool>("revert_when_unapply", null, "m_revert_when_unapply");
            REGISTER_COMPONENT<CreateObjectEffectComponent>()
                .REGISTER_VARIABLE<int>("object_type_id", null, "m_object_type_id")
                .REGISTER_VARIABLE<int>("object_proto_id", null, "m_object_proto_id")
                .REGISTER_VARIABLE<FixPoint>("object_life_time", null, "m_object_life_time")
                .REGISTER_VARIABLE<FixPoint>("offset_x", null, "m_offset.x")
                .REGISTER_VARIABLE<FixPoint>("offset_y", null, "m_offset.y")
                .REGISTER_VARIABLE<FixPoint>("offset_z", null, "m_offset.z")
                .REGISTER_VARIABLE<int>("object_count", null, "m_object_count")
                .REGISTER_VARIABLE<FixPoint>("interval", null, "m_interval")
                .REGISTER_VARIABLE<bool>("revert_when_unapply", null, "m_revert_when_unapply");
            REGISTER_COMPONENT<DamageEffectComponent>()
                .REGISTER_VARIABLE_CRC<int>("damage_type", null, "m_damage_type_id")
                .REGISTER_VARIABLE<Formula>("damage_amount", null, "m_damage_amount")
                .REGISTER_VARIABLE<int>("damage_render_effect", null, "m_damage_render_effect_cfgid")
                .REGISTER_VARIABLE<int>("damage_sound", null, "m_damage_sound_cfgid");
            REGISTER_COMPONENT<DamageOverTimeEffectComponent>()
                .REGISTER_VARIABLE_CRC<int>("damage_type", null, "m_damage_type_id")
                .REGISTER_VARIABLE<Formula>("damage_amount", null, "m_damage_amount")
                .REGISTER_VARIABLE<int>("damage_render_effect", null, "m_damage_render_effect_cfgid")
                .REGISTER_VARIABLE<int>("damage_sound", null, "m_damage_sound_cfgid")
                .REGISTER_VARIABLE<FixPoint>("period", null, "m_period");
            REGISTER_COMPONENT<EffectDefinitionComponent>()
                .REGISTER_VARIABLE_CRC<int>("category", "VID_Category", "m_category", Flag_Attribute_Get)
                .REGISTER_VARIABLE_CRC<int>("conflict_id", "VID_ConflictID", "m_conflict_id", Flag_Attribute_Get)
                .REGISTER_VARIABLE<Formula>("duration", "VID_Duration", "m_duration", Flag_Attribute_Get)
                .REGISTER_VARIABLE<int>("render_effect", null, "m_render_effect_cfgid");
            REGISTER_COMPONENT<HealEffectComponent>();
            REGISTER_COMPONENT<KillOwnerEffectComponent>();
            REGISTER_COMPONENT<KnockbackEffectComponent>()
                .REGISTER_VARIABLE<FixPoint>("distance", null, "m_distance")
                .REGISTER_VARIABLE<FixPoint>("time", null, "m_time");
            REGISTER_COMPONENT<ModifyAttributeEffectComponent>();
            #endregion
        }

        static void InitRenderComponents()
        {
            m_logic = false;
            REGISTER_COMPONENT<AnimationComponent>(Flag_RenderComponent)
                .REGISTER_VARIABLE<string>("animation_path", null, "m_animation_path")
                .REGISTER_VARIABLE<string>("locomotor_animation_name", null, "m_locomotor_animation_name");
            REGISTER_COMPONENT<AnimatorComponent>(Flag_RenderComponent)
                .REGISTER_VARIABLE<string>("animator_path", null, "m_animator_path")
                .REGISTER_VARIABLE<string>("locomotor_animation_name", null, "m_locomotor_animation_name");
            REGISTER_COMPONENT<AimingComponent>(Flag_RenderComponent)
                .REGISTER_VARIABLE<string>("root_path", null, "m_root_path")
                .REGISTER_VARIABLE<string>("aiming_line_parent_path", null, "m_aiming_line_parent_path")
                .REGISTER_VARIABLE<string>("aiming_line_asset", null, "m_aiming_line_asset")
                .REGISTER_VARIABLE<string>("rotate_turret_asset", null, "m_rotate_turret_asset");
            REGISTER_COMPONENT<ModelComponent>(Flag_RenderComponent)
                .REGISTER_VARIABLE<string>("asset", null, "m_asset_name")
                .REGISTER_VARIABLE<string>("bodyctrl_path", null, "m_bodyctrl_path")
                .REGISTER_VARIABLE<string>("headctrl_path", null, "m_headctrl_path");
            REGISTER_COMPONENT<PredictLogicComponent>(Flag_RenderComponent);
            REGISTER_COMPONENT<RenderEffectManagerComponent>(Flag_RenderComponent);
        }

        static ComponentPaitialGenerator()
        {
            m_real_type[typeof(char)] = "char";
            m_real_type[typeof(short)] = "short";
            m_real_type[typeof(int)] = "int";
            m_real_type[typeof(long)] = "long";
            m_real_type[typeof(bool)] = "bool";
            m_real_type[typeof(float)] = "float";
            m_real_type[typeof(double)] = "double";
            m_real_type[typeof(string)] = "string";
        }
        static Dictionary<System.Type, string> m_real_type = new Dictionary<Type, string>();

        class EditorVariable
        {
            public string m_type_name;              //类型名，比如"int"、"FixPoint"
            public string m_config_name;            //配置文件中使用的名字，比如"max_speed"
            public string m_code_name;              //代码中的VID_名，比如"VID_MaxSpeed"，可以是null，表示不支持variable，也不支持CSharp的属性
            public string m_code_fragment;          //代码中这个值的代码片段，比如"m_max_speed"、"MaxSpeed"
            public int m_flag = 0;
            public bool CanVariableInit()
            {
                return (m_flag & VARIABLE_INIT) != 0;
            }
            public bool NeedDeclareVariable()
            {
                return (m_flag & (VARIABLE_GET | VARIABLE_SET)) != 0;
            }
            public bool NeedCSAttribute()
            {
                return (m_flag & (CS_ATTRIBUTE_GET | CS_ATTRIBUTE_SET)) != 0;
            }
            public bool CanVariableGet()
            {
                return (m_flag & VARIABLE_GET) != 0;
            }
            public bool CanVariableSet()
            {
                return (m_flag & VARIABLE_SET) != 0;
            }
            public bool CanAttributeGet()
            {
                return (m_flag & CS_ATTRIBUTE_GET) != 0;
            }
            public bool CanAttributeSet()
            {
                return (m_flag & CS_ATTRIBUTE_SET) != 0 && !IsFormula();
            }
            public bool IsFormula()
            {
                return m_type_name == "Formula";
            }
            public bool NeedCast()
            {
                return m_type_name != "FixPoint";
            }
            public bool NeedParse()
            {
                return m_type_name != "string";
            }
            public bool Transform2Crc()
            {
                return (m_flag & TRANSFORM2CRCID) != 0;
            }
        }

        class EditorComponent
        {
            public string m_name;
            public List<EditorVariable> m_variables = new List<EditorVariable>();
            public int m_flag = 0;
            public bool Need_InitializeVariable()
            {
                return (m_flag & GENERATE_InitializeVariable) != 0;
            }
            public bool Need_GetVariable()
            {
                return (m_flag & GENERATE_GetVariable) != 0;
            }
            public bool Need_SetVariable()
            {
                return (m_flag & GENERATE_SetVariable) != 0;
            }
            public bool Need_CSharpAttribute()
            {
                return (m_flag & GENERATE_CSharpAttribute) != 0;
            }
            public EditorComponent REGISTER_VARIABLE<T>(string config_name, string code_name, string code_fragment = null, int flag = Flag_Variable_GetSet_Attribute_Get)
            {
                RegisterVariableInternal<T>(config_name, code_name, code_fragment, flag);
                //EditorVariable variable = new EditorVariable();
                //System.Type type = typeof(T);
                //if (!m_real_type.TryGetValue(type, out variable.m_type_name))
                //    variable.m_type_name = type.Name;
                //variable.m_config_name = config_name;
                //variable.m_code_name = code_name;
                //variable.m_code_fragment = code_fragment;
                //variable.m_flag = flag;
                //m_variables.Add(variable);
                return this;
            }
            public EditorComponent REGISTER_VARIABLE_CRC<T>(string config_name, string code_name, string code_fragment = null, int flag = Flag_Variable_GetSet_Attribute_Get)
            {
                EditorVariable variable = RegisterVariableInternal<T>(config_name, code_name, code_fragment, flag);
                variable.m_flag |= TRANSFORM2CRCID;
                variable.m_type_name = "int";
                return this;
            }
            EditorVariable RegisterVariableInternal<T>(string config_name, string code_name, string code_fragment, int flag)
            {
                EditorVariable variable = new EditorVariable();
                System.Type type = typeof(T);
                if (!m_real_type.TryGetValue(type, out variable.m_type_name))
                    variable.m_type_name = type.Name;
                variable.m_config_name = config_name;
                variable.m_code_name = code_name;
                variable.m_code_fragment = code_fragment;
                variable.m_flag = flag;
                m_variables.Add(variable);
                return variable;
            }
        }

        static bool m_logic = true;
        static List<EditorComponent> m_logic_componnets = new List<EditorComponent>();
        static List<EditorComponent> m_render_componnets = new List<EditorComponent>();

        static EditorComponent REGISTER_COMPONENT<T>(int flag = Flag_LogicComponent)
        {
            EditorComponent new_cmp = new EditorComponent();
            new_cmp.m_name = typeof(T).Name;
            new_cmp.m_flag = flag;
            if (m_logic)
                m_logic_componnets.Add(new_cmp);
            else
                m_render_componnets.Add(new_cmp);
            return new_cmp;
        }

        [MenuItem("FrameWork/Generate Combat Component Code", false, 1001)]
        public static void GenerateAll()
        {
            StreamWriter writer = new StreamWriter("Assets/Scripts/CombatModule/LogicWorld/Object/ComponentTypeRegistryExt.cs");
            writer.Write(
@"using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{");

            m_logic_componnets.Clear();
            m_render_componnets.Clear();
            InitLogicComponents();
            InitRenderComponents();

            Generate_ComponentTypeRegistry_RelatedCode(writer);

            m_logic = true;
            int count = m_logic_componnets.Count;
            for (int i = 0; i < count; ++i)
            {
                if (i > 0)
                    writer.Write("\r\n\r\n");
                else
                    writer.Write("\r\n");
                GenerateOneComponent(writer, m_logic_componnets[i]);
            }

            m_logic = false;
            writer.Write("\r\n"); //writer.Write("\r\n\r\n#if COMBAT_CLIENT");
            count = m_render_componnets.Count;
            for (int i = 0; i < count; ++i)
            {
                if (i > 0)
                    writer.Write("\r\n\r\n");
                else
                    writer.Write("\r\n");
                GenerateOneComponent(writer, m_render_componnets[i]);
            }
            //writer.Write("\r\n#endif");

            m_logic_componnets.Clear();
            m_render_componnets.Clear();
            writer.Write("\r\n}");
            writer.Flush();
            writer.Close();
            writer = null;
        }

        static void Generate_ComponentTypeRegistry_RelatedCode(StreamWriter writer)
        {
            writer.Write("\r\n    public partial class ComponentTypeRegistry\r\n    {");
            Generate_RegisterDefaultComponents(writer);
            //Generate_ActivateAllRegisteredComponents(writer);
            writer.Write("\r\n    }\r\n");
        }

        static void Generate_RegisterDefaultComponents(StreamWriter writer)
        {
            writer.Write(
@"
        static public void RegisterDefaultComponents()
        {
            if (ms_default_components_registered)
                return;
            ms_default_components_registered = true;
");
            for (int i = 0; i < m_logic_componnets.Count; ++i)
            {
                writer.Write("\r\n            Register<");
                writer.Write(m_logic_componnets[i].m_name);
                writer.Write(">(false);");
            }
            writer.Write("\r\n\r\n#if COMBAT_CLIENT");
            for (int i = 0; i < m_render_componnets.Count; ++i)
            {
                writer.Write("\r\n            Register<");
                writer.Write(m_render_componnets[i].m_name);
                writer.Write(">(true);");
            }
            writer.Write("\r\n#endif\r\n        }");
        }

        //static void Generate_ActivateAllRegisteredComponents(StreamWriter writer)
        //{
        //    writer.Write("\r\n        static public void ActivateAllRegisteredComponents()\r\n        {");
        //    writer.Write("\r\n            int id = 0;");
        //    for (int i = 0; i < m_logic_componnets.Count; ++i)
        //    {
        //        writer.Write("\r\n            id = ");
        //        writer.Write(m_logic_componnets[i].m_name);
        //        writer.Write(".ID;");
        //    }
        //    writer.Write("\r\n#if COMBAT_CLIENT");
        //    for (int i = 0; i < m_render_componnets.Count; ++i)
        //    {
        //        writer.Write("\r\n            id = ");
        //        writer.Write(m_logic_componnets[i].m_name);
        //        writer.Write(".ID;");
        //    }
        //    writer.Write("\r\n#endif\r\n        }");
        //}

        static void GenerateOneComponent(StreamWriter writer, EditorComponent component)
        {
            writer.Write("    public partial class ");
            writer.Write(component.m_name);
            writer.Write("\r\n    {\r\n        public const int ID = ");
            writer.Write(((int)CRC.Calculate(component.m_name)).ToString());
            writer.Write(";");
            if (component.m_variables.Count > 0)
                GenerateVariableRelatedCode(writer, component);
            writer.Write("\r\n    }");
        }

        static void GenerateVariableRelatedCode(StreamWriter writer, EditorComponent component)
        {
            int code_variable_count = 0;
            int attribute_count = 0;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_name == null)
                    continue;
                if (variable.NeedDeclareVariable())
                {
                    ++code_variable_count;
                    writer.Write("\r\n        public const int ");
                    writer.Write(variable.m_code_name);
                    writer.Write(" = ");
                    writer.Write(((int)CRC.Calculate(variable.m_config_name)).ToString());
                    writer.Write(";");
                }
                if (variable.NeedCSAttribute())
                {
                    ++attribute_count;
                }
            }

            bool generate = code_variable_count > 0 || component.Need_InitializeVariable();
            if (m_logic == false && generate)
                writer.Write("\r\n\r\n#if COMBAT_CLIENT");

            if (code_variable_count > 0)
                Generate_StaticConstructor(writer, component);
            if (component.Need_InitializeVariable())
                Generate_InitializeVariable(writer, component);
            if (code_variable_count > 0 && component.Need_GetVariable())
                Generate_GetVariable(writer, component);
            if (code_variable_count > 0 && component.Need_SetVariable())
                Generate_SetVariable(writer, component);
            if (attribute_count > 0 && component.Need_CSharpAttribute())
                Generate_CSharpAttribute(writer, component);

            if (m_logic == false && generate)
                writer.Write("\r\n#endif");
        }

        static void Generate_StaticConstructor(StreamWriter writer, EditorComponent component)
        {
            writer.Write("\r\n\r\n        static ");
            writer.Write(component.m_name);
            writer.Write("()\r\n        {");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_name == null || !variable.NeedDeclareVariable())
                    continue;
                writer.Write("\r\n            ComponentTypeRegistry.RegisterVariable(");
                writer.Write(variable.m_code_name);
                writer.Write(", ID);");
            }
            writer.Write("\r\n        }");
        }

        static void Generate_InitializeVariable(StreamWriter writer, EditorComponent component)
        {
            bool need = false;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (!variable.CanVariableInit())
                    continue;
                need = true;
                break;
            }
            if (!need)
                return;
            //        public override void InitializeVariable(Dictionary<string, string> variables)
            //        {
            //            string value;
            //            if (variables.TryGetValue("x", out value))
            //                m_current_position.x = int.Parse(value);
            //        }
            writer.Write(
@"

        public override void InitializeVariable(Dictionary<string, string> variables)
        {
            string value;");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (!variable.CanVariableInit())
                    continue;
                writer.Write("\r\n            if (variables.TryGetValue(\"");
                writer.Write(variable.m_config_name);
                writer.Write("\", out value))\r\n                ");
                writer.Write(variable.m_code_fragment);
                if (variable.IsFormula())
                {
                    writer.Write(".Compile(value);");
                }
                //else if (variable.NeedParse())
                //{
                //    if (variable.Transform2Crc())
                //        writer.Write(" = (int)CRC.Calculate(");
                //    else
                //        writer.Write(" = ");
                //    writer.Write(variable.m_type_name);
                //    if (variable.Transform2Crc())
                //        writer.Write(".Parse(value));");
                //    else
                //        writer.Write(".Parse(value);");
                //}
                //else
                //{
                //    if (variable.Transform2Crc())
                //        writer.Write(" = (int)CRC.Calculate(value);");
                //    else
                //        writer.Write(" = value;");
                //}
                else if (variable.Transform2Crc())
                {
                    writer.Write(" = (int)CRC.Calculate(value);");
                }
                else if (variable.NeedParse())
                {
                    writer.Write(" = ");
                    writer.Write(variable.m_type_name);
                    writer.Write(".Parse(value);");
                }
                else
                {
                    writer.Write(" = value;");
                }
            }
            writer.Write("\r\n        }");
        }

        static void Generate_GetVariable(StreamWriter writer, EditorComponent component)
        {
            bool need = false;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || !variable.CanVariableGet())
                    continue;
                need = true;
                break;
            }
            if (!need)
                return;
            //        public override bool GetVariable(int id, out FixPoint value)
            //        {
            //            switch (id)
            //            {
            //            case VID_X:
            //                value = m_current_position.x;
            //                return true;
            //            default:
            //                value = FixPoint.Zero;
            //                return false;
            //            }
            //        }
            writer.Write(
@"

        public override bool GetVariable(int id, out FixPoint value)
        {
            switch (id)
            {");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || !variable.CanVariableGet())
                    continue;
                writer.Write("\r\n            case ");
                writer.Write(variable.m_code_name);
                writer.Write(":\r\n                value = ");
                if (variable.IsFormula())
                {
                    writer.Write(variable.m_code_fragment);
                    writer.Write(".Evaluate(this);");
                }
                else if (variable.NeedCast())
                {
                    writer.Write("(FixPoint)(");
                    writer.Write(variable.m_code_fragment);
                    writer.Write(");");
                }
                else
                {
                    writer.Write(variable.m_code_fragment);
                    writer.Write(";");
                }
                writer.Write("\r\n                return true;");
            }
            writer.Write(
@"
            default:
                value = FixPoint.Zero;
                return false;
            }
        }");
        }

        static void Generate_SetVariable(StreamWriter writer, EditorComponent component)
        {
            bool need = false;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || !variable.CanVariableSet() || variable.IsFormula())
                    continue;
                need = true;
                break;
            }
            if (!need)
                return;
            //        public override bool SetVariable(int id, FixPoint value)
            //        {
            //            switch (id)
            //            {
            //            case VID_X:
            //                m_current_position.x = value;
            //                return true;
            //            default:
            //                return false;
            //            }
            //        }
            writer.Write(
@"

        public override bool SetVariable(int id, FixPoint value)
        {
            switch (id)
            {");
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || !variable.CanVariableSet() || variable.IsFormula())
                    continue;
                writer.Write("\r\n            case ");
                writer.Write(variable.m_code_name);
                writer.Write(":\r\n                ");
                writer.Write(variable.m_code_fragment);
                if (variable.NeedCast())
                {
                    writer.Write(" = (");
                    writer.Write(variable.m_type_name);
                    writer.Write(")value;");
                }
                else
                {
                    writer.Write(" = value;");
                }
                writer.Write("\r\n                return true;");
            }
            writer.Write(
@"
            default:
                return false;
            }
        }");
        }

        static void Generate_CSharpAttribute(StreamWriter writer, EditorComponent component)
        {
            bool need = false;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || (!variable.CanAttributeGet() && !variable.CanAttributeSet()))
                    continue;
                need = true;
                break;
            }
            if (!need)
                return;

            writer.Write("\r\n\r\n#region GETTER/SETTER");
            //        public FixPoint X
            //        {
            //            get { return m_current_position.x; }
            //            set { m_current_position.x = value; }
            //        }
            bool first = true;
            for (int i = 0; i < component.m_variables.Count; ++i)
            {
                EditorVariable variable = component.m_variables[i];
                if (variable.m_code_fragment == null || variable.m_code_name == null || (!variable.CanAttributeGet() && !variable.CanAttributeSet()))
                    continue;
                if (first)
                {
                    writer.Write("\r\n        public ");
                    first = false;
                }
                else
                {
                    writer.Write("\r\n\r\n        public ");
                }
                if (variable.IsFormula())
                {
                    writer.Write("FixPoint ");
                }
                else
                {
                    writer.Write(variable.m_type_name);
                    writer.Write(" ");
                }
                writer.Write(variable.m_code_name.Substring(4));
                writer.Write("\r\n        {");
                if (variable.CanAttributeGet())
                {
                    writer.Write("\r\n            get { return ");
                    if (variable.IsFormula())
                    {
                        writer.Write(variable.m_code_fragment);
                        writer.Write(".Evaluate(this); }");
                    }
                    else
                    {
                        writer.Write(variable.m_code_fragment);
                        writer.Write("; }");
                    }
                }
                if (variable.CanAttributeSet())
                {
                    writer.Write("\r\n            set { ");
                    writer.Write(variable.m_code_fragment);
                    writer.Write(" = value; }");
                }
                writer.Write("\r\n        }");
            }
            writer.Write("\r\n#endregion");
        }
    }
}
