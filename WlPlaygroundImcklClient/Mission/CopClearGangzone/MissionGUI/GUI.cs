using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.MissionGUI
{
    class GUI
    {

        public static class SafeZone
        {
            public static float Size { get { return API.GetSafeZoneSize(); } }
            public static float Left { get { return (1.0f - Size) * 0.5f; } }
            public static float Right { get { return 1.0f - Left; } }
            public static float Top { get { return Left; } }
            public static float Bottom { get { return Right; } }
        }

        /// <summary>
        /// Draw text on the screen at the provided x and y locations.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="xPosition">The x position for the text draw origin.</param>
        /// <param name="yPosition">The y position for the text draw origin.</param>
        /// <param name="size">The size of the text.</param>
        /// <param name="justification">Align the text. 0: center, 1: left, 2: right</param>
        /// <param name="font">Specify the font to use (0-8).</param>
        /// <param name="disableTextOutline">Disables the default text outline.</param>
        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (API.IsHudPreferenceSwitchedOn() && !API.IsPlayerSwitchInProgress() && API.IsScreenFadedIn() && !API.IsPauseMenuActive() && !API.IsFrontendFading() && !API.IsPauseMenuRestarting() && !API.IsHudHidden())
            {
                API.SetTextFont(font);
                API.SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    API.SetTextWrap(0f, xPosition);
                }
                API.SetTextJustification((int)justification);
                if (!disableTextOutline) { API.SetTextOutline(); }
                API.BeginTextCommandDisplayText("STRING");
                API.AddTextComponentSubstringPlayerName(text);
                API.EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
    }
}
