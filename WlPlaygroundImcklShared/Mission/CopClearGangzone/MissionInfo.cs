﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Newtonsoft.Json;

using CitizenFX.Core;
using CitizenFX.Core.Native;


namespace WlPlaygroundImcklShared.Mission.CopClearGangzone
{

    /// <summary>
    /// <seealso cref="MissionInfo"/>列表
    /// </summary>
    public class MissionsInfo
    {

        private List<MissionInfo> _MissionsInfo;

        public MissionsInfo()
        {
            _MissionsInfo = new List<MissionInfo>();
        }

        public MissionsInfo(List<MissionInfo> missionsInfo)
        {
            _MissionsInfo = missionsInfo;
        }

        public MissionInfo this[int index]
        {
            get => _MissionsInfo[index];
            set => _MissionsInfo[index] = value;
        }

        /// <summary>
        /// 添加新的<seealso cref="MissionInfo"/>到列表尾
        /// </summary>
        /// <param name="missionInfo"></param>
        public void Add(MissionInfo missionInfo)
        {
            _MissionsInfo.Add(missionInfo);
        }

        /// <summary>
        /// 移除第一个匹配的<seealso cref="MissionInfo"/>
        /// </summary>
        /// <param name="missionInfo"></param>
        public void Remove(MissionInfo missionInfo)
            => _MissionsInfo.Remove(missionInfo);

        /// <summary>
        /// 序列化<seealso cref="MissionsInfo"/>
        /// </summary>
        /// <returns></returns>
        public string Serialize()
            => JsonConvert.SerializeObject(_MissionsInfo);

        /// <summary>
        /// 序列化指定的<seealso cref="MissionInfo"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string Serialize(int index)
            => JsonConvert.SerializeObject(_MissionsInfo[index]);

        /// <summary>
        /// 反序列化<seealso cref="MissionsInfo"/>
        /// </summary>
        /// <param name="missionsInfoJson"></param>
        /// <returns></returns>
        public static List<MissionInfo> Deserialize(string missionsInfoJson)
            => JsonConvert.DeserializeObject<List<MissionInfo>>(missionsInfoJson);

        /// <summary>
        /// 返回任务列表中的所有<seealso cref="PedHash"/>(unique)
        /// </summary>
        /// <returns></returns>
        public List<PedHash> GetPedsHash()
            => _MissionsInfo
               .SelectMany(mi => mi.PedsInfo.Select(pi => pi.PedHash))
               .Distinct().ToList();

        /// <summary>
        /// 返回指定任务列表中的<seealso cref="PedHash"/>(unqiue)
        /// </summary>
        /// <param name="missionIndex"></param>
        /// <returns></returns>
        public List<PedHash> GetPedsHash(int missionIndex)
            => _MissionsInfo[missionIndex]
                .PedsInfo.Select(pi => pi.PedHash)
                .Distinct().ToList();

        /// <summary>
        /// 返回任务列表中的所有<seealso cref="WeaponHash"/>(unqiue)
        /// </summary>
        /// <returns></returns>
        public List<WeaponHash> GetWeaponsHash() => Enumerable.Union(
                _MissionsInfo.SelectMany(mi => mi.PlayerWeapons.Select(pw => (WeaponHash)pw.Hash)),
                _MissionsInfo.SelectMany(mi => mi.PedsInfo.SelectMany(pi => pi.Weapons.Select(wi => (WeaponHash)wi.Hash))))
            .ToList();
            

    }

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
        /// 任务计划描述
        /// </summary>
        public MissionNotificationInfo ScheduleNotificationInfo { get; set; }

        /// <summary>
        /// 任务发起描述
        /// </summary>
        public MissionNotificationInfo StartNotificationInfo { get; set; }

        /// <summary>
        /// 任务结束描述
        /// </summary>
        public MissionNotificationInfo EndNotificationInfo { get; set; }

        /// <summary>
        /// 任务字幕提示
        /// </summary>
        public string HintSubtitle { get; set; }
    }

    public struct MissionNotificationInfo
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public struct MissionRangeInfo
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }

#if CLIENT
        public bool IsPlayerIn
        {
            get => Position.DistanceToSquared(Game.PlayerPed.Position) <= Math.Pow(Radius, 2);
        }
#endif

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


