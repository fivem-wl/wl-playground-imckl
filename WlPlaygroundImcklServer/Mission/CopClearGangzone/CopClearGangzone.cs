using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using WlPlaygroundImcklShared.Mission.CopClearGangzone;


namespace WlPlaygroundImcklServer.Mission.CopClearGangzone
{

    public sealed class CopClearGangzone : BaseScript
    {

        public const string ResourceName = "CopClearGangzone";
        public static MissionsInfo MissionsInfo;

        private HostSelector HostSelector;
        private MissionCreator MissionCreator;

        /// <summary>
        /// 是否有玩家在线
        /// </summary>
        public bool HasAnyPlayer { get => Players.Count() >= 1; }

        public CopClearGangzone()
        {
            // 读取任务信息
            MissionsInfo = Storage.GetAll();

            HostSelector = new HostSelector();
            EventHandlers.Add(HostSelector.EventName, HostSelector.EventDelegate);

            MissionCreator = new MissionCreator();
            EventHandlers.Add(MissionCreator.EventName, MissionCreator.EventDelegate);

            EventHandlers.Add($"{ResourceName}:LoadAll", new Action<Player>(LoadAll));
            //EventHandlers.Add($"{ResourceName}:CreateMission", new Action(CreateMission));
            EventHandlers.Add($"{ResourceName}:ClientJoinMission", new Action<Player>(ClientJoinMission));
            EventHandlers.Add($"{ResourceName}:ClientLeaveMission", new Action<Player, string>(ClientLeaveMission));
            EventHandlers.Add($"{ResourceName}:ClientGetMissionRemainTime", new Action<Player>(ClientGetMissionRemainTime));

            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            // 任务计划
            Tick += ScheduleMissionAsync;

        }

        private void OnPlayerDropped([FromSource] Player source, string reason)
        {
            // 如果没有玩家在线, 则标记为任务未在运行(当前玩家离线前仍在Player列表中)
            if (Players.Count() <= 1)
            {
                Debug.WriteLine("no one online, set mission as not running");
                IsMissionRunning = false;
            }
        }



        //public bool IsMissionScheduled { get; private set; } = false;
        public bool IsMissionRunning { get; private set; } = false;
        public int CurrentMissionIndex { get; private set; } = 0;

        public int ScheduleMissionAsyncInterval { get; set; } = 1000 * 1;

        /// <summary>
        /// 任务计划计时
        /// </summary>
        public CustomTimer MissionScheduleTimer { get; set; } = new CustomTimer(1000 * 60);
        /// <summary>
        /// 广播计时
        /// </summary>
        public CustomTimer MissionBroadcastTimer { get; set; } = new CustomTimer(1000 * 60);
        /// <summary>
        /// 任务计时
        /// </summary>
        public CustomTimer MissionRunningTimer { get; set; }
        /// <summary>
        /// 上一次任务结束时间
        /// </summary>
        public long LastMissionEndTime { get; set; } = 0;
        /// <summary>
        /// 任务最小时间间隔
        /// </summary>
        public long MissionRescheduleWaitTime { get; set; } = 1000 * 60 * 1;
        /// <summary>
        /// 任务状态检测(异步)
        /// </summary>
        /// <returns></returns>
        private async Task ScheduleMissionAsync()
        {
            await Delay(ScheduleMissionAsyncInterval);
            // 已到达任务时限
            if (MissionRunningTimer?.IsMeet() ?? false)
            {
                // 中止当前任务, 并计划新任务
                if (HasAnyPlayer)
                {
                    // 结束任务
                    StopMission("timeup");
                }
                // 重置任务时限计时
                MissionRunningTimer = null;
                // 设置上一次任务结束时间
                LastMissionEndTime = API.GetGameTimer();
            }
            // 已到达计划下一个任务的时间
            if (MissionScheduleTimer.IsMeet())
            {
                // 已经超过任务最小间隔
                if (API.GetGameTimer() - LastMissionEndTime > MissionRescheduleWaitTime)
                {
                    // 任务不在进行中
                    if (!IsMissionRunning)
                    {
                        // 有任意玩家
                        if (HasAnyPlayer)
                        {
                            // 尝试创建任务
                            await CreateMission(CurrentMissionIndex);

                        }
                    }
                }
                
            }
            // 已到达提醒任务的事件
            if (MissionBroadcastTimer.IsMeet())
            {
                // 任务进行中
                if (IsMissionRunning)
                {
                    // 有任意玩家
                    if (HasAnyPlayer)
                    {
                        // 则广播任务计划
                        BroadcastMission(CurrentMissionIndex);
                    }
                }
            }
            
        }

        /// <summary>
        /// 客户端加入任务
        /// </summary>
        private void ClientJoinMission([FromSource]Player source)
        {
            Debug.WriteLine($"[{ResourceName}]Mission joined at client: {source.Identifiers["license"]}");
        }

        /// <summary>
        /// 客户端离开任务
        /// </summary>
        /// <param name="source"></param>
        /// <param name="reason"></param>
        private void ClientLeaveMission([FromSource]Player source, string reason)
        {
            Debug.WriteLine($"[{ResourceName}]Mission leaved at client: {source.Identifiers["license"]}, reason: {reason}");
        }

