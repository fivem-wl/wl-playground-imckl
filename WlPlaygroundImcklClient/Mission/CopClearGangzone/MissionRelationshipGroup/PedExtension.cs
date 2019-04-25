using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionRelationshipGroup
{
    static class PedExtension
    {
        /// <summary>
        /// 设置<seealso cref="Ped"/>组别为<seealso cref="RelationshipGroup"/>
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="relationshipGroup"></param>
        public static void SetRelationshipGroup(this Ped ped, RelationshipGroup relationshipGroup)
        {
            MissionRelationship.SetPedRelaionshipGroup(ped, relationshipGroup);
        }

        /// <summary>
        /// 设置<seealso cref="Ped"/>组别为<seealso cref="RelationshipBaseGroup"/>
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="relationshipGroup"></param>
        public static void SetRelationshipGroup(this Ped ped, RelationshipBaseGroup relationshipBaseGroup)
        {
            MissionRelationship.SetPedRelaionshipGroup(ped, (int)relationshipBaseGroup);
        }

        /// <summary>
        /// 设置<seealso cref="Ped"/>为任务敌人
        /// </summary>
        /// <param name="ped"></param>
        /// <param name="isOnlyDamagedByFighterGroup">是否仅玩家可以对其造成伤害</param>
        public static void SetAsEnemy(this Ped ped, bool isOnlyDamagedByFighterGroup)
        {
            MissionRelationship.SetPedAsEnemy(ped, isOnlyDamagedByFighterGroup);
        }

        /// <summary>
        /// 设置<seealso cref="Ped"/>为任务队友
        /// </summary>
        /// <param name="ped"></param>
        public static void SetAsAlliance(this Ped ped)
        {
            MissionRelationship.SetPedAsAlliance(ped);
        }

        /// <summary>
        /// 设置<seealso cref="Ped"/>为任务玩家
        /// </summary>
        /// <param name="ped"></param>
        public static void SetAsFighter(this Ped ped)
        {
            MissionRelationship.SetPedAsFighter(ped);
        }
    }
}
