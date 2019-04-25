using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionGUI
{
    public static class Subtitle
    {
        public static void Draw(string message, int duration)
        {
            API.SetTextEntry_2("STRING");
            API.AddTextComponentString(message);
            API.DrawSubtitleTimed(duration, true);
        }
    }
}
