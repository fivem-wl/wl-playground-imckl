using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    public static class MissionRemainTimeAsyncer
    {
        private static bool ServerResponsed { get; set; } = false;
        private static long RemainTime { get; set; } = 0;

        /// <summary>
        /// 从服务器返回任务剩余时间.
        /// 超时则抛出TimeoutException异常.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<long> GetFromServer(int timeout = 1000 * 15)
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
        /// 设置任务剩余时间, 请用于接收服务器返回结果
        /// </summary>
        /// <param name="remainTime"></param>
        public static void Set(long remainTime)
        {
            RemainTime = remainTime;
            ServerResponsed = true;
        }
    }

}
