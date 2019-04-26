using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionEntity;
using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionRelationshipGroup;

using WlPlaygroundImcklShared.Mission.CopClearGangzone;
using WlPlaygroundImcklShared.Extension;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionPed
{
    public class MissionPedController
    {

        public const string DecorIsMissionPed = "DecorIsMissionPed";
        public const string DecorMissionInfoIndex = "DecorMissionInfoIndex";
        public const string DecorMissionPedInfoIndex = "DecorMissionPedInfoIndex";

        private static Dictionary<int, bool> IsPedHandleActivated = new Dictionary<int, bool>();

        /// <summary>
        /// 初始化
        /// </summary>
        public MissionPedController()
        {
            EntityDecoration.RegisterProperty(DecorIsMissionPed, DecorationType.Bool);
            EntityDecoration.RegisterProperty(DecorMissionInfoIndex, DecorationType.Int);
            EntityDecoration.RegisterProperty(DecorMissionPedInfoIndex, DecorationType.Int);
        }

        /// <summary>
        /// Run tolerant tick 
        /// </summary>
        /// <returns></returns>
        public async Task TolerantTick()
        {
            await BaseScript.Delay(1000);
            foreach (var ped in World.GetAllPeds())
            {
                if (!(ped is null))
                {
                    // 属于任务敌人
                    if (ped.HasDecor(DecorIsMissionPed) && ped.HasDecor(DecorMissionInfoIndex) && ped.HasDecor(DecorMissionPedInfoIndex))
                    {
                        ActivatePedDetailOnlyOnce(ped);
                        ResetPedPositionWhenOutOfBoundary(ped);
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有任务人物List
        /// </summary>
        /// <returns></returns>
        public static List<Ped> GetAllMissionPeds()
        {
            return World.GetAllPeds()
                .Select(ped => ped)
                .Where(ped => ped.HasDecor(DecorIsMissionPed) && ped.HasDecor(DecorMissionInfoIndex) && ped.HasDecor(DecorMissionPedInfoIndex))
                .ToList();
        }

        /// <summary>
        /// 无论任务是否激活/进行, 超出行动范围的Ped传送回原刷新点
        /// </summary>
        /// <param name="ped"></param>
        private void ResetPedPositionWhenOutOfBoundary(Ped ped)
        {
            var missionInfoIndex = ped.GetDecor<int>(DecorMissionInfoIndex);
            var missionPedInfoIndex = ped.GetDecor<int>(DecorMissionPedInfoIndex);
            var missionInfo = CopClearGangzone.MissionsInfo[missionInfoIndex];
            var pedInfo = CopClearGangzone.MissionsInfo[missionInfoIndex].PedsInfo[missionPedInfoIndex];

            if (ped.Position.DistanceToSquared(missionInfo.RangeInfo.Position) > Math.Pow(missionInfo.RangeInfo.Radius, 2)) 
            {
                ped.Position = pedInfo.Position;
                ped.Task.WanderAround(pedInfo.Position, pedInfo.WanderRange);
            }
        }

        /// <summary>
        /// 激活/设置任务人物详细状态
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private void ActivatePedDetailOnlyOnce(Ped ped)
        {
            var isPedActivated = IsPedHandleActivated.GetValueOrDefault(ped.Handle, false);
            // 仅初始化一次
            if (!isPedActivated)
            {
                if (ped.HasDecor(DecorIsMissionPed) && ped.HasDecor(DecorMissionInfoIndex) && ped.HasDecor(DecorMissionPedInfoIndex))
                {
                    Debug.WriteLine($"Setting ped {ped.Handle}, {ped.IsInvincible}");
                    
                    var missionInfoIndex = ped.GetDecor<int>(DecorMissionInfoIndex);
                    var missionPedInfoIndex = ped.GetDecor<int>(DecorMissionPedInfoIndex);
                    var pedInfo = CopClearGangzone.MissionsInfo[missionInfoIndex].PedsInfo[missionPedInfoIndex];

                    // Debug.WriteLine($"{JsonConvert.SerializeObject(pedInfo)}");

                    ped.IsPersistent = true;

                    ped.IsInvincible = false;

                    // 设置分组, 并仅任务玩家组可对任务人物造成伤害
                    MissionRelationship.SetPedAsEnemy(ped, true);

                    ped.Accuracy = pedInfo.Accuracy;
                    ped.MaxHealthFloat = pedInfo.Health;
                    ped.HealthFloat = pedInfo.Health;
                    // 爆头击杀
                    ped.CanSufferCriticalHits = false;

                    ped.CanSwitchWeapons = true;
                    // 是否会倒地挣扎?
                    ped.CanWrithe = true;
                    // 死亡掉落物器
                    ped.DropsWeaponsOnDeath = false;
                    ped.DiesInstantlyInWater = false;
                    ped.DrownsInSinkingVehicle = false;
                    ped.DrownsInWater = false;

                    ped.IsInvincible = false;
                    ped.IsVisible = true;
                    ped.IsFireProof = true;
                    ped.CanRagdoll = false;

                    ped.FatalInjuryHealthThreshold = 0;
                    ped.AlwaysDiesOnLowHealth = true;
                    ped.InjuryHealthThreshold = 50;

                    // 控制AI动作?
                    //ped.Euphoria.BodyWrithe.
                    //ped.Euphoria.SetCharacterHealth.CharacterHealth
                    // AI射击倾向/模式 - see API.SetPedFiringPattern
                    //ped.FiringPattern
                    // AI视野相关
                    // ped.IsInAngledArea
                    // AI警戒范围相关
                    // ped.IsInArea
                    // ped.IsInRangeOf
                    // ped.IsNearEntity
                    // ped.IsOnScreen
                    // 是否在警车
                    //ped.IsInPoliceVehicle
                    // ???
                    // API.IsEntityOccluded
                    // 是否渲染?
                    // ped.IsRendered
                    // cash
                    // ped.Money
                    // networkid
                    //ped.NetworkId
                    // ped group relative
                    //var x = ped.PedGroup;
                    //x.Leader
                    // 四元数
                    //ped.Quaternion
                    // relationship group relative
                    //var x = ped.RelationshipGroup;
                    // 复活
                    //ped.Resurrect
                    // set the rate this Ped will shoot at.
                    //ped.ShootRate

                    ped.IsEnemy = true;
                    // ped.IsOnlyDamagedByPlayer = false;

                    //ped.BlockPermanentEvents = true;
                    //API.TaskSetBlockingOfNonTemporaryEvents(ped.Handle, true);

                    pedInfo.Weapons.ForEach(w => ped.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));

                    if (pedInfo.IsWanderAround)
                    {
                        ped.Task.WanderAround(pedInfo.Position, pedInfo.WanderRange);
                    }

                    ped.FiringPattern = FiringPattern.BurstFireBursts;

                    IsPedHandleActivated[ped.Handle] = true;
                }
            }
            
        }

        /// <summary>
        /// 创建任务人物(注意: 多客户端创建会重复创建)
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        /// <param name="missionPedInfoIndex"></param>
        /// <returns></returns>
        public async Task<Ped> Create(int missionInfoIndex, int missionPedInfoIndex)
        {
            
            var pedInfo = CopClearGangzone.MissionsInfo[missionInfoIndex].PedsInfo[missionPedInfoIndex];

            var model = new Model(pedInfo.PedHash);
            await model.Request(1000 * 10);

            if (!model.IsLoaded)
                return null;

            var ped = await World.CreatePed(model, pedInfo.Position);
            //var ped = new Ped(API.CreatePed(26, (uint)pedInfo.PedHash, pedInfo.Position.X, pedInfo.Position.Y, pedInfo.Position.Z, 0, true, false));
            if (ped is null)
            {
                Debug.WriteLine("create ped failed");
                return null;
            }
                

            ped.SetDecor(DecorIsMissionPed, true);
            ped.SetDecor(DecorMissionInfoIndex, missionInfoIndex);
            ped.SetDecor(DecorMissionPedInfoIndex, missionPedInfoIndex);
            
            ped.IsPersistent = true;

            ped.IsInvincible = true;

            pedInfo.Weapons.ForEach(w => ped.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));

            ped.FiringPattern = FiringPattern.BurstFireBursts;

            return ped;
        }

    }
}
