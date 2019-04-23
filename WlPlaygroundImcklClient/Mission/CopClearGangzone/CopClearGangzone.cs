using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

using WlPlaygroundImcklShared.Mission.CopClearGangzone;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{

    public delegate void StoppedEvent(string reason);

    public class MissionInstance : IDeletable
    {
        /// <summary>
        /// 任务结束事件
        /// </summary>
        public event StoppedEvent OnStop;

        /// <summary>
        /// Peds列表
        /// </summary>
        public List<Ped> Peds { get; set; } = new List<Ped>();
        /// <summary>
        /// Hotzone
        /// </summary>
        public Blip RadiusBlip { get; set; }

        /// <summary>
        /// 任务时间提醒/检测
        /// </summary>
        public CustomTimer MissionTimer { get; private set; }
        /// <summary>
        /// 设置警察为同伴的检测/时间间隔
        /// </summary>
        private CustomTimer SetCopAllianceTimer { get; set; } = new CustomTimer(1000 * 2);

        /// <summary>
        /// 任务信息
        /// </summary>
        private MissionInfo MissionInfo { get; set; }

        /// <summary>
        /// Internal IDeletable check boolean
        /// </summary>
        private bool _existed { get; set; } = false;
        /// <summary>
        /// (internal)任务是否激活/开始
        /// </summary>
        public bool IsActivated { get; private set; } = false;

        public MissionInstance(MissionInfo missionInfo)
        {
            _existed = true;

            MissionInfo = missionInfo;
        }

        public bool Exists()
        {
            return _existed;
        }

        public void Delete()
        {

            Peds?.ForEach(p =>
            {
                p.AttachedBlip?.Delete();
                p.IsPersistent = false;
                p.MarkAsNoLongerNeeded();
            });
            
            RadiusBlip?.Delete();

            if(IsActivated)
                Deactivate();

            _existed = false;

        }
        
        /// <summary>
        /// 激活/开始任务, 显示任务相关信息, 并与玩家进行关联
        /// </summary>
        public async Task Activate()
        {
            // 从服务器获取任务剩余时间
            long remainTime = 0;
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:ClientGetMissionRemainTime");
            try
            {
                remainTime = await MissionRemainTimeAsyncer.GetFromServer();
            }
            catch(TimeoutException e)
            {
                Notify.Error($"{e.Message}");
                Debug.WriteLine($"[{CopClearGangzone.ResourceName}][ERROR]{e.Message}");
                IsActivated = false;
                return;
            }

            // 任务倒计时
            MissionTimer = new CustomTimer(remainTime);

            // 设置任务玩家组别及关系
            Game.PlayerPed.RelationshipGroup = CopClearGangzone.MissionRunnerGroup;

            // 创建范围Blip
            RadiusBlip = World.CreateBlip(MissionInfo.RangeInfo.Position, MissionInfo.RangeInfo.Radius);
            RadiusBlip.Alpha = 64;
            RadiusBlip.Color = BlipColor.Red;
            RadiusBlip.ShowRoute = true;

            // 自定义玩家武器
            Game.PlayerPed.Weapons.RemoveAll();
            MissionInfo.PlayerWeapons.ForEach(
                w => Game.PlayerPed.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));
            
            Peds?.ForEach(p =>
            {
                // 显示任务人物红点
                var blip = p.AttachBlip();
                blip.IsFriendly = false;
                blip.IsFriend = false;
                blip.IsShortRange = false;
                // 设置玩家可伤害
                // p.IsOnlyDamagedByPlayer = true;
            });

            FatalDamageEvents.OnDeath += OnDeath;
            FatalDamageEvents.OnPlayerKillPed += OnPlayerKillPed;

            // Trigger server event
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:ClientActivateMission");

            // 界面/声音提醒
            var info = MissionInfo.StartNotificationInfo;
            Notify.CustomImage("CHAR_CALL911", "CHAR_CALL911", info.Message, info.Sender, info.Subject, true, 2);
            AudioPlayer.Play(AudioName.Beep);
            Subtitle.Draw(MissionInfo.HintSubtitle, (int)remainTime);

            IsActivated = true;
        }

        public void Deactivate()
        {
            RadiusBlip?.Delete();

            Peds?.ForEach(p =>
            {
                p.AttachedBlip?.Delete();
            });

            // (重置)设置任务玩家组别及关系
            Game.PlayerPed.RelationshipGroup = new RelationshipGroup((int)RelationshipBaseGroup.PLAYER);

            FatalDamageEvents.OnDeath -= OnDeath;
            FatalDamageEvents.OnPlayerKillPed -= OnPlayerKillPed;

            IsActivated = false;
        }

        private void OnDeath(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            // 如果玩家死亡, 则视为任务结束
            _OnStop("dead");
        }

        private void OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            // 任务目标死亡, 去除标记红点, 并标记为可回收资源
            Peds.Where(p => p == victim).ToList().ForEach(p => 
            {
                p.AttachedBlip?.Delete();
                p.IsPersistent = false;
                p.MarkAsNoLongerNeeded();
            });
            // 所有任务目标死亡, 则视为任务完成
            if (Peds.All(p => p.IsDead))
            {
                Debug.WriteLine("Mission complete");
                _OnStop("finish");
            }
        }

        private void _OnStop(string reason)
        {
            // 解除通缉状态
            API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
            API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
            // Trigger server event
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:ClientStopMission", reason);
            // Invoke callback
            OnStop?.Invoke(reason);
        }

        /// <summary>
        /// 作用于每帧的异步方法
        /// </summary>
        /// <returns></returns>
        public async Task EveryFrameTickAsync()
        {
            if (IsActivated)
            {
                // 显示剩余时间
                var remainTime = TimeSpan.FromMilliseconds(MissionTimer.RemainTime);
                var remainTimeString = string.Format(
                    "剩余时间 - {1:D2}:{2:D2}",
                    remainTime.Hours, remainTime.Minutes, remainTime.Seconds, remainTime.Milliseconds);
                GUI.DrawTextOnScreen(remainTimeString,
                    GUI.SafeZone.Right - 0.1f, GUI.SafeZone.Bottom - API.GetTextScaleHeight(0.4f, 1) - 0.01f, 0.4f, Alignment.Center, 1, false);
                await Task.FromResult(0);
            }
            else
            {
                await BaseScript.Delay(1000);
            }
        }

        /// <summary>
        /// 较为宽松的检测的异步方法
        /// </summary>
        /// <returns></returns>
        public async Task TickAsync()
        {
            if (IsActivated)
            {
                // 如果仍有敌人存活, 则保持通缉状态
                if (Peds.Any(p => p.IsAlive))
                {
                    API.SetPlayerWantedLevel(Game.Player.Handle, 5, false);
                    API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
                }

                // 设置警察为同伴(仅当前刷新的警察)
                if (SetCopAllianceTimer.IsMeet())
                {
                    foreach (var ped in World.GetAllPeds())
                    {
                        if (ped.IsPlayer)
                            continue;
                        var pedType = API.GetPedType(ped.Handle);
                        if (pedType == 6 || pedType == 27 || pedType == 29)
                        {
                            if (ped.RelationshipGroup != CopClearGangzone.MissionAlianceGroup)
                                ped.RelationshipGroup = CopClearGangzone.MissionAlianceGroup;
                        }
                    }
                }

                await BaseScript.Delay(1000);
            }
            else
            {
                await BaseScript.Delay(1000);
            }
        }
    }

    public class CopClearGangzone : BaseScript
    {

        public static string ResourceName = "CopClearGangzone";

        private List<MissionInfo> MissionsInfo;
        private static MissionInstance MissionInstance;

        public static async Task<bool> ActivateCurrentMission()
        {
            if (MissionInstance?.Exists() ?? false)
            {
                if (!MissionInstance.IsActivated)
                {
                    await MissionInstance.Activate();
                    return true;
                }
                else
                {
                    Notify.Alert("任务进行中");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"[{ResourceName}]Not prepare for activating");
                Notify.Info("没有可疑的区域暴乱活动");
                return false;
            }
                
        }

        private async Task<bool> CreateMission(int missionInfoIndex)
        {
            Debug.WriteLine($"CreateMission {missionInfoIndex}");

            // var index = missionInfoIndex;
            var missionInfo = MissionsInfo[missionInfoIndex];

            DestroyMission();
            MissionInstance = new MissionInstance(missionInfo);

            // 创建任务人物
            foreach (var pedInfo in missionInfo.PedsInfo)
            {
                var ped = await CreateMissionPed(pedInfo);
                // 如果创建失败, 则释放资源并返回创建失败
                if (ped is null)
                {
                    MissionInstance.Delete();
                    return false;
                }
                else
                {
                    MissionInstance.Peds.Add(ped);
                }
            }

            // 注册任务人物至服务端(客户端同步)
            //MissionInstance.Peds.ForEach(ped =>
            //{
            //    Debug.WriteLine(
            //        $"[{ResourceName}]Try set {ped.Handle} as networked - " +
            //        $"{API.NetworkDoesEntityExistWithNetworkId(ped.Handle)} {API.NetworkGetNetworkIdFromEntity(ped.Handle)}");
            //    //while (!API.NetworkDoesEntityExistWithNetworkId(ped.Handle))
            //    //{
            //    var networkId = API.NetworkGetNetworkIdFromEntity(ped.Handle);
            //    Debug.WriteLine($"[{ResourceName}]Trying... {ped.Handle} as networked - {API.NetworkDoesEntityExistWithNetworkId(ped.Handle)}");
            //    Debug.WriteLine($"[{ResourceName}]Trying... {ped.Handle} Ped to Net - {API.PedToNet(ped.Handle)}");
            //    API.SetNetworkIdCanMigrate(networkId, true);
            //    API.SetNetworkIdExistsOnAllMachines(networkId, true);
            //    API.NetworkRegisterEntityAsNetworked(API.PedToNet(ped.Handle));
            //    API.NetworkRegisterEntityAsNetworked(ped.Handle);
            //    Debug.WriteLine($"[{ResourceName}]Trying... {ped.Handle} as networked - {API.NetworkDoesEntityExistWithNetworkId(ped.Handle)}");
            //    Debug.WriteLine($"[{ResourceName}]Trying... {ped.Handle} Ped to Net - {API.PedToNet(ped.Handle)}");
            //    //await Delay(1000);
            //    //}
            //    //API.NetworkSetNetworkIdDynamic(API.NetworkGetNetworkIdFromEntity(ped.Handle), false);
            //    Debug.WriteLine($"[{ResourceName}]Set - {API.NetworkGetNetworkIdFromEntity(ped.Handle)}");
            //});

            Tick += MissionInstance.TickAsync;
            Tick += MissionInstance.EveryFrameTickAsync;

            MissionInstance.OnStop += StopMission;

            // 将创建记录发送至Server
            //var x = MissionInstance.Peds.Select(p => new { p.NetworkId, p.Handle }).ToList();
            //foreach (var i in x)
            //{
            //    Debug.WriteLine($"{i.Handle} {i.NetworkId}");

            //}
            //TriggerServerEvent($"{ResourceName}:RecordCreatedPed", x);
            
            
            return true;

        }

        private void DestroyMission()
        {

            if (!(MissionInstance is null))
            {
                Tick -= MissionInstance.TickAsync;
                Tick -= MissionInstance.EveryFrameTickAsync;

                MissionInstance.OnStop -= StopMission;

                MissionInstance.Delete();
            }

        }

        private async void StopMission(string reason)
        {
            if (MissionInstance.IsActivated)
            {
                await Delay(1000 * 3);
                API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
                API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
                await HintOnStopMission(reason);
            }
            DestroyMission();
        }

        /// <summary>
        /// 任务结束提示
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task HintOnStopMission(string reason)
        {
            switch (reason)
            {
                case "finish":
                    AudioPlayer.Play(AudioName.MissionComplete);
                    await ScaleformDrawer.DrawCenterBar(API.GetLabelText("BM_PASS"), "~y~评分: 100~s~");
                    break;
                case "dead":
                case "timeup":
                default:
                    AudioPlayer.Play(AudioName.Wasted);
                    await ScaleformDrawer.DrawCenterBar(API.GetLabelText("REPLAY_T"), "~y~评分: 0~s~");
                    break;
            }
        }

        /// <summary>
        /// 创建任务人物
        /// </summary>
        /// <param name="pedInfo"></param>
        /// <returns></returns>
        private async Task<Ped> CreateMissionPed(MissionPedInfo pedInfo)
        {
            var model = new Model(pedInfo.PedHash);
            await model.Request(1000 * 10);

            if (!model.IsLoaded)
                return null;

            var ped = await World.CreatePed(model, pedInfo.Position);
            // var ped = new Ped(API.CreatePed(26, (uint)pedInfo.PedHash, pedInfo.Position.X, pedInfo.Position.Y, pedInfo.Position.Z, 0, false, false));
            if (ped is null)
                return null;

            //API.NetworkGetServerTime
            //API.NetworkIsSessionStarted();
            //API.GetIsLoadingScreenActive();

            // 设置分组
            ped.RelationshipGroup = MissionEnemyGroup;
            // 仅特定组可以对任务人物造成伤害
            API.SetEntityOnlyDamagedByRelationshipGroup(ped.Handle, true, (uint)MissionRunnerGroup.Hash);
            // API.SetEntityOnlyDamagedByRelationshipGroup(ped.Handle, true, (uint)MissionAlianceGroup.Hash);

            ped.Accuracy = pedInfo.Accuracy;
            ped.MaxHealthFloat = pedInfo.Health;
            ped.HealthFloat = pedInfo.Health;
            ped.CanSufferCriticalHits = !pedInfo.IsHeadshotImmute;

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

            // persistent
            ped.IsPersistent = true;

            pedInfo.Weapons.ForEach(w => ped.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));

            if (pedInfo.IsWanderAround)
            {
                ped.Task.WanderAround(pedInfo.Position, pedInfo.WanderRange);
            }

            ped.FiringPattern = FiringPattern.BurstFireBursts;

            return ped;

        }

        /// <summary>
        /// 任务敌人组
        /// </summary>
        public static RelationshipGroup MissionEnemyGroup;
        /// <summary>
        /// 任务同盟组
        /// </summary>
        public static RelationshipGroup MissionAlianceGroup;
        /// <summary>
        /// 任务玩家组
        /// </summary>
        public static RelationshipGroup MissionRunnerGroup;

        private static Menus.MainMenu MainMenu { get; set; }
        private async Task ToggleMenuCheckAsync()
        {
            if (Game.PlayerPed.IsInPoliceVehicle && API.IsControlPressed(0, (int)Control.Context))
            {
                MainMenu?.GetMenu()?.OpenMenu();
            }
            await Delay(1000);
        }

        private async Task LoadAll(string MissionsInfoJson)
        {

            Debug.WriteLine($"[{ResourceName}]Loading MissionsInfo.");

            // 反序列化
            MissionsInfo = JsonConvert.DeserializeObject<List<MissionInfo>>(MissionsInfoJson);
            await Task.FromResult(0);

        }

        /// <summary>
        /// 加载模型(延迟执行)
        /// </summary>
        /// <returns></returns>
        private async Task LoadModel()
        {
            
            await Delay(1000 * 30);
            Debug.WriteLine($"[{ResourceName}]Loading Models.");

            // 预加载人物模型
            MissionsInfo
                .SelectMany(mi => mi.PedsInfo.Select(pi => pi.PedHash))
                .Distinct()
                .ToList()
                .ForEach(async pedHash =>
                {
                    var loaded = await new Model(pedHash).Request(1000 * 60);
                    Debug.WriteLine($"[{ResourceName}]Ped model {pedHash} load {(loaded ? "succeed" : "failed")}.");
                });

            // 预加载武器模型
            //var weaponHashUnion = Enumerable.Union(
            //    MissionsInfo.SelectMany(mi => mi.PlayerWeapons.Select(pw => pw.Hash)),
            //    MissionsInfo.SelectMany(mi => mi.PedsInfo.SelectMany(pi => pi.Weapons.Select(wi => wi.Hash))));
            //weaponHashUnion
            //    .ToList().ForEach(async weaponHash =>
            //    {
            //        var loaded = await new Model((WeaponHash)weaponHash).Request(1000 * 60);
            //        Debug.WriteLine($"[{ResourceName}]Weapon model {weaponHash} load {(loaded ? "succeed" : "failed")}.");
            //    });

            Tick -= LoadModel;

        }

        private async Task BroadcastMission(int missionInfoIndex)
        {
            var info = MissionsInfo[missionInfoIndex].ScheduleNotificationInfo;
            // 发出提醒
            Notify.CustomImage("CHAR_BUGSTARS", "CHAR_BUGSTARS",
                info.Message, info.Sender, info.Subject,
                true, 2);
            AudioPlayer.Play(AudioName.Beep);
            await Task.FromResult(0);
        }

        public CopClearGangzone()
        {

            Game.PlayerPed.IsInvincible = true;
            
            EventHandlers.Add($"{ResourceName}:LoadAll", new Func<string, Task>(LoadAll));
            EventHandlers.Add($"{ResourceName}:CreateMission", new Func<int, Task<bool>>(CreateMission));
            EventHandlers.Add($"{ResourceName}:StopMission", new Action<string>(StopMission));
            EventHandlers.Add($"{ResourceName}:BroadcastMission", new Func<int, Task>(BroadcastMission));
            EventHandlers.Add($"{ResourceName}:ClientGetMissionRemainTime", new Action<long>(MissionRemainTimeAsyncer.Set));
            
            // 从服务器加载任务信息
            TriggerServerEvent($"{ResourceName}:LoadAll");

            Tick += ToggleMenuCheckAsync;
            Tick += LoadModel;

            MainMenu = new Menus.MainMenu();
            MainMenu.CreateMenu();
            
            PresetRelationshipGroup();

            //API.RegisterCommand("myrelationship", new Action<int, List<object>, string>((source, args, raw) =>
            //{
            //    Debug.WriteLine($"My relationship: {Game.PlayerPed.RelationshipGroup.Hash}");
            //    foreach (var ped in World.GetAllPeds())
            //    {
            //        Debug.WriteLine($"{ped.Model}, {ped.Handle}, {ped.RelationshipGroup.Hash}");
            //    }
            //}), false);

        }

        /// <summary>
        /// 预设任务组关系, should be called only once in init.
        /// </summary>
        private static void PresetRelationshipGroup()
        {

            MissionEnemyGroup = World.AddRelationshipGroup("MissionEnemyGroup");
            MissionAlianceGroup = World.AddRelationshipGroup("MissionAlianceGroup");
            MissionRunnerGroup = World.AddRelationshipGroup("MissionRunnerGroup");

            // 默认玩家与任务玩家为同盟
            new RelationshipGroup((int)RelationshipBaseGroup.PLAYER).SetRelationshipBetweenGroups(MissionRunnerGroup, Relationship.Companion, true);
            // 默认玩家与任务敌人为敌对
            new RelationshipGroup((int)RelationshipBaseGroup.PLAYER).SetRelationshipBetweenGroups(MissionEnemyGroup, Relationship.Hate, true);
            // 任务玩家与任务敌人为敌对
            MissionRunnerGroup.SetRelationshipBetweenGroups(MissionEnemyGroup, Relationship.Hate, true);
            // 任务玩家与任务同盟为同盟
            MissionRunnerGroup.SetRelationshipBetweenGroups(MissionAlianceGroup, Relationship.Companion, true);
            // 任务同盟与任务敌人为敌对
            MissionAlianceGroup.SetRelationshipBetweenGroups(MissionEnemyGroup, Relationship.Hate, true);
            // 任务敌人与行人为敌对
            MissionEnemyGroup.SetRelationshipBetweenGroups(new RelationshipGroup((int)RelationshipBaseGroup.COP), Relationship.Hate, true);
            MissionEnemyGroup.SetRelationshipBetweenGroups(new RelationshipGroup((int)RelationshipBaseGroup.CIVFEMALE), Relationship.Hate, true);
        }

    }

}
