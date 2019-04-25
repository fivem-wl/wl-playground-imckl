using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionPed
{
    static class PedExtension
    {
        /// <summary>
        /// Is Ped a cop, SWAT or SWAT
        /// </summary>
        /// <returns></returns>
        public static bool IsCop(this Ped ped)
        {
            var pedType = API.GetPedType(ped.Handle);
            return (pedType == (int)PedType.Cop || pedType == (int)PedType.Army || pedType == (int)PedType.SWAT);
        }
    }
}
