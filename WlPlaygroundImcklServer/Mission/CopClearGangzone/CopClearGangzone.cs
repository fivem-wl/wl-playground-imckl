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

        public static string ResourceName = "CopClearGangzone";
        private List<MissionInfo> MissionsInfo;

        /// <summary>
        /// 是否有玩家在线
        /// </summary>
        public bool HasAnyPlayer { get => Players.Count() >= 1; }

        public CopClearGangzone()
        {
            EventHandlers.Add($"{ResourceName}:LoadAll", new Action<Player>(LoadAll));
            //EventHandlers.Add($"{ResourceName}:CreateMission", new Action(CreateMission));
            EventHandlers.Add($"{ResourceName}:ClientActivateMission", new Action<Player>(ClientActivateMission));
            EventHandlers.Add($"{ResourceName}:ClientGetMissionRemainTime", new Action<Player>(ClientGetMissionRemainTime));
            EventHandlers.Add($"{ResourceName}:ClientStopMission", new Action<Player, string>(ClientStopMission));
            //EventHandlers.Add($"{ResourceName}:RecordCreatedPed", new Action<Player, List<dynamic>>(RecordCreatedPed));

            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            // 读取任务信息
            MissionsInfo = Storage.GetAll();

            // 任务计划
            Tick += ScheduleMissionAsync;

            API.RegisterCommand("pedtest", new Action<int, List<object>, string>((source, args, raw) =>
            {
                Debug.WriteLine($"Ped test");
                foreach (var (Handle, NetworkId) in TestSample)
                {
                    Debug.WriteLine($"{Handle} {NetworkId}");
                    Debug.WriteLine($"NetworkGetEntityFromNetworkId: {API.NetworkGetEntityFromNetworkId(NetworkId)}");
                    Debug.WriteLine($"NetworkGetNetworkIdFromEntity: {API.NetworkGetNetworkIdFromEntity(API.NetworkGetEntityFromNetworkId(NetworkId))}");
                    Debug.WriteLine($"NetworkGetEntityOwner: {API.NetworkGetEntityOwner(API.NetworkGetEntityFromNetworkId(NetworkId))}");
                }
            }), false);
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
        public long MissionRescheduleWaitTime { get; set; } = 1000 * 60 * 3;
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
                            CreateMission(CurrentMissionIndex);
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
        /// 客户端激活任务
        /// </summary>
        private void ClientActivateMission([FromSource]Player source)
        {
            Debug.WriteLine($"[{ResourceName}]Mission activated at client: {source.Identifiers["license"]}");
        }

        /// <summary>
        /// 客户端完成任务
        /// </summary>
        /// <param name="source"></param>
        /// <param name="reason"></param>
        private void ClientStopMission([FromSource]Player source, string reason)
        {
            Debug.WriteLine($"[{ResourceName}]Mission stopped at client: {source.Identifiers["license"]}, reason: {reason}");
            StopMission(reason);
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
            TriggerClientEvent($"{ResourceName}:StopMission", reason);
        }

        /// <summary>
        /// 在客户端创建任务, 并开始服务器的任务倒计时
        /// </summary>
        /// <param name="currentMissionIndex"></param>
        private void CreateMission(int currentMissionIndex)
        {
            IsMissionRunning = true;

            MissionRunningTimer = new CustomTimer(MissionsInfo[currentMissionIndex].Duration);

            TriggerClientEvent($"{ResourceName}:CreateMission", currentMissionIndex);
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
            TriggerClientEvent(source, $"{ResourceName}:LoadAll", JsonConvert.SerializeObject(MissionsInfo));
        }

        List<(int Handle, int NetworkId)> TestSample = new List<(int Handle, int NetworkId)>();

        //private void RecordCreatedPed([FromSource] Player source, List<dynamic> list)
        //{
        //    Debug.WriteLine($"uploader: {source.Identifiers["license"]}");
        //    foreach(var item in list)
        //    {
        //        Debug.WriteLine($"{item.Handle} {item.NetworkId}");
        //        Debug.WriteLine($"NetworkGetEntityFromNetworkId: {API.NetworkGetEntityFromNetworkId(item.NetworkId)}");
        //        Debug.WriteLine($"NetworkGetNetworkIdFromEntity: {API.NetworkGetNetworkIdFromEntity(API.NetworkGetEntityFromNetworkId(item.NetworkId))}");
        //        Debug.WriteLine($"NetworkGetEntityOwner: {API.NetworkGetEntityOwner(API.NetworkGetEntityFromNetworkId(item.NetworkId))}");
        //        TestSample.Add((item.Handle, item.NetworkId));
        //    }
        //}



    }
}
