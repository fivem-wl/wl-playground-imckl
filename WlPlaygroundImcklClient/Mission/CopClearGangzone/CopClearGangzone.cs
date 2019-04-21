using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{

    public struct MissionInfo
    {
        /// <summary>
        /// 任务人物信息
        /// </summary>
        public List<MissionPedInfo> PedsInfo { get; set; }

        /// <summary>
        /// 任务范围
        /// </summary>
        public MissionRangeInfo RangeInfo { get; set; }

        /// <summary>
        /// 任务武器
        /// </summary>
        public List<MissionWeaponInfo> PlayerWeapons { get; set; }

        /// <summary>
        /// 任务时间
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }
    }

    public struct MissionRangeInfo
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }


        public bool IsPlayerIn
        {
            get => Position.DistanceToSquared(Game.PlayerPed.Position) <= Math.Pow(Radius, 2);
        }

        public bool IsPositionIn(Vector3 position)
            => Position.DistanceToSquared(position) <= Math.Pow(Radius, 2);
    }

    public struct MissionPedInfo
    {
        public PedHash PedHash { get; set; }
        public int Accuracy { get; set; }
        public int Health { get; set; }
        public Vector3 Position { get; set; }
        /// <summary>
        /// 是否爆头击杀
        /// </summary>
        public bool IsHeadshotImmute { get; set; }

        public bool IsWanderAround { get; set; }
        public float WanderRange { get; set; }

        public List<MissionWeaponInfo> Weapons { get; set; }
    }

    public struct MissionWeaponInfo
    {
        public uint Hash { get; set; }
        public int Ammo { get; set; }
        
    }

    public delegate void StoppedEvent(string reason);
    public class MissionTimer
    {
        
        public event StoppedEvent OnStop;

        public int Duration { get; set; }
        public int StartTime { get; }
        public int RemainTime { get => Duration - (Game.GameTime - StartTime); }

        private bool _stopped { get; set; }

        public MissionTimer(int duration)
        {
            Duration = duration;
            StartTime = Game.GameTime;
        }

        public async Task TickAsync()
        {
            if (!_stopped)
            {
                if (RemainTime <= 0)
                {
                    OnStop?.Invoke("timeup");
                    _stopped = true;
                }
            }
            await BaseScript.Delay(100);
        }

    }

    public class MissionInstance : IDisposable
    {
        public List<Ped> Peds { get; set; } = new List<Ped>();
        public Blip RadiusBlip { get; set; }
        public MissionTimer Timer { get; set; }
        public event StoppedEvent OnStop;

        public MissionInstance(int duration)
        {
            Timer = new MissionTimer(duration);

            FatalDamageEvents.OnPlayerDead += OnPlayerDead;
            Timer.OnStop += _OnStop;
        }

        public void Dispose()
        {

            // 解除通缉状态
            API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
            API.SetPlayerWantedLevelNow(Game.Player.Handle, false);

            Peds?.ForEach(p =>
            {
                p?.AttachedBlip?.Delete();
                p?.Delete();
            });
            
            RadiusBlip?.Delete();

            FatalDamageEvents.OnPlayerDead -= OnPlayerDead;
            Timer.OnStop -= _OnStop;

        }

        private void OnPlayerDead()
        {
            OnStop?.Invoke("dead");
        }

        private void _OnStop(string reason)
        {
            OnStop?.Invoke(reason);
        }

        private int LastSetCopGroupTime { get; set; } = 0;
        private bool IsMeetSetCopGroupInterval
        {
            get
            {
                if (Game.GameTime - LastSetCopGroupTime > 1000)
                {
                    LastSetCopGroupTime = Game.GameTime;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public async Task EveryFrameTickAsync()
        {
            // 显示剩余时间
            var remainTime = TimeSpan.FromMilliseconds(Timer.RemainTime);
            var remainTimeString = string.Format(
                "剩余时间 - {1:D2}:{2:D2}",
                remainTime.Hours, remainTime.Minutes, remainTime.Seconds, remainTime.Milliseconds);
            GUI.DrawTextOnScreen(remainTimeString,
                GUI.SafeZone.Right - 0.1f, GUI.SafeZone.Bottom - API.GetTextScaleHeight(0.4f, 1) - 0.01f, 0.4f, Alignment.Center, 1, false);

            await Task.FromResult(0);
        }

        public async Task TickAsync()
        {
            
            if(Peds.All(p => p.IsDead))
            {
                Debug.WriteLine("Mission complete");
                OnStop?.Invoke("finish");
            }
            else
            {
                // 保持通缉状态
                API.SetPlayerWantedLevel(Game.Player.Handle, 5, false);
                API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
            }

            Peds?.ForEach(p =>
            {
                if (p.IsDead)
                    p.AttachedBlip?.Delete();
            });

            // 设置警察为同伴
            if (IsMeetSetCopGroupInterval)
            {
                foreach (var ped in World.GetAllPeds())
                {
                    if (ped.IsPlayer)
                        continue;
                    var pedType = API.GetPedType(ped.Handle);
                    if (pedType == 6 || pedType == 27 || pedType == 29)
                    {
                        if (ped.RelationshipGroup != CopClearGangzone.CopGroup)
                            ped.RelationshipGroup = CopClearGangzone.CopGroup;
                    }
                }
            }
            

            await Timer.TickAsync();

            await BaseScript.Delay(100);

        }
    }

    public class CopClearGangzone : BaseScript
    {

        public static string ResourceName = "CopClearGangzone";

        private List<MissionInfo> MissionsInfo;
        private MissionInstance MissionInstance;

        

        private async Task<bool> CreateMission()
        {
            MissionsInfo = new List<MissionInfo>()
            {
                new MissionInfo()
                {
                    Duration = 1000 * 60 * 10,
                    PedsInfo = new List<MissionPedInfo>()
                    {
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.RsRanger01AMO,
                            Accuracy = 20,
                            Health = 1000,
                            Position = new Vector3(-436f, 1128f, 332f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.GrenadeLauncher,
                                    Ammo = 300,
                                },
                            }
                        },
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.JohnnyKlebitz,
                            Accuracy = 100,
                            Health = 1000,
                            Position = new Vector3(-436f, 1128f, 332f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.SniperRifle,
                                    Ammo = 300,
                                },
                            }
                        },

                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.RsRanger01AMO,
                        //    Accuracy = 20,
                        //    Health = 1000,
                        //    Position = new Vector3(-420f, 1108f, 332f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.GrenadeLauncher,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.JohnnyKlebitz,
                            Accuracy = 100,
                            Health = 1000,
                            Position = new Vector3(-420f, 1108f, 332f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.SniperRifle,
                                    Ammo = 300,
                                },
                            }
                        },

                        new MissionPedInfo()
                        {
                            PedHash = PedHash.Mani,
                            Accuracy = 100,
                            Health = 1000,
                            Position = new Vector3(-432f, 1102f, 340f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.RPG,
                                    Ammo = 300,
                                },
                            }
                        },

                        new MissionPedInfo()
                        {
                            PedHash = PedHash.Orleans,
                            Accuracy = 100,
                            Health = 2000,
                            Position = new Vector3(-439f, 1074f, 353f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.RPG,
                                    Ammo = 300,
                                },
                            }
                        },

                        // footman
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.PestContGunman,
                            Accuracy = 20,
                            Health = 500,
                            Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.CarbineRifleMk2,
                                    Ammo = 300,
                                },
                            }
                        },
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.PestContGunman,
                            Accuracy = 20,
                            Health = 500,
                            Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.CarbineRifleMk2,
                                    Ammo = 300,
                                },
                            }
                        },
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.PestContGunman,
                            Accuracy = 20,
                            Health = 500,
                            Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.CarbineRifleMk2,
                                    Ammo = 300,
                                },
                            }
                        },
                        new MissionPedInfo()
                        {
                            PedHash = PedHash.PestContGunman,
                            Accuracy = 20,
                            Health = 500,
                            Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                            IsHeadshotImmute = true,
                            IsWanderAround = true,
                            WanderRange = 10f,
                            Weapons = new List<MissionWeaponInfo>()
                            {
                                new MissionWeaponInfo()
                                {
                                    Hash = (uint)WeaponHash.CarbineRifleMk2,
                                    Ammo = 300,
                                },
                            }
                        },
                        //PestContGunman - gunman
                    },
                    RangeInfo = new MissionRangeInfo()
                    {
                        Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                        Radius = 200f,
                    },
                    PlayerWeapons = new List<MissionWeaponInfo>()
                    {
                        new MissionWeaponInfo()
                        {
                            Hash = (uint)WeaponHash.AssaultRifle,
                            Ammo = 480,
                        },
                        new MissionWeaponInfo()
                        {
                            Hash = (uint)WeaponHash.Flashlight,
                            Ammo = 99,
                        },
                        new MissionWeaponInfo()
                        {
                            Hash = (uint)WeaponHash.SniperRifle,
                            Ammo = 30,
                        },
                        new MissionWeaponInfo()
                        {
                            Hash = (uint)WeaponHash.VintagePistol,
                            Ammo = 99,
                        },
                        new MissionWeaponInfo()
                        {
                            Hash = (uint)WeaponHash.StunGun,
                            Ammo = 99,
                        },
                    },
                    Description = "一群有~r~重火力的恐怖分子~s~占领了~y~天文台~s~, 为了维护世界和平, 请立即赶往~y~天文台~s~并协助警方制止他们!",
                }
            };

            DestroyMission();
            MissionInstance = new MissionInstance(MissionsInfo[0].Duration);

            // 创建任务人物
            foreach(var pi in MissionsInfo[0].PedsInfo)
            {

                Debug.WriteLine(pi.ToString());

                var p = await CreateMissionPed(pi);
                if (p is null)
                {
                    MissionInstance.Dispose();
                    return false;
                }
                else
                {
                    MissionInstance.Peds.Add(p);
                }
            }

            // 创建范围Blip
            MissionInstance.RadiusBlip = World.CreateBlip(MissionsInfo[0].RangeInfo.Position, MissionsInfo[0].RangeInfo.Radius);
            MissionInstance.RadiusBlip.Alpha = 64;
            MissionInstance.RadiusBlip.Color = BlipColor.Red;

            // 自定义玩家武器
            Game.PlayerPed.Weapons.RemoveAll();
            MissionsInfo[0].PlayerWeapons.ForEach(
                w => Game.PlayerPed.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));
            
            Tick += MissionInstance.TickAsync;
            Tick += MissionInstance.EveryFrameTickAsync;

            MissionInstance.OnStop += StopMission;

            // 发出提醒
            Notify.CustomImage("CHAR_CALL911", "CHAR_CALL911", MissionsInfo[0].Description, "LSPD", "制止区域暴乱", true, 2);
            API.PlaySoundFrontend(-1, "Boss_Message_Orange", "GTAO_Boss_Goons_FM_Soundset", false);

            return true;

        }

        private void DestroyMission()
        {

            if (!(MissionInstance is null))
            {
                Tick -= MissionInstance.TickAsync;
                Tick -= MissionInstance.EveryFrameTickAsync;

                MissionInstance.OnStop -= StopMission;

                MissionInstance.Dispose();
            }

        }

        private async void StopMission(string reason)
        {
            DestroyMission();
            await HintOnStopMission(reason);
        }

        private async Task HintOnStopMission(string reason)
        {
            var now = Game.GameTime;
            Scaleform scaleform;
            switch (reason)
            {
                case "finish":
                    // 音乐
                    if (API.GetResourceState("interact-sound") == "started")
                        TriggerEvent("LIFE_CL:Sound:PlayOnOne", "gta_iv_mission_completed_sound", 30);
                    else
                        API.PlaySoundFrontend(-1, "Mission_Pass_Notify", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS", true);
                    // 界面
                    scaleform = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
                    while (!scaleform.IsLoaded)
                    {
                        await Delay(100);
                    }
                    scaleform.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", API.GetLabelText("BM_PASS"), "~y~评分: 100~s~", 5);
                    while (Game.GameTime - now <= 1000 * 8)
                    {
                        scaleform.Render2D();
                        await Delay(0);
                    }
                    scaleform.Dispose();
                    break;
                case "dead":
                case "timeup":
                default:
                    // 音乐
                    API.PlaySoundFrontend(-1, "MP_Flash", "WastedSounds", true);
                    // 界面
                    scaleform = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
                    while (!scaleform.IsLoaded)
                    {
                        await Delay(100);
                    }
                    scaleform.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", API.GetLabelText("REPLAY_T"), "~y~评分: 0~s~", 5);
                    while (Game.GameTime - now <= 1000 * 8)
                    {
                        scaleform.Render2D();
                        await Delay(0);
                    }
                    scaleform.Dispose();
                    break;
            }
        }

        private async Task<Ped> CreateMissionPed(MissionPedInfo pedInfo)
        {
            var model = new Model(pedInfo.PedHash);
            await model.Request(1000 * 10);

            if (!model.IsLoaded)
                return null;

            var ped = await World.CreatePed(model, pedInfo.Position);

            if (ped is null)
                return null;

            ped.RelationshipGroup = MissionGroup;

            ped.Accuracy = pedInfo.Accuracy;
            ped.MaxHealthFloat = pedInfo.Health;
            ped.HealthFloat = pedInfo.Health;
            ped.CanSufferCriticalHits = !pedInfo.IsHeadshotImmute;

            ped.AlwaysDiesOnLowHealth = false;
            ped.CanSwitchWeapons = true;
            // 是否会倒地挣扎?
            ped.CanWrithe = false;
            // 死亡掉落物器
            ped.DropsWeaponsOnDeath = false;
            ped.DiesInstantlyInWater = false;
            ped.DrownsInSinkingVehicle = false;
            ped.DrownsInWater = false;

            ped.IsInvincible = false;
            ped.IsVisible = true;
            ped.IsFireProof = true;
            ped.CanRagdoll = false;

            ped.FatalInjuryHealthThreshold = 0;
            ped.InjuryHealthThreshold = 0;

            // 控制AI动作?
            //ped.Euphoria.BodyWrithe.
            //ped.Euphoria.SetCharacterHealth.CharacterHealth
            // AI射击倾向/模式 - see API.SetPedFiringPattern
            //ped.FiringPattern
            // AI视野相关
            // ped.IsInAngledArea
            // AI警戒范围相关
            // ped.IsInArea
            // ped.IsInRangeOf
            // ped.IsNearEntity
            // ped.IsOnScreen
            // 是否在警车
            //ped.IsInPoliceVehicle
            // ???
            // API.IsEntityOccluded
            // 是否渲染?
            // ped.IsRendered
            // cash
            // ped.Money
            // networkid
            //ped.NetworkId
            // ped group relative
            //var x = ped.PedGroup;
            //x.Leader
            // 四元数
            //ped.Quaternion
            // relationship group relative
            //var x = ped.RelationshipGroup;
            // 复活
            //ped.Resurrect
            // set the rate this Ped will shoot at.
            //ped.ShootRate

            ped.IsEnemy = true;
            ped.IsOnlyDamagedByPlayer = false;

            //ped.BlockPermanentEvents = true;
            //API.TaskSetBlockingOfNonTemporaryEvents(ped.Handle, true);

            // persistent
            ped.IsPersistent = true;

            pedInfo.Weapons.ForEach(w => ped.Weapons.Give((WeaponHash)w.Hash, w.Ammo, false, true));

            ped.AttachBlip();

            if (pedInfo.IsWanderAround)
            {
                ped.Task.WanderAround(pedInfo.Position, pedInfo.WanderRange);
            }

            ped.FiringPattern = FiringPattern.BurstFireBursts;
            
            return ped;

        }

        public static RelationshipGroup MissionGroup = World.AddRelationshipGroup("MissionGroup");
        public static RelationshipGroup CopGroup = World.AddRelationshipGroup("CopGroup");
        public static RelationshipGroup PlayerGroup = World.AddRelationshipGroup("PlayerGroup");

        private static Menus.MainMenu MainMenu { get; set; }
        private async Task ToggleMenuCheckAsync()
        {
            if (Game.PlayerPed.IsInPoliceVehicle && API.IsControlPressed(0, (int)Control.Context))
            {
                MainMenu?.GetMenu()?.OpenMenu();
            }
            await Delay(1000);
        }

        public CopClearGangzone()
        {

            MainMenu = new Menus.MainMenu();
            MainMenu.CreateMenu();

            EventHandlers.Add($"{ResourceName}:CreateMission", new Func<Task<bool>>(CreateMission));

            Tick += ToggleMenuCheckAsync;

            Game.PlayerPed.RelationshipGroup = PlayerGroup;

            PlayerGroup.SetRelationshipBetweenGroups(CopGroup, Relationship.Companion, true);
            PlayerGroup.SetRelationshipBetweenGroups(MissionGroup, Relationship.Hate, true);

            MissionGroup.SetRelationshipBetweenGroups(CopGroup, Relationship.Hate, true);


            //Tick += new Func<Task>(async () =>
            //{
            //    foreach(var ped in World.GetAllPeds())
            //    {
            //        if (ped.IsPlayer)
            //            continue;
            //        var pedType = API.GetPedType(ped.Handle);
            //        if (pedType == 6 || pedType == 27 || pedType == 29)
            //        {
            //            if (ped.RelationshipGroup != CopGroup)
            //                ped.RelationshipGroup = CopGroup;
            //        }
            //    }
            //    await Delay(1000);
            //});


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

                        // AUDIO::PLAY_SOUND_FRONTEND(-1, "RANK_UP", "HUD_AWARDS", 1); // 任务完成

                        break;
                }

                await Task.FromResult(0);
            }), false);

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
                //API.PlaySoundFrontend(-1, "UNDER_THE_BRIDGE", "HUD_AWARDS", true);
                //API.PlaySoundFrontend(-1, "MP_Flash", "WastedSounds", true);
                // API.PlaySoundFrontend(-1, "Mission_Pass_Notify", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS", true);
                // REPLAY_T => mission failed
                while (Game.GameTime - now <= 1000 * 8)
                {
                    scaleform.Render2D();
                    await Delay(0);
                }
            }), false);

        }

    }

}