        /// <summary>
        /// 客户端获取任务剩余时间
        /// </summary>
        /// <param name="source"></param>
        private void ClientGetMissionRemainTime([FromSource]Player source)
        {
            TriggerClientEvent(source, $"{ResourceName}:ClientGetMissionRemainTime", MissionRunningTimer?.RemainTime ?? 0);
        }

        /// <summary>
        /// 任务完成 - 只要有任一玩家完成任务, 则视为所有玩家完成任务
        /// ?可能存在资源竞争问题?
        /// </summary>
        private void StopMission(string reason)
        {
            IsMissionRunning = false;
            if (reason == "timeup")
            {
                TriggerClientEvent($"{ResourceName}:LeaveCurrentMission", reason);
            }
            else
            {
                TriggerClientEvent($"{ResourceName}:DestroyMission", reason);
            }
        }

        /// <summary>
        /// 尝试在客户端创建任务, 并开始服务器的任务倒计时
        /// </summary>
        /// <param name="currentMissionIndex"></param>
        private async Task<bool> CreateMission(int missionInfoIndex)
        {

            Debug.WriteLine($"[DEBUG][{ResourceName}]Start to create mission.");
            // 尝试获取合适的客户端用于Host
            var turstedClient = await HostSelector.TryGetByCreatingDummyPedsAsync(MissionsInfo.GetPedsHash(missionInfoIndex));
            if (turstedClient is null)
            {
                Debug.WriteLine($"[WARNING][{ResourceName}]Failed to get trusted client, retry creating after next interval.");
                return false;
            }

            // 尝试用指定Client创建任务
            var succeed = await MissionCreator.TryCreateByClient(turstedClient, missionInfoIndex);
            if (!succeed)
            {
                Debug.WriteLine($"[WARNING][{ResourceName}]Failed to create mission by trusted client, retry creating after next interval.");
                return false;
            }

            Debug.WriteLine($"[INFO][{ResourceName}]Create mission succeed.");

            IsMissionRunning = true;

            MissionRunningTimer = new CustomTimer(MissionsInfo[missionInfoIndex].Duration);

            TriggerClientEvent($"{ResourceName}:CreateAndActivateMission", missionInfoIndex);

            return true;
        }

        /// <summary>
        /// 在所有客户端广播任务信息
        /// </summary>
        /// <param name="currentMissionIndex"></param>
        private void BroadcastMission(int currentMissionIndex)
        {
            TriggerClientEvent($"{ResourceName}:BroadcastMission", CurrentMissionIndex);
        }

        private void LoadAll([FromSource] Player source)
        {
            TriggerClientEvent(source, $"{ResourceName}:LoadAll", MissionsInfo.Serialize());
        }

    }

    /// <summary>
    /// 任务创建器
    /// </summary>
    public class MissionCreator
    {

        private Player RespondFrom;
        private long RespondAt;
        private long WaitAt;
        private bool IsWaiting;

        public string EventName;
        public Action<Player, int> EventDelegate;

        /// <summary>
        /// 初始化
        /// </summary>
        public MissionCreator()
        {
            EventName = $"{CopClearGangzone.ResourceName}:RespondCreateMissionAsHost";
            EventDelegate = new Action<Player, int>(RespondCreateMissionAsHost);
        }

        /// <summary>
        /// 尝试创建任务
        /// </summary>
        /// <param name="trustedClient"></param>
        /// <param name="missionInfoIndex"></param>
        /// <returns></returns>
        internal async Task<bool> TryCreateByClient(Player trustedClient, int missionInfoIndex)
        {

            var clientLicense = trustedClient.Identifiers["license"];
            var clientEndPoint = trustedClient.EndPoint;

            BaseScript.TriggerClientEvent(trustedClient, $"{CopClearGangzone.ResourceName}:TryCreateMissionAsHost", missionInfoIndex);
            try
            {
                await WaitForAnyResponse();
            }
            catch (TimeoutException e)
            {
                Debug.WriteLine($"[ERROR][{CopClearGangzone.ResourceName}]No respond from {clientLicense}({clientEndPoint}). Reason: {e.Message}");
                return false;
            }

            Debug.WriteLine($"[DEBUG][{CopClearGangzone.ResourceName}]Response from {clientLicense}({clientEndPoint}), created succeed");

            return true;

        }

        /// <summary>
        /// 接收客户端响应结果
        /// </summary>
        /// <param name="source"></param>
        /// <param name="clientElapsedTime"></param>
        private void RespondCreateMissionAsHost([FromSource]Player source, int clientElapsedTime)
        {
            if (IsWaiting)
            {
                RespondFrom = source;
                RespondAt = API.GetGameTimer();
                IsWaiting = false;
            }
            Debug.WriteLine($"[DEBUG][{CopClearGangzone.ResourceName}]" +
                $"Recieved mission creation succeed response from {source.Identifiers["license"]} at endpoint {source.EndPoint}, " +
                $"client elapsed time: {clientElapsedTime}");
        }

        /// <summary>
        /// 等待任意客户端响应
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<bool> WaitForAnyResponse(int timeout = 1000 * 30)
        {
            WaitAt = API.GetGameTimer();
            IsWaiting = true;
            while (IsWaiting)
            {
                if (API.GetGameTimer() - WaitAt > timeout)
                {
                    IsWaiting = false;
                    throw (new TimeoutException("Wait timeout."));
                }
                await BaseScript.Delay(100);
            }
            IsWaiting = false;

            return false;
        }

    }


}
