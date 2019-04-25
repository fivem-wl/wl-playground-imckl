using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionRelationshipGroup
{
    class MissionRelationship
    {
        /// <summary>
        /// 任务敌人组
        /// </summary>
        private static RelationshipGroup MissionEnemyGroup;
        /// <summary>
        /// 任务同盟组
        /// </summary>
        private static RelationshipGroup MissionAlianceGroup;
        /// <summary>
        /// 任务玩家组
        /// </summary>
        private static RelationshipGroup MissionFighterGroup;

        /// <summary>
        /// 设置<seealso cref="Ped"/>为敌人
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="isOnlyDamagedByFighterGroup">是否仅玩家可以对其造成伤害</param>
        public static void SetPedAsEnemy(Ped ped, bool isOnlyDamagedByFighterGroup)
        {
            SetPedRelaionshipGroup(ped, MissionEnemyGroup);
            API.SetEntityOnlyDamagedByRelationshipGroup(ped.Handle, isOnlyDamagedByFighterGroup, (uint)MissionFighterGroup.Hash);
        }

        public static void SetPedAsAlliance(Ped ped)
            => SetPedRelaionshipGroup(ped, MissionAlianceGroup);

        /// <summary>
        /// 设置<seealso cref="Ped"/>为任务玩家
        /// </summary>
        /// <param name="ped"></param>
        public static void SetPedAsFighter(Ped ped)
            => SetPedRelaionshipGroup(ped, MissionFighterGroup);

        /// <summary>
        /// 预设任务组关系, should be called only once in init.
        /// </summary>
        public static void Init()
        {
            MissionEnemyGroup = World.AddRelationshipGroup("MissionEnemyGroup");
            MissionAlianceGroup = World.AddRelationshipGroup("MissionAlianceGroup");
            MissionFighterGroup = World.AddRelationshipGroup("MissionFighterGroup");

            // 默认玩家与任务玩家为同盟
            new RelationshipGroup((int)RelationshipBaseGroup.PLAYER).SetRelationshipBetweenGroups(MissionFighterGroup, Relationship.Companion, true);
            // 默认玩家与任务敌人为敌对
            new RelationshipGroup((int)RelationshipBaseGroup.PLAYER).SetRelationshipBetweenGroups(MissionEnemyGroup, Relationship.Hate, true);
            // 任务玩家与任务敌人为敌对
            MissionFighterGroup.SetRelationshipBetweenGroups(MissionEnemyGroup, Relationship.Hate, true);
            // 任务玩家与任务同盟为同盟
            MissionFighterGroup.SetRelationshipBetweenGroups(MissionAlianceGroup, Relationship.Companion, true);
            // 任务同盟与任务敌人为敌对
            MissionAlianceGroup.SetRelationshipBetweenGroups(MissionEnemyGroup, Relationship.Hate, true);
            // 任务敌人与行人为敌对
            MissionEnemyGroup.SetRelationshipBetweenGroups(new RelationshipGroup((int)RelationshipBaseGroup.COP), Relationship.Hate, true);
            MissionEnemyGroup.SetRelationshipBetweenGroups(new RelationshipGroup((int)RelationshipBaseGroup.CIVFEMALE), Relationship.Hate, true);
        }

        public static void SetPedRelaionshipGroup(Ped ped, RelationshipGroup relationshipGroup)
        {
            if (ped.RelationshipGroup != relationshipGroup)
                ped.RelationshipGroup = relationshipGroup;
        }


    }
}
