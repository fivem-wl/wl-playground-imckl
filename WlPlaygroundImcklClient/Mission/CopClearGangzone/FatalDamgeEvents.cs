using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(Ped attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);


    class FatalDamageEvents : BaseScript
    {
        
        public static event PlayerKillPlayerEvent OnPlayerKillPlayer;
        public static event PlayerKillPedEvent OnPlayerKillPed;
        public static event PedKillPlayerEvent OnPedKillPlayer;
        public static event PedKillPedEvent OnPedKillPed;
        public static event EntityKillEntityEvent OnEntityKillEntity;
        public static event DeadEvent OnDeath;
        
        /// <summary>
        /// Handle game event CEventNetworkEntityDamage,
        /// Useful for indicating entity damage/died/destroyed.
        /// </summary>
        /// <param name="victim">victim</param>
        /// <param name="attacker">attacker</param>
        /// <param name="arg2">Unknown</param>
        /// <param name="isDamageFatal">Is damage fatal to entity. or victim died/destroyed.</param>
        /// <param name="weaponInfoHash">Probably related to common.rpf/data/ai => Item type = "CWeaponInfo"</param>
        /// <param name="arg5">Unknown</param>
        /// <param name="arg6">Unknown</param>
        /// <param name="arg7">Unknown, might be int</param>
        /// <param name="arg8">Unknown, might be int</param>
        /// <param name="isMeleeDamage">Is melee damage</param>
        /// <param name="damageTypeFlag">0 for peds, 116 for the body of a vehicle, 93 for a tire, 120 for a side window, 121 for a rear window, 122 for a windscreen, etc</param>
        private void HandleCEventNetworkEntityDamaged(
            Entity victim, Entity attacker, int arg2, bool isDamageFatal, uint weaponInfoHash,
            int arg5, int arg6, object arg7, object arg8, bool isMeleeDamage,
            int damageTypeFlag)
        {
            // 致命伤
            if (isDamageFatal)
            {
                bool isAttackerPed = false;
                bool isAttackerPlayer = false;
                Ped pedAttacker = null;
                Player playerAttacker = null;
                // 攻击者是Ped
                if (attacker is Ped)
                {
                    pedAttacker = (Ped)attacker;
                    isAttackerPed = true;
                    // 攻击者是Player
                    if (pedAttacker.IsPlayer)
                    {
                        playerAttacker = new Player(API.NetworkGetPlayerIndexFromPed(pedAttacker.Handle));
                        isAttackerPlayer = true;
                    }
                }
                bool isVictimPed = false;
                bool isVictimPlayer = false;
                bool isVictimThisPlayer = false;
                Ped pedVictim = null;
                Player playerVictim = null;
                // 受害者是Ped
                if (victim is Ped)
                {
                    pedVictim = (Ped)victim;
                    isVictimPed = true;
                    // 受害者是Player
                    if (pedVictim.IsPlayer)
                    {
                        playerVictim = new Player(API.NetworkGetPlayerIndexFromPed(pedVictim.Handle));
                        isVictimPlayer = true;
                        if (playerVictim == Game.Player)
                        {
                            isVictimThisPlayer = true;
                        }
                    }
                }

                if (isAttackerPlayer && isVictimPlayer)
                {
                    OnPlayerKillPlayer?.Invoke(playerAttacker, playerVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else if (isAttackerPlayer && isVictimPed)
                {
                    OnPlayerKillPed?.Invoke(playerAttacker, pedVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else if (isAttackerPed && isVictimPlayer)
                {
                    OnPedKillPlayer?.Invoke(pedAttacker, playerVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else if (isVictimPed && isAttackerPed)
                {
                    OnPedKillPed?.Invoke(pedAttacker, pedVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else
                {
                    OnEntityKillEntity?.Invoke(attacker, victim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }

                if (isVictimThisPlayer)
                {
                    OnDeath?.Invoke(attacker, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }

            }

        }

        public FatalDamageEvents()
        {
            EventHandlers.Add("gameEventTriggered", new Action<string, List<object>>((string eventName, List<object> args) =>
            {
                if (eventName == "CEventNetworkEntityDamage")
                {
                    Entity victim = Entity.FromHandle(int.Parse(args[0].ToString()));
                    Entity attacker = Entity.FromHandle(int.Parse(args[1].ToString()));
                    bool isDamageFatal = int.Parse(args[3].ToString()) == 1;
                    uint weaponInfoHash = (uint)int.Parse(args[4].ToString());
                    bool isMeleeDamage = int.Parse(args[9].ToString()) != 0;
                    int damageTypeFlag = int.Parse(args[10].ToString());
                    HandleCEventNetworkEntityDamaged(
                        victim, attacker, int.Parse(args[2].ToString()), isDamageFatal, weaponInfoHash,
                        int.Parse(args[5].ToString()), int.Parse(args[6].ToString()), args[7], args[8], isMeleeDamage,
                        damageTypeFlag);
                }
            }));
        }
    }
}
