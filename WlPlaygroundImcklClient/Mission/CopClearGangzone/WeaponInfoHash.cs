using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    public static class WeaponInfoHash
    {
        // weapon
        public static readonly int WEAPON_UNARMED = API.GetHashKey("WEAPON_UNARMED");
        public static readonly int WEAPON_ANIMAL = API.GetHashKey("WEAPON_ANIMAL");
        public static readonly int WEAPON_COUGAR = API.GetHashKey("WEAPON_COUGAR");
        public static readonly int WEAPON_KNIFE = API.GetHashKey("WEAPON_KNIFE");
        public static readonly int WEAPON_NIGHTSTICK = API.GetHashKey("WEAPON_NIGHTSTICK");
        public static readonly int WEAPON_HAMMER = API.GetHashKey("WEAPON_HAMMER");
        public static readonly int WEAPON_BAT = API.GetHashKey("WEAPON_BAT");
        public static readonly int WEAPON_GOLFCLUB = API.GetHashKey("WEAPON_GOLFCLUB");
        public static readonly int WEAPON_CROWBAR = API.GetHashKey("WEAPON_CROWBAR");
        public static readonly int WEAPON_PISTOL = API.GetHashKey("WEAPON_PISTOL");
        public static readonly int WEAPON_COMBATPISTOL = API.GetHashKey("WEAPON_COMBATPISTOL");
        public static readonly int WEAPON_APPISTOL = API.GetHashKey("WEAPON_APPISTOL");
        public static readonly int WEAPON_PISTOL50 = API.GetHashKey("WEAPON_PISTOL50");
        public static readonly int WEAPON_MICROSMG = API.GetHashKey("WEAPON_MICROSMG");
        public static readonly int WEAPON_SMG = API.GetHashKey("WEAPON_SMG");
        public static readonly int WEAPON_ASSAULTSMG = API.GetHashKey("WEAPON_ASSAULTSMG");
        public static readonly int WEAPON_ASSAULTRIFLE = API.GetHashKey("WEAPON_ASSAULTRIFLE");
        public static readonly int WEAPON_CARBINERIFLE = API.GetHashKey("WEAPON_CARBINERIFLE");
        public static readonly int WEAPON_ADVANCEDRIFLE = API.GetHashKey("WEAPON_ADVANCEDRIFLE");
        public static readonly int WEAPON_MG = API.GetHashKey("WEAPON_MG");
        public static readonly int WEAPON_COMBATMG = API.GetHashKey("WEAPON_COMBATMG");
        public static readonly int WEAPON_PUMPSHOTGUN = API.GetHashKey("WEAPON_PUMPSHOTGUN");
        public static readonly int WEAPON_SAWNOFFSHOTGUN = API.GetHashKey("WEAPON_SAWNOFFSHOTGUN");
        public static readonly int WEAPON_ASSAULTSHOTGUN = API.GetHashKey("WEAPON_ASSAULTSHOTGUN");
        public static readonly int WEAPON_BULLPUPSHOTGUN = API.GetHashKey("WEAPON_BULLPUPSHOTGUN");
        public static readonly int WEAPON_STUNGUN = API.GetHashKey("WEAPON_STUNGUN");
        public static readonly int WEAPON_SNIPERRIFLE = API.GetHashKey("WEAPON_SNIPERRIFLE");
        public static readonly int WEAPON_HEAVYSNIPER = API.GetHashKey("WEAPON_HEAVYSNIPER");
        public static readonly int WEAPON_REMOTESNIPER = API.GetHashKey("WEAPON_REMOTESNIPER");
        public static readonly int WEAPON_GRENADELAUNCHER = API.GetHashKey("WEAPON_GRENADELAUNCHER");
        public static readonly int WEAPON_GRENADELAUNCHER_SMOKE = API.GetHashKey("WEAPON_GRENADELAUNCHER_SMOKE");
        public static readonly int WEAPON_RPG = API.GetHashKey("WEAPON_RPG");
        public static readonly int WEAPON_PASSENGER_ROCKET = API.GetHashKey("WEAPON_PASSENGER_ROCKET");
        public static readonly int WEAPON_AIRSTRIKE_ROCKET = API.GetHashKey("WEAPON_AIRSTRIKE_ROCKET");
        public static readonly int WEAPON_STINGER = API.GetHashKey("WEAPON_STINGER");
        public static readonly int WEAPON_MINIGUN = API.GetHashKey("WEAPON_MINIGUN");
        public static readonly int WEAPON_GRENADE = API.GetHashKey("WEAPON_GRENADE");
        public static readonly int WEAPON_STICKYBOMB = API.GetHashKey("WEAPON_STICKYBOMB");
        public static readonly int WEAPON_SMOKEGRENADE = API.GetHashKey("WEAPON_SMOKEGRENADE");
        public static readonly int WEAPON_BZGAS = API.GetHashKey("WEAPON_BZGAS");
        public static readonly int WEAPON_MOLOTOV = API.GetHashKey("WEAPON_MOLOTOV");
        public static readonly int WEAPON_FIREEXTINGUISHER = API.GetHashKey("WEAPON_FIREEXTINGUISHER");
        public static readonly int WEAPON_PETROLCAN = API.GetHashKey("WEAPON_PETROLCAN");
        public static readonly int WEAPON_DIGISCANNER = API.GetHashKey("WEAPON_DIGISCANNER");
        public static readonly int GADGET_NIGHTVISION = API.GetHashKey("GADGET_NIGHTVISION");
        public static readonly int GADGET_PARACHUTE = API.GetHashKey("GADGET_PARACHUTE");
        public static readonly int OBJECT = API.GetHashKey("OBJECT");
        public static readonly int WEAPON_BRIEFCASE = API.GetHashKey("WEAPON_BRIEFCASE");
        public static readonly int WEAPON_BRIEFCASE_02 = API.GetHashKey("WEAPON_BRIEFCASE_02");
        public static readonly int WEAPON_BALL = API.GetHashKey("WEAPON_BALL");
        public static readonly int WEAPON_FLARE = API.GetHashKey("WEAPON_FLARE");
        // vehicle weapon
        public static readonly int VEHICLE_WEAPON_TANK = API.GetHashKey("VEHICLE_WEAPON_TANK");
        public static readonly int VEHICLE_WEAPON_SPACE_ROCKET = API.GetHashKey("VEHICLE_WEAPON_SPACE_ROCKET");
        public static readonly int VEHICLE_WEAPON_PLANE_ROCKET = API.GetHashKey("VEHICLE_WEAPON_PLANE_ROCKET");
        public static readonly int VEHICLE_WEAPON_PLAYER_LASER = API.GetHashKey("VEHICLE_WEAPON_PLAYER_LASER");
        public static readonly int VEHICLE_WEAPON_PLAYER_BULLET = API.GetHashKey("VEHICLE_WEAPON_PLAYER_BULLET");
        public static readonly int VEHICLE_WEAPON_PLAYER_BUZZARD = API.GetHashKey("VEHICLE_WEAPON_PLAYER_BUZZARD");
        public static readonly int VEHICLE_WEAPON_PLAYER_HUNTER = API.GetHashKey("VEHICLE_WEAPON_PLAYER_HUNTER");
        public static readonly int VEHICLE_WEAPON_PLAYER_LAZER = API.GetHashKey("VEHICLE_WEAPON_PLAYER_LAZER");
        public static readonly int VEHICLE_WEAPON_ENEMY_LASER = API.GetHashKey("VEHICLE_WEAPON_ENEMY_LASER");
        public static readonly int VEHICLE_WEAPON_SEARCHLIGHT = API.GetHashKey("VEHICLE_WEAPON_SEARCHLIGHT");
        public static readonly int VEHICLE_WEAPON_RADAR = API.GetHashKey("VEHICLE_WEAPON_RADAR");
        public static readonly int WEAPON_VEHICLE_ROCKET = API.GetHashKey("WEAPON_VEHICLE_ROCKET");
        // other weapon
        public static readonly int WEAPON_BARBED_WIRE = API.GetHashKey("WEAPON_BARBED_WIRE");
        public static readonly int WEAPON_DROWNING = API.GetHashKey("WEAPON_DROWNING");
        public static readonly int WEAPON_DROWNING_IN_VEHICLE = API.GetHashKey("WEAPON_DROWNING_IN_VEHICLE");
        public static readonly int WEAPON_BLEEDING = API.GetHashKey("WEAPON_BLEEDING");
        public static readonly int WEAPON_ELECTRIC_FENCE = API.GetHashKey("WEAPON_ELECTRIC_FENCE");
        public static readonly int WEAPON_EXPLOSION = API.GetHashKey("WEAPON_EXPLOSION");
        public static readonly int WEAPON_FALL = API.GetHashKey("WEAPON_FALL");
        public static readonly int WEAPON_EXHAUSTION = API.GetHashKey("WEAPON_EXHAUSTION");
        public static readonly int WEAPON_HIT_BY_WATER_CANNON = API.GetHashKey("WEAPON_HIT_BY_WATER_CANNON");
        public static readonly int WEAPON_RAMMED_BY_CAR = API.GetHashKey("WEAPON_RAMMED_BY_CAR");
        public static readonly int WEAPON_RUN_OVER_BY_CAR = API.GetHashKey("WEAPON_RUN_OVER_BY_CAR");
        public static readonly int WEAPON_HELI_CRASH = API.GetHashKey("WEAPON_HELI_CRASH");
        public static readonly int VEHICLE_WEAPON_ROTORS = API.GetHashKey("VEHICLE_WEAPON_ROTORS");
        public static readonly int WEAPON_FIRE = API.GetHashKey("WEAPON_FIRE");
        public static readonly int WEAPON_ANIMAL_RETRIEVER = API.GetHashKey("WEAPON_ANIMAL_RETRIEVER");
        public static readonly int WEAPON_SMALL_DOG = API.GetHashKey("WEAPON_SMALL_DOG");
        public static readonly int WEAPON_TIGER_SHARK = API.GetHashKey("WEAPON_TIGER_SHARK");
        public static readonly int WEAPON_HAMMERHEAD_SHARK = API.GetHashKey("WEAPON_HAMMERHEAD_SHARK");
        public static readonly int WEAPON_KILLER_WHALE = API.GetHashKey("WEAPON_KILLER_WHALE");
        public static readonly int WEAPON_BOAR = API.GetHashKey("WEAPON_BOAR");
        public static readonly int WEAPON_PIG = API.GetHashKey("WEAPON_PIG");
        public static readonly int WEAPON_COYOTE = API.GetHashKey("WEAPON_COYOTE");
        public static readonly int WEAPON_DEER = API.GetHashKey("WEAPON_DEER");
        public static readonly int WEAPON_HEN = API.GetHashKey("WEAPON_HEN");
        public static readonly int WEAPON_RABBIT = API.GetHashKey("WEAPON_RABBIT");
        public static readonly int WEAPON_CAT = API.GetHashKey("WEAPON_CAT");
        public static readonly int WEAPON_COW = API.GetHashKey("WEAPON_COW");
        public static readonly int WEAPON_BIRD_CRAP = API.GetHashKey("WEAPON_BIRD_CRAP");
    }
}
