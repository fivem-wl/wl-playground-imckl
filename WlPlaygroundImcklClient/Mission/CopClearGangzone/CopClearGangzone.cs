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

using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionRelationshipGroup;
using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionEntity;
using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionPed;
using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionAudio;
using WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionGUI;

using WlPlaygroundImcklShared.Mission.CopClearGangzone;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    
    public delegate void PlayerJoinMissionEvent();
    public delegate void PlayerLeaveMissionEvent(string reason);

    public class MissionInstance : IDeletable
    {

        private MissionRemainTimeAsyncer MissionRemainTimeAsyncer = MissionRemainTimeAsyncer.Instance;

        /// <summary>
        /// 玩家加入任务事件
        /// </summary>
        public event PlayerJoinMissionEvent OnPlayerJoinMission;
        /// <summary>
        /// 玩家离开任务事件
        /// </summary>
        public event PlayerLeaveMissionEvent OnPlayerLeaveMission;

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
        /// IDeletable check boolean
        /// </summary>
        private bool _existed { get; set; } = false;
        /// <summary>
        /// 任务是否已经激活
        /// </summary>
        public bool IsActivated { get; private set; } = false;
        /// <summary>
        /// 玩家是否加入任务
        /// </summary>
        public bool IsJoined { get; private set; } = false;

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

            Leave("delete");
            Deactivate();

            MissionPedController.GetAllMissionPeds().ForEach(ped =>
            {
                ped.AttachedBlip?.Delete();
                ped.IsPersistent = false;
                ped.MarkAsNoLongerNeeded();
            });
            
            RadiusBlip?.Delete();

            _existed = false;

        }
        
        private void RevertBaseState()
        {
            var missionPeds = MissionPedController.GetAllMissionPeds();

            #region 隐藏范围Blip
            if (RadiusBlip is null)
                RadiusBlip = World.CreateBlip(MissionInfo.RangeInfo.Position, MissionInfo.RangeInfo.Radius);
            RadiusBlip.Alpha = 0;
            RadiusBlip.Color = BlipColor.Red;
            RadiusBlip.ShowRoute = false;
            #endregion

            // 隐藏敌人位置
            missionPeds
                .Where(ped => ped.IsAlive)
                .ToList().ForEach(ped =>
                {
                    var blip = ped.AttachedBlip;
                    if (blip is null)
                        blip = ped.AttachBlip();
                    blip.Color = BlipColor.Red;
                    blip.IsShortRange = true;
                    blip.Alpha = 0;
                });
        }

        /// <summary>
        /// 激活任务
        /// </summary>
        public void Activate()
        {
            RetainActivateState();
            IsActivated = true;
        }

        /// <summary>
        /// 保持激活状态
        /// </summary>
        private void RetainActivateState()
        {
            var missionPeds = MissionPedController.GetAllMissionPeds();

            #region 显示范围Blip
            if (RadiusBlip is null)
                RadiusBlip = World.CreateBlip(MissionInfo.RangeInfo.Position, MissionInfo.RangeInfo.Radius);
            RadiusBlip.Alpha = 64;
            RadiusBlip.Color = BlipColor.Red;
            RadiusBlip.ShowRoute = false;
            #endregion

            // 范围内显示敌人位置
            missionPeds
                .Where(ped => ped.IsAlive)
                .ToList().ForEach(ped =>
            {
                var blip = ped.AttachedBlip;
                if (blip is null)
                    blip = ped.AttachBlip();
                blip.Color = BlipColor.Red;
                blip.IsShortRange = true;
                blip.Alpha = 256;
            });
        }

        /// <summary>
        /// 解除激活任务 - todo
        /// </summary>
        public void Deactivate()
        {
            RevertBaseState();
            IsActivated = false;
        }

        /// <summary>
        /// 玩家加入任务, 显示任务相关信息, 并与玩家进行关联
        /// </summary>
        public async Task Join()
        {
            #region 从服务器获取任务剩余时间, 并开始倒计时
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
                return;
            }
            // 任务倒计时
            MissionTimer = new CustomTimer(remainTime);
            #endregion

            // 设置任务玩家组别及关系
            Game.PlayerPed.SetAsFighter();

            // 自定义玩家武器
            Game.PlayerPed.Weapons.RemoveAll();
            MissionInfo.PlayerWeapons.ForEach(
                w => Game.PlayerPed.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));

            // 绑定事件
            FatalDamageEvents.OnDeath += OnDeath;
            FatalDamageEvents.OnPlayerKillPed += OnPlayerKillPed;

            // 界面/声音提醒
            var info = MissionInfo.StartNotificationInfo;
            Notify.CustomImage("CHAR_CALL911", "CHAR_CALL911", info.Message, info.Sender, info.Subject, true, 2);
            AudioPlayer.Play(AudioName.Beep);
            Subtitle.Draw(MissionInfo.HintSubtitle, (int)remainTime);

            // 通知服务器
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:ClientJoinMission");

            IsJoined = true;
        }

        /// <summary>
        /// 保持加入状态
        /// </summary>
        private void RetainJoinState()
        {
            var missionPeds = MissionPedController.GetAllMissionPeds();

            #region 显示范围Blip
            if (RadiusBlip is null)
                RadiusBlip = World.CreateBlip(MissionInfo.RangeInfo.Position, MissionInfo.RangeInfo.Radius);
            RadiusBlip.Alpha = 64;
            RadiusBlip.Color = BlipColor.Red;
            RadiusBlip.ShowRoute = true;
            #endregion

            // 全地图显示敌人位置
            missionPeds
                .Where(ped => ped.IsAlive)
                .ToList().ForEach(ped =>
            {
                var blip = ped.AttachedBlip;
                if (blip is null)
                    blip = ped.AttachBlip();
                blip.Color = BlipColor.Red;
                blip.IsShortRange = false;
                blip.Alpha = 256;
            });

            // 如果仍有敌人存活, 则保持通缉状态
            if (missionPeds.Any(ped => ped.IsAlive))
            {
                API.SetPlayerWantedLevel(Game.Player.Handle, 5, false);
                API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
            }

            // 设置当前刷新的警察为同伴
            if (SetCopAllianceTimer.IsMeet())
            {
                World.GetAllPeds()
                    .Where(ped => !ped.IsPlayer && ped.IsCop())
                    .ToList().ForEach(ped => ped.SetAsAlliance());
            }
        }

        /// <summary>
        /// 玩家离开任务
        /// </summary>
        public void Leave(string reason)
        {
            // 调用玩家离开事件
            OnPlayerLeaveMission?.Invoke(reason);

            // (重置)设置任务玩家组别及关系
            Game.PlayerPed.SetRelationshipGroup(RelationshipBaseGroup.PLAYER);

            // 解除通缉状态
            API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
            API.SetPlayerWantedLevelNow(Game.Player.Handle, false);

            // 通知服务器
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:ClientLeaveMission", reason);

            // 解除事件绑定
            FatalDamageEvents.OnDeath -= OnDeath;
            FatalDamageEvents.OnPlayerKillPed -= OnPlayerKillPed;

            IsJoined = false;
        }

        private void OnDeath(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            // 如果玩家死亡, 则视为离开任务
            Leave("dead");
        }

        private void OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            var missionPeds = MissionPedController.GetAllMissionPeds();

            // 任务目标死亡, 隐藏标记红点, 并标记为可回收资源
            missionPeds
                .Where(ped => ped == victim)
                .ToList().ForEach(ped => 
            {
                // 隐藏标记红点
                var blip = ped.AttachedBlip;
                if (blip is null)
                    blip = ped.AttachBlip();
                blip.Color = BlipColor.Red;
                blip.IsShortRange = true;
                blip.Alpha = 0;

                ped.IsPersistent = false;
                ped.MarkAsNoLongerNeeded();
            });
            // 所有任务目标死亡, 则视为任务完成
            if (missionPeds.All(ped => ped.IsDead))
            {
                Leave("finish");
                Deactivate();
            }
        }

        /// <summary>
        /// 作用于每帧的异步方法, 一般用于GUI显示
        /// </summary>
        /// <returns></returns>
        public async Task EveryFrameTick()
        {
            if (IsJoined)
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
        /// 时间间隔较为宽松的检测的异步方法
        /// </summary>
        /// <returns></returns>
        public async Task TolerantTick()
        {
            var missionPeds = MissionPedController.GetAllMissionPeds();
            // 已经加入
            if (IsJoined)
            {
                RetainJoinState();
            }
            else if (IsActivated)
            {
                RetainActivateState();
            }
            else
            {
                RevertBaseState();
            }

            await BaseScript.Delay(1000);
        }
    }

    public class CopClearGangzone : BaseScript
    {

        public const string ResourceName = "CopClearGangzone";

        public static MissionsInfo MissionsInfo;
        public static MissionInstance MissionInstance;
        private HostSelector HostSelector;
        private MissionRemainTimeAsyncer MissionRemainTimeAsyncer = MissionRemainTimeAsyncer.Instance;
        private MissionPedController MissionPedController;

        /// <summary>
        /// Is mission instance exist
        /// </summary>
        /// <returns></returns>
        private static bool IsMissionInstraceExists
        { get => MissionInstance?.Exists() ?? false; }

        /// <summary>
        /// 加入当前任务
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> JoinCurrentMission()
        {
            if (IsMissionInstraceExists)
            {
                if (!MissionInstance.IsJoined)
                {
                    await MissionInstance.Join();
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
                Notify.Info("没有可疑的区域暴乱活动");
                return false;
            }
                
        }

        /// <summary>
        /// Alias for JoinCurrentMission
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> TryJoinCurrentMission()
            => await JoinCurrentMission();

        /// <summary>
        /// 离开任务
        /// </summary>
        /// <param name="reason"></param>
        private async Task LeaveCurrentMission(string reason)
        {
            if (IsMissionInstraceExists && MissionInstance.IsJoined)
            {
                MissionInstance.Leave(reason);
                HintOnLeaveMission(reason);
                await Task.FromResult(0);
            }
        }

        /// <summary>
        /// 离开任务(无提示)
        /// </summary>
        /// <param name="reason"></param>
        private void LeaveCurrentMissionWithoutHint(string reason)
        {
            if (IsMissionInstraceExists && MissionInstance.IsJoined)
            {
                MissionInstance.Leave(reason);
            }
        }

        /// <summary>
        /// 任务结束提示
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async void HintOnLeaveMission(string reason)
        {
            switch (reason)
            {
                case "finish":
                    AudioPlayer.Play(AudioName.MissionComplete);
                    await ScaleformDrawer.DrawCenterBar(API.GetLabelText("BM_PASS"), "~y~评分: 100~s~");
                    Subtitle.Draw("", 1000);
                    break;
                case "dead":
                case "timeup":
                case "delete":
                default:
                    AudioPlayer.Play(AudioName.Wasted);
                    await ScaleformDrawer.DrawCenterBar(API.GetLabelText("REPLAY_T"), "~y~评分: 0~s~");
                    Subtitle.Draw("", 1000);
                    break;
            }

            // 取消通缉
            await Delay(1000 * 2);
            API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
            API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
        }

        /// <summary>
        /// 创建并激活任务
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        private void CreateAndActivateMission(int missionInfoIndex)
        {
            CreateMission(missionInfoIndex);
            ActivateCurrentMission();
        }

        
        private async Task LeaveAndDeactivateCurrentMission(string reason)
        {
            await LeaveCurrentMission(reason);
            DeactivateCurrentMission();
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        private void CreateMission(int missionInfoIndex)
        {
            MissionInstance = new MissionInstance(MissionsInfo[missionInfoIndex]);
        }

        /// <summary>
        /// 激活当前任务
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        public void ActivateCurrentMission()
        {
            if (IsMissionInstraceExists && !MissionInstance.IsActivated)
            {
                Tick += MissionInstance.EveryFrameTick;
                Tick += MissionInstance.TolerantTick;

                MissionInstance.OnPlayerLeaveMission += HintOnLeaveMission;

                MissionInstance.Activate();
            }
        }

        /// <summary>
        /// 解除激活当前任务
        /// </summary>
        private void DeactivateCurrentMission()
        {
            if (IsMissionInstraceExists && MissionInstance.IsActivated)
            {
                Tick -= MissionInstance.EveryFrameTick;
                Tick -= MissionInstance.TolerantTick;

                MissionInstance.OnPlayerLeaveMission -= HintOnLeaveMission;

                MissionInstance.Deactivate();
            }
            
        }

        /// <summary>
        /// 作为主机, 尝试创建任务
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        /// <returns></returns>
        private async Task<bool> TryCreateMissionAsHost(int missionInfoIndex)
        {
            Debug.WriteLine($"TryCreateMissionAsHost {missionInfoIndex}");
            var TryAt = API.GetGameTimer();

            var created = await CreateMissionAsHost(missionInfoIndex);
            
            if (created)
            {
                TriggerServerEvent($"{ResourceName}:RespondCreateMissionAsHost", API.GetGameTimer() - TryAt);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        /// <returns></returns>
        private async Task<bool> CreateMissionAsHost(int missionInfoIndex)
        {
            Debug.WriteLine($"CreateMission {missionInfoIndex}");
            
            var missionInfo = MissionsInfo[missionInfoIndex];

            await DestroyMission("destroy");

            // 创建任务人物
            foreach (var item in missionInfo.PedsInfo.Select((pi, i) => new { pi, i }))
            {

                var missionPedInfo = item.pi;
                var missionPedInfoIndex = item.i;

                var ped = await MissionPedController.Create(missionInfoIndex, missionPedInfoIndex);
                // 如果创建失败, 则尝试释放资源并返回创建失败
                if (ped is null)
                {
                    MissionInstance = null;
                    Debug.WriteLine($"[ERROR]Failed at creating ped {missionPedInfo} {missionPedInfoIndex}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 销毁任务
        /// </summary>
        private async Task DestroyMission(string reason)
        {
            if (IsMissionInstraceExists)
            {
                LeaveCurrentMissionWithoutHint(reason);
                DeactivateCurrentMission();

                MissionInstance.Delete();
                MissionInstance = null;

                await Task.FromResult(0);
            }
        }

        private static MissionMenu.MainMenu MainMenu { get; set; }
        private async Task ToggleMenuCheckAsync()
        {
            if (Game.PlayerPed.IsInPoliceVehicle && API.IsControlPressed(0, (int)Control.Context))
            {
                MainMenu?.GetMenu()?.OpenMenu();
            }
            await Delay(1000);
        }
        
        private async Task LoadAll(string missionsInfoJson)
        {
            // Debug.WriteLine($"{missionsInfoJson}");
            MissionsInfo = new MissionsInfo(MissionsInfo.Deserialize(missionsInfoJson));
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
            MissionsInfo.GetPedsHash().ForEach(async pedHash =>
            {
                var loaded = await new Model(pedHash).Request(1000 * 60);
                Debug.WriteLine($"[{ResourceName}]Ped model {pedHash} load {(loaded ? "succeed" : "failed")}.");
            });
            
            // Unsubscribe
            Tick -= LoadModel;

        }

        /// <summary>
        /// 广播任务情况
        /// </summary>
        /// <param name="missionInfoIndex"></param>
        /// <returns></returns>
        private async Task BroadcastMission(int missionInfoIndex)
        {
            if (!IsMissionInstraceExists)
                return;

            // 不对任务进行中的玩家进行广播
            if (MissionInstance.IsJoined)
                return;

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
            
            EventHandlers.Add($"{ResourceName}:LoadAll", new Func<string, Task>(LoadAll));
            EventHandlers.Add($"{ResourceName}:TryCreateMissionAsHost", new Func<int, Task<bool>>(TryCreateMissionAsHost));
            EventHandlers.Add($"{ResourceName}:CreateAndActivateMission", new Action<int>(CreateAndActivateMission));
            EventHandlers.Add($"{ResourceName}:LeaveAndDeactivateCurrentMission", new Func<string, Task>(LeaveAndDeactivateCurrentMission));
            EventHandlers.Add($"{ResourceName}:DestroyMission", new Func<string, Task>(DestroyMission));
            EventHandlers.Add($"{ResourceName}:BroadcastMission", new Func<int, Task>(BroadcastMission));
            
            // 从服务器加载任务信息
            TriggerServerEvent($"{ResourceName}:LoadAll");

            Tick += ToggleMenuCheckAsync;
            Tick += LoadModel;

            MainMenu = new MissionMenu.MainMenu();
            MainMenu.CreateMenu();
            
            EventHandlers.Add(MissionRemainTimeAsyncer.EventName, MissionRemainTimeAsyncer.EventDelegate);
            
            HostSelector = new HostSelector();
            EventHandlers.Add(HostSelector.EventName, HostSelector.EventDelegate);

            MissionRelationship.Init();

            MissionPedController = new MissionPedController();
            Tick += MissionPedController.TolerantTick;
            
            API.RegisterCommand("netpedtest", new Action(() =>
            {
                List<dynamic> l = new List<dynamic>();
                foreach (var ped in World.GetAllPeds())
                {
                    var netHandle = API.PedToNet(ped.Handle);
                    var pedHandle = API.NetToEnt(netHandle);
                    Debug.WriteLine(
                        $"handle(raw): {ped.Handle}, handle(fromnet): {pedHandle}, net: {netHandle}, " +
                        $"distance: {Math.Sqrt(ped.Position.DistanceToSquared(Game.PlayerPed.Position))}, " +
                        $"model: {ped.Model.Hash}, " +
                        $"network: {API.NetworkDoesEntityExistWithNetworkId(ped.Handle)}, {API.NetworkDoesNetworkIdExist(netHandle)}, {API.NetworkGetEntityFromNetworkId(netHandle)}");
                    l.Add(new { Handle = pedHandle, NetworkId = netHandle, rawHandle = ped.Handle });
                }
                //TriggerServerEvent($"{ResourceName}:RecordCreatedPed", l);
            }), false);

            API.RegisterCommand("wudi", new Action(() =>
            {
                Game.PlayerPed.IsInvincible = !Game.PlayerPed.IsInvincible;
                Debug.WriteLine($"wudi: {Game.PlayerPed.IsInvincible}");
            }), false);

        }

    }

}
