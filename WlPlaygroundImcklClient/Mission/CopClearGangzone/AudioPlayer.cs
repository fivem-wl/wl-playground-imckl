using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{

    public enum AudioName
    {
        Beep = 1,
        MissionComplete,
        Wasted,
    }

    public static class AudioPlayer
    {
        // AUDIO::PLAY_SOUND_FRONTEND(-1, "RANK_UP", "HUD_AWARDS", 1); // 任务完成
        // API.PlaySoundFrontend(-1, "Mission_Pass_Notify", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS", true);

        public static int Play(AudioName audioName)
        {
            switch (audioName)
            {
                case AudioName.Beep:
                    return PlayBeep();
                case AudioName.MissionComplete:
                    return PlayMissionComplete();
                case AudioName.Wasted:
                    return PlayWasted();
            }
            return 0;
        }

        private static int PlayBeep()
        {
            return Audio.PlaySoundFrontend("Boss_Message_Orange", "GTAO_Boss_Goons_FM_Soundset");
        }

        private static int PlayMissionComplete()
        {
            if (API.GetResourceState("wl-interact-sound") == "started")
            {
                BaseScript.TriggerEvent("LIFE_CL:Sound:PlayOnOne", "gta_sa_mission_completed_sound", 30);
                return 0;
            } 
            else
            {
                API.PlaySoundFrontend(-1, "Mission_Pass_Notify", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS", true);
                return API.GetSoundId();
            }
        }

        private static int PlayWasted()
        {
            return Audio.PlaySoundFrontend("MP_Flash", "WastedSounds");
        }

        private static void _registerTestCommand()
        {
            API.RegisterCommand("mytest", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                if (args.Count <= 0)
                    return;

                var cmd1 = args[0].ToString();

                switch (cmd1)
                {
                    case "PlaySoundFrontend":
                        if (args.Count < 4)
                            return;
                        var audioName = args[1].ToString();
                        var audioRef = args[2].ToString();
                        var p3 = int.Parse(args[3].ToString()) >= 1 ? true : false;
                        API.PlaySoundFrontend(-1, audioName, audioRef, p3);
                        break;
                }

                await Task.FromResult(0);
            }), false);
        }

    }
}
