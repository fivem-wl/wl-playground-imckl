using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    /// <summary>
    /// 从服务器获取任务剩余时间
    /// </summary>
    public class MissionRemainTimeAsyncer
    {
        private bool ServerResponsed { get; set; }
        private long RemainTime { get; set; }

        public string EventName;
        public Delegate EventDelegate;

        // Singleton
        private static readonly Lazy<MissionRemainTimeAsyncer>
            lazy = new Lazy<MissionRemainTimeAsyncer>(() => new MissionRemainTimeAsyncer());
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static MissionRemainTimeAsyncer Instance { get { return lazy.Value; } }
        private MissionRemainTimeAsyncer()
        {
            ServerResponsed = false;
            RemainTime = 0;

            EventName = $"{CopClearGangzone.ResourceName}:ClientGetMissionRemainTime";
            EventDelegate = new Action<long>(Set);
        }

        /// <summary>
        /// 从服务器返回任务剩余时间.
        /// 超时则抛出TimeoutException异常.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<long> GetFromServer(int timeout = 1000 * 15)
        {
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:ClientGetMissionRemainTime");

            var now = API.GetGameTimer();

            while (!ServerResponsed)
            {
                if (API.GetGameTimer() - now >= timeout)
                {
                    throw (new TimeoutException("Failed to recieve value from server (reason: timeout)."));
                }
                await BaseScript.Delay(100);
            }

            ServerResponsed = false;

            return RemainTime;
        }

        /// <summary>
        /// 设置任务剩余时间, 用于接收服务器返回结果
        /// </summary>
        /// <param name="remainTime"></param>
        private void Set(long remainTime)
        {
            RemainTime = remainTime;
            ServerResponsed = true;
        }
    }

}
