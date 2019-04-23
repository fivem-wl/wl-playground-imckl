using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{

    public enum ScaleformName
    {
        CenterBar = 1,
    }

    public static class ScaleformDrawer
    {

        private static async Task Delay(int msecs) => await BaseScript.Delay(msecs);

        private static async Task Draw()
        {
            await Delay(0);
        }

        public static async Task DrawCenterBar(string message, string subMessage, int colId = 5, int duration = 1000 * 10)
        {
            var scaleform = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
            while (!scaleform.IsLoaded)
            {
                await Delay(100);
            }
            var now = API.GetGameTimer();
            scaleform.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", message, subMessage, colId);
            while (API.GetGameTimer() - now <= duration)
            {
                scaleform.Render2D();
                await Delay(0);
            }
            scaleform.Dispose();
        }

        private static void _registerTestCommand()
        {
            API.RegisterCommand("testshows", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                var now = Game.GameTime;
                Scaleform scaleform = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
                while (!scaleform.IsLoaded)
                {
                    await Delay(100);
                }

                var labelText = "";
                if (!(args.Count == 0 || string.IsNullOrEmpty(args[0].ToString())))
                {
                    labelText = args[0].ToString();
                }

                scaleform.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", API.GetLabelText(labelText), "~y~DONE~s~", 5);
                while (Game.GameTime - now <= 1000 * 8)
                {
                    scaleform.Render2D();
                    await Delay(0);
                }
            }), false);
        }

    }
}
