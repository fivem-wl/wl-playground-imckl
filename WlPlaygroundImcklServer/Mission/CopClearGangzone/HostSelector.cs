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
    /// <summary>
    /// Used to find proper client to do the mission creation job.
    /// 委托给客户端进行Ped创建事件:
    /// 1. 在所有客户端创建不同步至网络的dummy ped
    /// 2. 移除所有dummy ped
    /// 3. 最先返回到服务器的即视为受信任的创建主
    /// </summary>
    internal class HostSelector
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
        public HostSelector()
        {
            EventName = $"{CopClearGangzone.ResourceName}:RespondCreateDummyPed";
            EventDelegate = new Action<Player, int>(RespondCreateDummyPed);
        }

        /// <summary>
        /// Try to get proper client by creating dummy peds test
        /// </summary>
        /// <param name="pedsHash"></param>
        /// <returns></returns>
        internal async Task<Player> TryGetByCreatingDummyPedsAsync(List<PedHash> pedsHash)
        {

            var pedsHashJson = JsonConvert.SerializeObject(pedsHash);

            BaseScript.TriggerClientEvent($"{CopClearGangzone.ResourceName}:TryCreateDummyPeds", pedsHashJson);
            try
            {
                await WaitForAnyResponse();
            }
            catch (TimeoutException e)
            {
                Debug.WriteLine($"[ERROR][{CopClearGangzone.ResourceName}]No client respond. Reason: {e.Message}");
                return null;
            }

            Debug.WriteLine($"[DEBUG][{CopClearGangzone.ResourceName}]" +
                $"Pick {RespondFrom.Identifiers["license"]}({RespondFrom.EndPoint}) as trusted client for creation.");

            return RespondFrom;

        }

        /// <summary>
        /// 接收客户端响应结果
        /// </summary>
        /// <param name="source"></param>
        /// <param name="clientElapsedTime"></param>
        private void RespondCreateDummyPed([FromSource]Player source, int clientElapsedTime)
        {
            if (IsWaiting)
            {
                RespondFrom = source;
                RespondAt = API.GetGameTimer();
                IsWaiting = false;
            }
            Debug.WriteLine($"[DEBUG][{CopClearGangzone.ResourceName}]" +
                $"Recieved dummy ped creation test response from {source.Identifiers["license"]} at endpoint {source.EndPoint}, " +
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
