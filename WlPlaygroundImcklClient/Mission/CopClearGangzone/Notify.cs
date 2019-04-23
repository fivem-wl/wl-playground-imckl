using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    /// <summary>
    /// 发送提醒至玩家.
    /// Port from - <seealso cref="https://github.com/TomGrobbe/vMenu"/>
    /// </summary>
    public static class Notify
    {

        /// <summary>
        /// 适应汉字大小的StringToArray
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string[] StringToArray(string inputString)
        {
            int stringsNeeded = (inputString.Length % 30 == 0) ? (inputString.Length / 30) : (inputString.Length / 30 + 1);
            string[] outputString = new string[stringsNeeded];
            for (int i = 0; i < stringsNeeded; i++)
            {
                outputString[i] = inputString.Substring(i * 30, MathUtil.Clamp(inputString.Substring(i * 30).Length, 0, 30));
            }
            return outputString;
        }

        /// <summary>
        /// Show a custom notification above the minimap.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Custom(string message, bool blink = true, bool saveToBrief = true)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            StringToArray(message)
                .ToList().ForEach(s => AddTextComponentSubstringPlayerName(s));
            DrawNotification(blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Alert: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Alert(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~y~~h~[提醒]~h~~s~" + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Error: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Error(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~r~~h~[错误]~h~~s~" + message, blink, saveToBrief);
            Debug.Write($"[{GetCurrentResourceName()}][ERROR] " + message + "\n");
        }

        /// <summary>
        /// Show a notification with "Info: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Info(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~b~~h~[信息]~h~~s~" + message, blink, saveToBrief);
        }

        /// <summary>
        /// Show a notification with "Success: " prefixed to the message.
        /// </summary>
        /// <param name="message">The message to be displayed on the notification.</param>
        /// <param name="blink">Should the notification blink 3 times?</param>
        /// <param name="saveToBrief">Should the notification be logged to the brief (PAUSE menu > INFO > Notifications)?</param>
        public static void Success(string message, bool blink = true, bool saveToBrief = true)
        {
            Custom("~g~~h~[成功]~h~~s~" + message, blink, saveToBrief);
        }

        /// <summary>
        /// Shows a custom notification with an image attached.
        /// </summary>
        /// <param name="textureDict"></param>
        /// <param name="textureName"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="subtitle"></param>
        /// <param name="safeToBrief"></param>
        public static void CustomImage(string textureDict, string textureName, string message, string title, string subtitle, bool saveToBrief, int iconType = 0)
        {
            SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            StringToArray(message)
               .ToList().ForEach(s => AddTextComponentSubstringPlayerName(s));
            SetNotificationMessage(textureName, textureDict, false, iconType, title, subtitle);
            DrawNotification(false, saveToBrief);
        }
    }
}