#if SERVER
    public enum PedHash : uint
    {
        Michael = 225514697u,
        Franklin = 2602752943u,
        Trevor = 2608926626u,
        Abigail = 1074457665u,
        Agent = 3614493108u,
        Agent14 = 4227433577u,
        AmandaTownley = 1830688247u,
        Andreas = 1206185632u,
        Ashley = 2129936603u,
        AviSchwartzman = 939183526u,
        Ballasog = 2802535058u,
        Bankman = 2426248831u,
        Barry = 797459875u,
        Bestmen = 1464257942u,
        Beverly = 3181518428u,
        Brad = 3183167778u,
        Bride = 1633872967u,
        Car3Guy1 = 2230970679u,
        Car3Guy2 = 1975732938u,
        Casey = 3774489940u,
        Chef = 1240128502u,
        Chef2 = 2240322243u,
        Clay = 1825562762u,
        Claypain = 2634057640u,
        Cletus = 3865252245u,
        CrisFormage = 678319271u,
        Dale = 1182012905u,
        DaveNorton = 365775923u,
        Denise = 2181772221u,
        Devin = 1952555184u,
        DoaMan = 1646160893u,
        Dom = 2620240008u,
        Dreyfuss = 3666413874u,
        DrFriedlander = 3422293493u,
        EdToh = 712602007u,
        Fabien = 3499148112u,
        FbiSuit01 = 988062523u,
        Floyd = 2981205682u,
        G = 2216405299u,
        Groom = 4274948997u,
        Hao = 1704428387u,
        Hunter = 3457361118u,
        Janet = 225287241u,
        JayNorris = 2050158196u,
        Jewelass = 257763003u,
        JimmyBoston = 3986688045u,
        JimmyDisanto = 1459905209u,
        JoeMinuteman = 3189787803u,
        JohnnyKlebitz = 2278195374u,
        Josef = 3776618420u,
        Josh = 2040438510u,
        KarenDaniels = 3948009817u,
        KerryMcintosh = 1530648845u,
        LamarDavis = 1706635382u,
        Lazlow = 3756278757u,
        LesterCrest = 1302784073u,
        Lifeinvad01 = 1401530684u,
        Lifeinvad02 = 666718676u,
        Magenta = 4242313482u,
        Malc = 4055673113u,
        Manuel = 4248931856u,
        Marnie = 411185872u,
        MaryAnn = 2741999622u,
        Maude = 1005070462u,
        Michelle = 3214308084u,
        Milton = 3408943538u,
        Molly = 2936266209u,
        MrK = 3990661997u,
        MrsPhillips = 946007720u,
        MrsThornhill = 503621995u,
        Natalia = 3726105915u,
        NervousRon = 3170921201u,
        Nigel = 3367442045u,
        OldMan1a = 1906124788u,
        OldMan2 = 4011150407u,
        Omega = 1625728984u,
        ONeil = 768005095u,
        Orleans = 1641334641u,
        Ortega = 648372919u,
        Paper = 2577072326u,
        Patricia = 3312325004u,
        Popov = 645279998u,
        Paige = 357551935u,
        Priest = 1681385341u,
        PrologueDriver = 2237544099u,
        PrologueSec01 = 1888624839u,
        PrologueSec02 = 666086773u,
        RampGang = 3845001836u,
        RampHic = 1165307954u,
        RampHipster = 3740245870u,
        RampMex = 3870061732u,
        Rashkovsky = 940326374u,
        RoccoPelosi = 3585757951u,
        RussianDrunk = 1024089777u,
        ScreenWriter = 4293277303u,
        SiemonYetarian = 1283141381u,
        Solomon = 2260598310u,
        SteveHains = 941695432u,
        Stretch = 915948376u,
        Talina = 3885222120u,
        Tanisha = 226559113u,
        TaoCheng = 3697041061u,
        TaosTranslator = 2089096292u,
        TennisCoach = 2721800023u,
        Terry = 1728056212u,
        TomEpsilon = 3447159466u,
        Tonya = 3402126148u,
        TracyDisanto = 3728026165u,
        TrafficWarden = 1461287021u,
        TylerDixon = 1382414087u,
        VagosSpeak = 4194109068u,
        Wade = 2459507570u,
        WeiCheng = 2867128955u,
        Zimbor = 188012277u,
        AbigailCutscene = 2306246977u,
        AgentCutscene = 3614493108u,
        Agent14Cutscene = 1841036427u,
        AmandaTownleyCutscene = 2515474659u,
        AndreasCutscene = 3881194279u,
        AnitaCutscene = 117698822u,
        AntonCutscene = 2781317046u,
        AshleyCutscene = 650367097u,
        AviSchwartzmanCutscene = 2560490906u,
        BallasogCutscene = 2884567044u,
        BankmanCutscene = 2539657518u,
        BarryCutscene = 1767447799u,
        BeverlyCutscene = 3027157846u,
        BradCutscene = 4024807398u,
        BradCadaverCutscene = 1915268960u,
        BrideCutscene = 2193587873u,
        BurgerDrugCutscene = 2363277399u,
        Car3Guy1Cutscene = 71501447u,
        Car3Guy2Cutscene = 327394568u,
        CarBuyerCutscene = 2362341647u,
        CaseyCutscene = 3935738944u,
        ChefCutscene = 2739391114u,
        Chef2Cutscene = 2925257274u,
        ChinGoonCutscene = 2831296918u,
        ClayCutscene = 3687553076u,
        CletusCutscene = 3404326357u,
        CopCutscene = 2595446627u,
        CrisFormageCutscene = 3253960934u,
        CustomerCutscene = 2756669323u,
        DaleCutscene = 216536661u,
        DaveNortonCutscene = 2240226444u,
        DebraCutscene = 3973074921u,
        DeniseCutscene = 1870669624u,
        DeniseFriendCutscene = 3045926185u,
        DevinCutscene = 788622594u,
        DomCutscene = 1198698306u,
        DreyfussCutscene = 1012965715u,
        DrFriedlanderCutscene = 2745392175u,
        FabienCutscene = 1191403201u,
        FbiSuit01Cutscene = 1482427218u,
        FloydCutscene = 103106535u,
        FosRepCutscene = 466359675u,
        GCutscene = 2727244247u,
        GroomCutscene = 2058033618u,
        GroveStrDlrCutscene = 3898166818u,
        GuadalopeCutscene = 261428209u,
        GurkCutscene = 3272931111u,
        HaoCutscene = 3969814300u,
        HughCutscene = 1863555924u,
        HunterCutscene = 1531218220u,
        ImranCutscene = 3812756443u,
        JackHowitzerCutscene = 1153203121u,
        JanetCutscene = 808778210u,
        JanitorCutscene = 3254803008u,
        JewelassCutscene = 1145088004u,
        JimmyBostonCutscene = 60192701u,
        JimmyDisantoCutscene = 3100414644u,
        JoeMinutemanCutscene = 4036845097u,
        JohnnyKlebitzCutscene = 4203395201u,
        JosefCutscene = 1167549130u,
        JoshCutscene = 1158606749u,
        KarenDanielsCutscene = 1269774364u,
        LamarDavisCutscene = 1162230285u,
        LazlowCutscene = 949295643u,
        LesterCrestCutscene = 3046438339u,
        Lifeinvad01Cutscene = 1918178165u,
        MagentaCutscene = 1477887514u,
        ManuelCutscene = 4222842058u,
        MarnieCutscene = 1464721716u,
        MartinMadrazoCutscene = 1129928304u,
        MaryannCutscene = 161007533u,
        MaudeCutscene = 3166991819u,
        MerryWeatherCutscene = 1631478380u,
        MichelleCutscene = 1890499016u,
        MiltonCutscene = 3077190415u,
        MollyCutscene = 1167167044u,
        MoviePremFemaleCutscene = 1270514905u,
        MoviePremMaleCutscene = 2372398717u,
        MrKCutscene = 3284966005u,
        MrsPhillipsCutscene = 3422397391u,
        MrsThornhillCutscene = 1334976110u,
        NataliaCutscene = 1325314544u,
        NervousRonCutscene = 2023152276u,
        NigelCutscene = 3779566603u,
        OldMan1aCutscene = 518814684u,
        OldMan2Cutscene = 2566514544u,
        OmegaCutscene = 2339419141u,
        OrleansCutscene = 2905870170u,
        OrtegaCutscene = 3235579087u,
        OscarCutscene = 4095687067u,
        PaigeCutscene = 1528799427u,
        PaperCutscene = 1798879480u,
        PopovCutscene = 1635617250u,
        PatriciaCutscene = 3750433537u,
        PornDudesCutscene = 793443893u,
        PriestCutscene = 1299047806u,
        PrologueDriverCutscene = 4027271643u,
        PrologueSec01Cutscene = 2141384740u,
        PrologueSec02Cutscene = 512955554u,
        RampGangCutscene = 3263172030u,
        RampHicCutscene = 2240582840u,
        RampHipsterCutscene = 569740212u,
        RampMarineCutscene = 1634506681u,
        RampMexCutscene = 4132362192u,
        RashkovskyCutscene = 411081129u,
        ReporterCutscene = 776079908u,
        RoccoPelosiCutscene = 2858686092u,
        RussianDrunkCutscene = 1179785778u,
        ScreenWriterCutscene = 2346790124u,
        SiemonYetarianCutscene = 3230888450u,
        SolomonCutscene = 4140949582u,
        SteveHainsCutscene = 2766184958u,
        StretchCutscene = 2302502917u,
        Stripper01Cutscene = 2934601397u,
        Stripper02Cutscene = 2168724337u,
        TanishaCutscene = 1123963760u,
        TaoChengCutscene = 2288257085u,
        TaosTranslatorCutscene = 1397974313u,
        TennisCoachCutscene = 1545995274u,
        TerryCutscene = 978452933u,
        TomCutscene = 1776856003u,
        TomEpsilonCutscene = 2349847778u,
        TonyaCutscene = 1665391897u,
        TracyDisantoCutscene = 101298480u,
        TrafficWardenCutscene = 3727243251u,
        UndercoverCopCutscene = 4017642090u,
        VagosSpeakCutscene = 1224690857u,
        WadeCutscene = 3529955798u,
        WeiChengCutscene = 819699067u,
        ZimborCutscene = 3937184496u,
        Boar = 3462393972u,
        Cat = 1462895032u,
        ChickenHawk = 2864127842u,
        Chimp = 2825402133u,
        Chop = 351016938u,
        Cormorant = 1457690978u,
        Cow = 4244282910u,
        Coyote = 1682622302u,
        Crow = 402729631u,
        Deer = 3630914197u,
        Dolphin = 2344268885u,
        Fish = 802685111u,
        Hen = 1794449327u,
        HammerShark = 1015224100u,
        Humpback = 1193010354u,
        Husky = 1318032802u,
        KillerWhale = 2374682809u,
        MountainLion = 307287994u,
        Pig = 2971380566u,
        Pigeon = 111281960u,
        Poodle = 1125994524u,
        Pug = 1832265812u,
        Rabbit = 3753204865u,
        Rat = 3283429734u,
        Retriever = 882848737u,
        Rhesus = 3268439891u,
        Rottweiler = 2506301981u,
        Seagull = 3549666813u,
        Shepherd = 1126154828u,
        Stingray = 2705875277u,
        TigerShark = 113504370u,
        Westy = 2910340283u,
        Abner = 4037813798u,
        AlDiNapoli = 4042020578u,
        Antonb = 3479321132u,
        Armoured01 = 3455013896u,
        Babyd = 3658575486u,
        Bankman01 = 3272005365u,
        Baygor = 1380197501u,
        Benny = 3300333010u,
        BikeHire01 = 1984382277u,
        BikerChic = 4198014287u,
        BoatStaff01M = 3361671816u,
        BoatStaff01F = 848542878u,
        BurgerDrug = 2340239206u,
        Chip = 610290475u,
        Claude01 = 3237179831u,
        ClubHouseBar01 = 1914945105u,
        CocaineFemale01 = 1897303236u,
        CocaineMale01 = 3455927962u,
        ComJane = 3064628686u,
        Corpse01 = 773063444u,
        Corpse02 = 228356856u,
        CounterfeitFemale01 = 1074385436u,
        CounterfeitMale01 = 2625926338u,
        Cyclist01 = 755956971u,
        DeadHooker = 1943971979u,
        Drowned = 1943971979u,
        ExArmy01 = 1161072059u,
        ExecutivePAMale01 = 983887149u,
        ExecutivePAFemale01 = 2913175640u,
        Famdd01 = 866411749u,
        FibArchitect = 874722259u,
        FibMugger01 = 2243544680u,
        FibSec01 = 1558115333u,
        FilmDirector = 728636342u,
        FilmNoir = 732742363u,
        Finguru01 = 1189322339u,
        ForgeryFemale01 = 3691903615u,
        ForgeryMale01 = 325317957u,
        FreemodeFemale01 = 2627665880u,
        FreemodeMale01 = 1885233650u,
        Glenstank01 = 1169888870u,
        Griff01 = 3293887675u,
        Guido01 = 3333724719u,
        GunVend01 = 3005388626u,
        Hacker = 2579169528u,
        HeliStaff01 = 431423238u,
        Hippie01 = 4030826507u,
        Hotposh01 = 2526768638u,
        Imporage = 880829941u,
        Jesus01 = 3459037009u,
        Jewelass01 = 4040474158u,
        JewelSec01 = 2899099062u,
        JewelThief = 3872144604u,
        Justin = 2109968527u,
        Mani = 3367706194u,
        Markfost = 479578891u,
        Marston01 = 943915367u,
        MethFemale01 = 3778572496u,
        MethMale01 = 1293671805u,
        MilitaryBum = 1191548746u,
        Miranda = 1095737979u,
        Mistress = 1573528872u,
        Misty01 = 3509125021u,
        MovieStar = 894928436u,
        MPros01 = 1822283721u,
        Niko01 = 4007317449u,
        Paparazzi = 1346941736u,
        Party01 = 921110016u,
        PartyTarget = 2180468199u,
        PestContDriver = 994527967u,
        PestContGunman = 193469166u,
        Pogo01 = 3696858125u,
        Poppymich = 602513566u,
        Princess = 3538133636u,
        Prisoner01 = 2073775040u,
        PrologueHostage01 = 3306347811u,
        PrologueMournFemale01 = 2718472679u,
        PrologueMournMale01 = 3465937675u,
        RivalPaparazzi = 1624626906u,
        ShopKeep01 = 416176080u,
        SpyActor = 2886641112u,
        SpyActress = 1535236204u,
        StripperLite = 695248020u,
        Taphillbilly = 2585681490u,
        Tramp01 = 1787764635u,
        VagosFun01 = 3299219389u,
        WillyFist = 2423691919u,
        WeedFemale01 = 1596374223u,
        WeedMale01 = 2648833641u,
        Zombie01 = 2890614022u,
        Acult01AMM = 1413662315u,
        Acult01AMO = 1430544400u,
        Acult01AMY = 3043264555u,
        Acult02AMO = 1268862154u,
        Acult02AMY = 2162532142u,
        AfriAmer01AMM = 3513928062u,
        Airhostess01SFY = 1567728751u,
        AirworkerSMY = 1644266841u,
        Ammucity01SMY = 2651349821u,
        AmmuCountrySMM = 233415434u,
        ArmBoss01GMM = 4058522530u,
        ArmGoon01GMM = 4255728232u,
        ArmGoon02GMY = 3310258058u,
        ArmLieut01GMM = 3882958867u,
        Armoured01SMM = 2512875213u,
        Armoured02SMM = 1669696074u,
        Armymech01SMY = 1657546978u,
        Autopsy01SMY = 2988916046u,
        Autoshop01SMM = 68070371u,
        Autoshop02SMM = 4033578141u,
        Azteca01GMY = 1752208920u,
        BallaEast01GMY = 4096714883u,
        BallaOrig01GMY = 588969535u,
        Ballas01GFY = 361513884u,
        BallaSout01GMY = 599294057u,
        Barman01SMY = 3852538118u,
        Bartender01SFY = 2014052797u,
        Baywatch01SFY = 1250841910u,
        Baywatch01SMY = 189425762u,
        Beach01AFM = 808859815u,
        Beach01AFY = 3349113128u,
        Beach01AMM = 1077785853u,
        Beach01AMO = 2217202584u,
        Beach01AMY = 3523131524u,
        Beach02AMM = 2021631368u,
        Beach02AMY = 600300561u,
        Beach03AMY = 3886638041u,
        Beachvesp01AMY = 2114544056u,
        Beachvesp02AMY = 3394697810u,
        Bevhills01AFM = 3188223741u,
        Bevhills01AFY = 1146800212u,
        Bevhills01AMM = 1423699487u,
        Bevhills01AMY = 1982350912u,
        Bevhills02AFM = 2688103263u,
        Bevhills02AFY = 1546450936u,
        Bevhills02AMM = 1068876755u,
        Bevhills02AMY = 1720428295u,
        Bevhills03AFY = 549978415u,
        Bevhills04AFY = 920595805u,
        Blackops01SMY = 3019107892u,
        Blackops02SMY = 2047212121u,
        Blackops03SMY = 1349953339u,
        Bodybuild01AFM = 1004114196u,
        Bouncer01SMM = 2681481517u,
        Breakdance01AMY = 933205398u,
        Busboy01SMY = 3640249671u,
        Busicas01AMY = 2597531625u,
        Business01AFY = 664399832u,
        Business01AMM = 2120901815u,
        Business01AMY = 3382649284u,
        Business02AFM = 532905404u,
        Business02AFY = 826475330u,
        Business02AMY = 3014915558u,
        Business03AFY = 2928082356u,
        Business03AMY = 2705543429u,
        Business04AFY = 3083210802u,
        Busker01SMO = 2912874939u,
        CCrew01SMM = 3387290987u,
        Chef01SMY = 261586155u,
        ChemSec01SMM = 788443093u,
        ChemWork01GMM = 4128603535u,
        ChiBoss01GMM = 3118269184u,
        ChiCold01GMM = 275618457u,
        ChiGoon01GMM = 2119136831u,
        ChiGoon02GMM = 4285659174u,
        CiaSec01SMM = 1650288984u,
        Clown01SMY = 71929310u,
        Cntrybar01SMM = 436345731u,
        Construct01SMY = 3621428889u,
        Construct02SMY = 3321821918u,
        Cop01SFY = 368603149u,
        Cop01SMY = 1581098148u,
        Cyclist01AMY = 4257633223u,
        Dealer01SMY = 3835149295u,
        Devinsec01SMY = 2606068340u,
        Dhill01AMY = 4282288299u,
        Dockwork01SMM = 349680864u,
        Dockwork01SMY = 2255894993u,
        Doctor01SMM = 3564307372u,
        Doorman01SMY = 579932932u,
        Downtown01AFM = 1699403886u,
        Downtown01AMY = 766375082u,
        DwService01SMY = 1976765073u,
        DwService02SMY = 4119890438u,
        Eastsa01AFM = 2638072698u,
        Eastsa01AFY = 4121954205u,
        Eastsa01AMM = 4188468543u,
        Eastsa01AMY = 2756120947u,
        Eastsa02AFM = 1674107025u,
        Eastsa02AFY = 70821038u,
        Eastsa02AMM = 131961260u,
        Eastsa02AMY = 377976310u,
        Eastsa03AFY = 1371553700u,
        Epsilon01AFY = 1755064960u,
        Epsilon01AMY = 2010389054u,
        Epsilon02AMY = 2860711835u,
        Factory01SFY = 1777626099u,
        Factory01SMY = 1097048408u,
        Famca01GMY = 3896218551u,
        Famdnf01GMY = 3681718840u,
        Famfor01GMY = 2217749257u,
        Families01GFY = 1309468115u,
        Farmer01AMM = 2488675799u,
        FatBla01AFM = 4206136267u,
        FatCult01AFM = 3050275044u,
        Fatlatin01AMM = 1641152947u,
        FatWhite01AFM = 951767867u,
        FemBarberSFM = 373000027u,
        FibOffice01SMM = 3988550982u,
        FibOffice02SMM = 653289389u,
        FibSec01SMM = 2072724299u,
        Fireman01SMY = 3065114024u,
        Fitness01AFY = 1165780219u,
        Fitness02AFY = 331645324u,
        Gaffer01SMM = 2841034142u,
        GarbageSMY = 4000686095u,
        Gardener01SMM = 1240094341u,
        Gay01AMY = 3519864886u,
        Gay02AMY = 2775713665u,
        Genfat01AMM = 115168927u,
        Genfat02AMM = 330231874u,
        Genhot01AFY = 793439294u,
        Genstreet01AFO = 1640504453u,
        Genstreet01AMO = 2908022696u,
        Genstreet01AMY = 2557996913u,
        Genstreet02AMY = 891398354u,
        GentransportSMM = 411102470u,
        Golfer01AFY = 2111372120u,
        Golfer01AMM = 2850754114u,
        Golfer01AMY = 3609190705u,
        Grip01SMY = 815693290u,
        Hairdress01SMM = 1099825042u,
        Hasjew01AMM = 1809430156u,
        Hasjew01AMY = 3782053633u,
        Highsec01SMM = 4049719826u,
        Highsec02SMM = 691061163u,
        Hiker01AFY = 813893651u,
        Hiker01AMY = 1358380044u,
        Hillbilly01AMM = 1822107721u,
        Hillbilly02AMM = 2064532783u,
        Hippie01AFY = 343259175u,
        Hippy01AMY = 2097407511u,
        Hipster01AFY = 2185745201u,
        Hipster01AMY = 587703123u,
        Hipster02AFY = 2549481101u,
        Hipster02AMY = 349505262u,
        Hipster03AFY = 2780469782u,
        Hipster03AMY = 1312913862u,
        Hipster04AFY = 429425116u,
        Hooker01SFY = 42647445u,
        Hooker02SFY = 348382215u,
        Hooker03SFY = 51789996u,
        Hwaycop01SMY = 1939545845u,
        Indian01AFO = 3134700416u,
        Indian01AFY = 153984193u,
        Indian01AMM = 3721046572u,
        Indian01AMY = 706935758u,
        JanitorSMM = 2842417644u,
        Jetski01AMY = 767028979u,
        Juggalo01AFY = 3675473203u,
        Juggalo01AMY = 2445950508u,
        KorBoss01GMM = 891945583u,
        Korean01GMY = 611648169u,
        Korean02GMY = 2414729609u,
        KorLieut01GMY = 2093736314u,
        Ktown01AFM = 1388848350u,
        Ktown01AFO = 1204772502u,
        Ktown01AMM = 3512565361u,
        Ktown01AMO = 355916122u,
        Ktown01AMY = 452351020u,
        Ktown02AFM = 1090617681u,
        Ktown02AMY = 696250687u,
        Lathandy01SMM = 2659242702u,
        Latino01AMY = 321657486u,
        Lifeinvad01SMM = 3724572669u,
        LinecookSMM = 3684436375u,
        Lost01GFY = 4250220510u,
        Lost01GMY = 1330042375u,
        Lost02GMY = 1032073858u,
        Lost03GMY = 850468060u,
        Lsmetro01SMM = 1985653476u,
        Maid01SFM = 3767780806u,
        Malibu01AMM = 803106487u,
        Mariachi01SMM = 2124742566u,
        Marine01SMM = 4074414829u,
        Marine01SMY = 1702441027u,
        Marine02SMM = 4028996995u,
        Marine02SMY = 1490458366u,
        Marine03SMY = 1925237458u,
        Methhead01AMY = 1768677545u,
        MexBoss01GMM = 1466037421u,
        MexBoss02GMM = 1226102803u,
        MexCntry01AMM = 3716251309u,
        MexGang01GMY = 3185399110u,
        MexGoon01GMY = 653210662u,
        MexGoon02GMY = 832784782u,
        MexGoon03GMY = 2521633500u,
        MexLabor01AMM = 2992445106u,
        MexThug01AMY = 810804565u,
        Migrant01SFY = 3579522037u,
        Migrant01SMM = 3977045190u,
        MimeSMY = 1021093698u,
        Motox01AMY = 1694362237u,
        Motox02AMY = 2007797722u,
        MovAlien01 = 1684083350u,
        MovPrem01SFY = 587253782u,
        Movprem01SMM = 3630066984u,
        Movspace01SMM = 3887273010u,
        Musclbeac01AMY = 1264920838u,
        Musclbeac02AMY = 3374523516u,
        OgBoss01AMM = 1746653202u,
        Paparazzi01AMM = 3972697109u,
        Paramedic01SMM = 3008586398u,
        PestCont01SMY = 1209091352u,
        Pilot01SMM = 3881519900u,
        Pilot01SMY = 2872052743u,
        Pilot02SMM = 4131252449u,
        PoloGoon01GMY = 1329576454u,
        PoloGoon02GMY = 2733138262u,
        Polynesian01AMM = 2849617566u,
        Polynesian01AMY = 2206530719u,
        Postal01SMM = 1650036788u,
        Postal02SMM = 1936142927u,
        Prisguard01SMM = 1456041926u,
        PrisMuscl01SMY = 1596003233u,
        Prisoner01SMY = 2981862233u,
        PrologueHostage01AFM = 379310561u,
        PrologueHostage01AMM = 2534589327u,
        Ranger01SFY = 2680682039u,
        Ranger01SMY = 4017173934u,
        Roadcyc01AMY = 4116817094u,
        Robber01SMY = 3227390873u,
        RsRanger01AMO = 1011059922u,
        Runner01AFY = 3343476521u,
        Runner01AMY = 623927022u,
        Runner02AMY = 2218630415u,
        Rurmeth01AFY = 1064866854u,
        Rurmeth01AMM = 1001210244u,
        Salton01AFM = 3725461865u,
        Salton01AFO = 3439295882u,
        Salton01AMM = 1328415626u,
        Salton01AMO = 539004493u,
        Salton01AMY = 3613420592u,
        Salton02AMM = 1626646295u,
        Salton03AMM = 2995538501u,
        Salton04AMM = 2521108919u,
        SalvaBoss01GMY = 2422005962u,
        SalvaGoon01GMY = 663522487u,
        SalvaGoon02GMY = 846439045u,
        SalvaGoon03GMY = 62440720u,
        SbikeAMO = 1794381917u,
        Scdressy01AFY = 3680420864u,
        Scientist01SMM = 1092080539u,
        Scrubs01SFY = 2874755766u,
        Security01SMM = 3613962792u,
        Sheriff01SFY = 1096929346u,
        Sheriff01SMY = 2974087609u,
        ShopHighSFM = 2923947184u,
        ShopLowSFY = 2842568196u,
        ShopMaskSMY = 1846684678u,
        ShopMidSFY = 1055701597u,
        Skater01AFY = 1767892582u,
        Skater01AMM = 3654768780u,
        Skater01AMY = 3250873975u,
        Skater02AMY = 2952446692u,
        Skidrow01AFM = 2962707003u,
        Skidrow01AMM = 32417469u,
        Snowcop01SMM = 451459928u,
        Socenlat01AMM = 193817059u,
        Soucent01AFM = 1951946145u,
        Soucent01AFO = 1039800368u,
        Soucent01AFY = 744758650u,
        Soucent01AMM = 1750583735u,
        Soucent01AMO = 718836251u,
        Soucent01AMY = 3877027275u,
        Soucent02AFM = 4079145784u,
        Soucent02AFO = 2775443222u,
        Soucent02AFY = 1519319503u,
        Soucent02AMM = 2674735073u,
        Soucent02AMO = 1082572151u,
        Soucent02AMY = 2896414922u,
        Soucent03AFY = 2276611093u,
        Soucent03AMM = 2346291386u,
        Soucent03AMO = 238213328u,
        Soucent03AMY = 3287349092u,
        Soucent04AMM = 3271294718u,
        Soucent04AMY = 2318861297u,
        Soucentmc01AFM = 3454621138u,
        Staggrm01AMO = 2442448387u,
        Stbla01AMY = 3482496489u,
        Stbla02AMY = 2563194959u,
        Stlat01AMY = 2255803900u,
        Stlat02AMM = 3265820418u,
        Stripper01SFY = 1381498905u,
        Stripper02SFY = 1846523796u,
        StripperLiteSFY = 1544875514u,
        Strperf01SMM = 2035992488u,
        Strpreach01SMM = 469792763u,
        StrPunk01GMY = 4246489531u,
        StrPunk02GMY = 228715206u,
        Strvend01SMM = 3465614249u,
        Strvend01SMY = 2457805603u,
        Stwhi01AMY = 605602864u,
        Stwhi02AMY = 919005580u,
        Sunbathe01AMY = 3072929548u,
        Surfer01AMY = 3938633710u,
        Swat01SMY = 2374966032u,
        Sweatshop01SFM = 824925120u,
        Sweatshop01SFY = 2231547570u,
        Tattoo01AMO = 2494442380u,
        Tennis01AFY = 1426880966u,
        Tennis01AMM = 1416254276u,
        Topless01AFY = 2633130371u,
        Tourist01AFM = 1347814329u,
        Tourist01AFY = 1446741360u,
        Tourist01AMM = 3365863812u,
        Tourist02AFY = 2435054400u,
        Tramp01AFM = 1224306523u,
        Tramp01AMM = 516505552u,
        Tramp01AMO = 390939205u,
        TrampBeac01AFM = 2359345766u,
        TrampBeac01AMM = 1404403376u,
        Tranvest01AMM = 3773208948u,
        Tranvest02AMM = 4144940484u,
        Trucker01SMM = 1498487404u,
        Ups01SMM = 2680389410u,
        Ups02SMM = 3502104854u,
        Uscg01SMY = 3389018345u,
        Vagos01GFY = 1520708641u,
        Valet01SMY = 999748158u,
        Vindouche01AMY = 3247667175u,
        Vinewood01AFY = 435429221u,
        Vinewood01AMY = 1264851357u,
        Vinewood02AFY = 3669401835u,
        Vinewood02AMY = 1561705728u,
        Vinewood03AFY = 933092024u,
        Vinewood03AMY = 534725268u,
        Vinewood04AFY = 4209271110u,
        Vinewood04AMY = 835315305u,
        Waiter01SMY = 2907468364u,
        WinClean01SMY = 1426951581u,
        Xmech01SMY = 1142162924u,
        Xmech02SMY = 3189832196u,
        Xmech02SMYMP = 1755203590u,
        Yoga01AFY = 3290105390u,
        Yoga01AMY = 2869588309u
    }

    public enum WeaponHash : uint
    {
        Knife = 2578778090u,
        Nightstick = 1737195953u,
        Hammer = 1317494643u,
        Bat = 2508868239u,
        GolfClub = 1141786504u,
        Crowbar = 2227010557u,
        Bottle = 4192643659u,
        SwitchBlade = 3756226112u,
        Pistol = 453432689u,
        CombatPistol = 1593441988u,
        APPistol = 584646201u,
        Pistol50 = 2578377531u,
        FlareGun = 1198879012u,
        MarksmanPistol = 3696079510u,
        Revolver = 3249783761u,
        MicroSMG = 324215364u,
        SMG = 736523883u,
        AssaultSMG = 4024951519u,
        CombatPDW = 171789620u,
        AssaultRifle = 3220176749u,
        CarbineRifle = 2210333304u,
        AdvancedRifle = 2937143193u,
        CompactRifle = 1649403952u,
        MG = 2634544996u,
        CombatMG = 2144741730u,
        PumpShotgun = 487013001u,
        SawnOffShotgun = 2017895192u,
        AssaultShotgun = 3800352039u,
        BullpupShotgun = 2640438543u,
        DoubleBarrelShotgun = 4019527611u,
        StunGun = 911657153u,
        SniperRifle = 100416529u,
        HeavySniper = 205991906u,
        GrenadeLauncher = 2726580491u,
        GrenadeLauncherSmoke = 1305664598u,
        RPG = 2982836145u,
        Minigun = 1119849093u,
        Grenade = 2481070269u,
        StickyBomb = 741814745u,
        SmokeGrenade = 4256991824u,
        BZGas = 2694266206u,
        Molotov = 615608432u,
        FireExtinguisher = 101631238u,
        PetrolCan = 883325847u,
        SNSPistol = 3218215474u,
        SpecialCarbine = 3231910285u,
        HeavyPistol = 3523564046u,
        BullpupRifle = 2132975508u,
        HomingLauncher = 1672152130u,
        ProximityMine = 2874559379u,
        Snowball = 126349499u,
        VintagePistol = 137902532u,
        Dagger = 2460120199u,
        Firework = 2138347493u,
        Musket = 2828843422u,
        MarksmanRifle = 3342088282u,
        HeavyShotgun = 984333226u,
        Gusenberg = 1627465347u,
        Hatchet = 4191993645u,
        Railgun = 1834241177u,
        Unarmed = 2725352035u,
        KnuckleDuster = 3638508604u,
        Machete = 3713923289u,
        MachinePistol = 3675956304u,
        Flashlight = 2343591895u,
        Ball = 600439132u,
        Flare = 1233104067u,
        NightVision = 2803906140u,
        Parachute = 4222310262u,
        SweeperShotgun = 317205821u,
        BattleAxe = 3441901897u,
        CompactGrenadeLauncher = 125959754u,
        MiniSMG = 3173288789u,
        PipeBomb = 3125143736u,
        PoolCue = 2484171525u,
        Wrench = 419712736u,
        PistolMk2 = 3219281620u,
        AssaultRifleMk2 = 961495388u,
        CarbineRifleMk2 = 4208062921u,
        CombatMGMk2 = 3686625920u,
        HeavySniperMk2 = 177293209u,
        SMGMk2 = 2024373456u
    }

#endif

}
