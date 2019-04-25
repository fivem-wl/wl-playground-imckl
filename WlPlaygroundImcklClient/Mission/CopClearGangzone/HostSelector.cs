using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using WlPlaygroundImcklClient.Mission.CopClearGangzone;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    /// <summary>
    /// Host选择器, 用于筛选合适的客户端作为Host(例如用于创建任务)
    /// </summary>
    internal class HostSelector
    {
        public string EventName;
        public Delegate EventDelegate;

        /// <summary>
        /// 初始化
        /// </summary>
        public HostSelector()
        {
            EventName = $"{CopClearGangzone.ResourceName}:TryCreateDummyPeds";
            EventDelegate = new Func<string, Task<bool>>(TryCreateDummyPeds);
        }

        /// <summary>
        /// 尝试创建并删除人物模型, 并将创建所需时间返回服务器
        /// </summary>
        /// <param name="pedsHash"></param>
        /// <returns></returns>
        private async Task<bool> TryCreateDummyPeds(string pedsHashJson)
        {
            
            var pedsHash = JsonConvert.DeserializeObject<List<PedHash>>(pedsHashJson);

            List<Ped> dummyPeds = new List<Ped>();
            var tryAt = API.GetGameTimer();
            // try create dummy peds
            foreach (var pedHash in pedsHash)
            {
                // try request model, not loaded = false
                var model = new Model(pedHash);
                await model.Request(1000 * 30);
                if (!model.IsLoaded)
                    return false;
                // create ped
                var pedHandle = API.CreatePed(26, (uint)pedHash, 0, 0, 0, 0, false, false);
                var ped = new Ped(pedHandle);
                // set ped persistent
                ped.IsPersistent = true;
                // push into dummy list
                dummyPeds.Add(ped);
            }
            // delete dummpy peds; if not exist, return false
            foreach (var ped in dummyPeds)
            {
                if (ped.Exists())
                    ped.Delete();
                else
                    return false;
            }
            var elapsedTime = API.GetGameTimer() - tryAt;
            // finish process, response to server
            BaseScript.TriggerServerEvent($"{CopClearGangzone.ResourceName}:RespondCreateDummyPed", elapsedTime);
            return true;
        }
    }
}
