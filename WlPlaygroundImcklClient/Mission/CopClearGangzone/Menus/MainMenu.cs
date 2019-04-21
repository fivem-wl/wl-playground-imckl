using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using MenuAPI;


namespace WlPlaygroundImcklClient.Mission.CopClearGangzone.Menus
{

    public abstract class SubMenu
    {
        protected MainMenu RootMenu;
        protected Menu Menu;

        public Menu GetMenu()
        {
            if (Menu == null)
            {
                CreateMenu();
            }
            return Menu;
        }

        protected SubMenu(MainMenu rootMenu)
        {
            this.RootMenu = rootMenu;
        }

        protected abstract void CreateMenu();

    }

    public class MissionMenu : SubMenu
    {

        public MissionMenu(MainMenu rootMenu)
            : base(rootMenu)
        { }

        protected override void CreateMenu()
        {
            Menu = new Menu("警察系统", "警民合作, 共建和谐社会");

            MenuItem areaRiotMissionMenuBtn = new MenuItem("打击区域暴乱活动", "打击可疑的区域暴乱活动");

            Menu.AddMenuItem(areaRiotMissionMenuBtn);

            Menu.OnItemSelect += (_menu, _item, _index) =>
            {
                if (_item == areaRiotMissionMenuBtn)
                {
                    Debug.WriteLine("try create mission");
                    BaseScript.TriggerEvent($"{CopClearGangzone.ResourceName}:CreateMission");
                }
            };
        }
    }

    public class MainMenu
    {
        private Menu Menu;
        public MissionMenu MissionMenu { get; set; }

        public Menu GetMenu()
        {
            if (Menu == null)
            {
                CreateMenu();
            }
            return Menu;
        }

        public void CreateMenu()
        {
            // 菜单靠右
            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Right;

            // 菜单叫出逻辑交由其它处理
            MenuController.EnableMenuToggleKeyOnController = false;

            // 主菜单
            Menu = new Menu("警民合作系统", "警民合作, 共建和谐社会");
            MissionMenu = new MissionMenu(this);

            MenuController.AddMenu(Menu);
            MenuController.MainMenu = Menu;

            // 菜单按钮/下级菜单
            MenuItem missionMenuBtn = new MenuItem("打击犯罪", "查看当前可疑的犯罪活动");

            Menu.AddMenuItem(missionMenuBtn);

            MenuController.AddSubmenu(this.Menu, MissionMenu.GetMenu());
            MenuController.BindMenuItem(this.Menu, MissionMenu.GetMenu(), missionMenuBtn);

            Menu.OnMenuOpen += (_menu) =>
            {

            };
            
            Menu.OnItemSelect += (_menu, _item, _index) =>
            {
                
            };

        }


    }
}
