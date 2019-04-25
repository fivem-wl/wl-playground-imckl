using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using WlPlaygroundImcklShared.Mission.CopClearGangzone;


namespace WlPlaygroundImcklServer.Mission.CopClearGangzone
{
    public class Storage
    {

        public static MissionsInfo GetAll()
        {
            var missionInfo = new MissionInfo()
            {
                Duration = 1000 * 60 * 15,
                PedsInfo = new List<MissionPedInfo>()
                    {
                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.RsRanger01AMO,
                        //    Accuracy = 20,
                        //    Health = 1000,
                        //    Position = new Vector3(-436f, 1128f, 332f),
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
                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.JohnnyKlebitz,
                        //    Accuracy = 100,
                        //    Health = 1000,
                        //    Position = new Vector3(-436f, 1128f, 332f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.SniperRifle,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},
                        
                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.JohnnyKlebitz,
                        //    Accuracy = 100,
                        //    Health = 1000,
                        //    Position = new Vector3(-420f, 1108f, 332f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.SniperRifle,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},

                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.Mani,
                        //    Accuracy = 100,
                        //    Health = 1000,
                        //    Position = new Vector3(-432f, 1102f, 340f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.RPG,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},

                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.Orleans,
                        //    Accuracy = 100,
                        //    Health = 2000,
                        //    Position = new Vector3(-439f, 1074f, 353f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.RPG,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},

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
                            PedHash = PedHash.Hooker01SFY,
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
                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.PestContGunman,
                        //    Accuracy = 20,
                        //    Health = 500,
                        //    Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.CarbineRifleMk2,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},
                        //new MissionPedInfo()
                        //{
                        //    PedHash = PedHash.PestContGunman,
                        //    Accuracy = 20,
                        //    Health = 500,
                        //    Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                        //    IsHeadshotImmute = true,
                        //    IsWanderAround = true,
                        //    WanderRange = 10f,
                        //    Weapons = new List<MissionWeaponInfo>()
                        //    {
                        //        new MissionWeaponInfo()
                        //        {
                        //            Hash = (uint)WeaponHash.CarbineRifleMk2,
                        //            Ammo = 300,
                        //        },
                        //    }
                        //},
                        //PestContGunman - gunman
                    },
                RangeInfo = new MissionRangeInfo()
                {
                    Position = new Vector3(-418.77f, 1147.17f, 325.86f),
                    Radius = 500f,
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

                ScheduleNotificationInfo = new MissionNotificationInfo()
                {
                    Sender = "CCGV",
                    Subject = "特别新闻报道",
                    Message = "有一群~r~暴徒~s~占领了~y~天文台~s~, 热心市民可~y~抢劫~s~一台~y~警车~s~并协助警方打击暴徒, 维护社会稳定, 人人有责",
                },

                StartNotificationInfo = new MissionNotificationInfo()
                {
                    Sender = "LSPD",
                    Subject = "制止区域暴乱",
                    Message = "一群无法无天的~r~暴徒~s~占领了~y~天文台~s~, 为了维护世界和平, 请立即赶往~y~天文台~s~并协助警方制止他们!",
                },

                EndNotificationInfo = new MissionNotificationInfo()
                {
                    Sender = "CCGV",
                    Subject = "特别新闻报道",
                    Message = "todo",
                },

                HintSubtitle = "前往~y~天文台~s~并制服所有~r~暴徒~s~",
            };
            var missionsInfo = new MissionsInfo();
            missionsInfo.Add(missionInfo);

            return missionsInfo;
        }

    }
}
