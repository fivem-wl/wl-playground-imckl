using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklShared.Mission.CopClearGangzone
{

    /// <summary>
    /// 自定义Timer
    /// </summary>
    public class CustomTimer
    {
        /// <summary>
        /// 持续时间
        /// </summary>
        public long Duration { get; private set; }
        /// <summary>
        /// 最近检测时间
        /// </summary>
        public long LastCheckTime { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public long StartTime { get; private set; }
        /// <summary>
        /// 剩余时间
        /// </summary>
        public long RemainTime { get => Duration - (API.GetGameTimer() - StartTime); }

        /// <summary>
        /// 是否到达或超过持续时间
        /// </summary>
        /// <param name="updateCheckTime">是否自动更新最近检测时间 - 如果为否, 需要手动设置LastCheckTime值; 否则会一直返回True</param>
        /// <returns></returns>
        public bool IsMeet(bool updateCheckTime = true)
        {
            var now = API.GetGameTimer();
            if (now - LastCheckTime >= Duration)
            {
                if (updateCheckTime)
                {
                    LastCheckTime = now;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Consturctor
        /// </summary>
        /// <param name="duration">持续时间</param>
        public CustomTimer(long duration)
        {
            var now = API.GetGameTimer();
            Duration = duration;
            LastCheckTime = now;
            StartTime = now;
        }

    }

}
