using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone
{
    public enum RelationshipBaseGroup : int
    {
        PLAYER = 0x6F0783F5,
        CIVMALE = 0x02B8FA80,
        CIVFEMALE = 0x47033600,
        COP = unchecked((int)0xA49E591C),
        SECURITY_GUARD = unchecked((int)0xF50B51B7),
        PRIVATE_SECURITY = unchecked((int)0xA882EB57),
        FIREMAN = unchecked((int)0xFC2CA767),
        GANG_1 = 0x4325F88A,
        GANG_2 = 0x11DE95FC,
        GANG_9 = unchecked((int)0x8DC30DC3),
        GANG_10 = 0x0DBF2731,
        AMBIENT_GANG_LOST = unchecked((int)0x90C7DA60),
        AMBIENT_GANG_MEXICAN = 0x11A9A7E3,
        AMBIENT_GANG_FAMILY = 0x45897C40,
        AMBIENT_GANG_BALLAS = unchecked((int)0xC26D562A),
        AMBIENT_GANG_MARABUNTE = 0x7972FFBD,
        AMBIENT_GANG_CULT = 0x783E3868,
        AMBIENT_GANG_SALVA = unchecked((int)0x936E7EFB),
        AMBIENT_GANG_WEICHENG = 0x6A3B9F86,
        AMBIENT_GANG_HILLBILLY = unchecked((int)0xB3598E9C),
        DEALER = unchecked((int)0x8296713E),
        HATES_PLAYER = unchecked((int)0x84DCFAAD),
        HEN = unchecked((int)0xC01035F9),
        WILD_ANIMAL = 0x7BEA6617,
        SHARK = 0x229503C8,
        COUGAR = unchecked((int)0xCE133D78),
        NO_RELATIONSHIP = unchecked((int)0xFADE4843),
        SPECIAL = unchecked((int)0xD9D08749),
        MISSION2 = unchecked((int)0x80401068),
        MISSION3 = 0x49292237,
        MISSION4 = 0x5B4DC680,
        MISSION5 = 0x270A5DFA,
        MISSION6 = 0x392C823E,
        MISSION7 = 0x024F9485,
        MISSION8 = 0x14CAB97B,
        ARMY = unchecked((int)0xE3D976F3),
        GUARD_DOG = 0x522B964A,
        AGGRESSIVE_INVESTIGATE = unchecked((int)0xEB47D4E0),
        MEDIC = unchecked((int)0xB0423AA0),
        PRISONER = 0x7EA26372,
        DOMESTIC_ANIMAL = 0x72F30F6E,
        DEER = 0x31E50E10,
    }
}
