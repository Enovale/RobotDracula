using ChoiceEvent;
using Dungeon.Mirror;
using MainUI;
using RobotDracula.Battle.Automation;
using RobotDracula.ChoiceEvent.Automation;
using RobotDracula.Dungeon.Automation;
using RobotDracula.General;

namespace RobotDracula.Trainer
{
    public static partial class TrainerManager
    {
        static TrainerManager()
        {
            BattleUpdate += BattleAutomation.HandleBattleAutomation;
            MirrorDungeonMapUpdate += DungeonAutomation.HandleDungeonAutomation;
            MirrorDungeonEventChoiceUpdate += ChoiceEventAutomation.HandleChoiceEventAutomation;
            MirrorDungeonShopUpdate += ShopAutomation.HandleShopAutomation;
            FormationUpdate += DungeonAutomation.HandleFormationAutomation;
            LevelUpUpdate += DungeonAutomation.HandleLevelUpAutomation;
            NewCharacterUpdate += DungeonAutomation.HandleNewCharacterAutomation;
            EgoGiftUpdate += DungeonAutomation.HandleEgoGiftAutomation;
            EgoGiftPopupUpdate += DungeonAutomation.HandleCloseAnnoyingEgoGiftPopup;
            TrainerUpdate += GeneralAutomation.HandleTimescaleUpdate;
        }

        public delegate void BattleUpdateEventHandler();

        public static event BattleUpdateEventHandler BattleUpdate;

        public delegate void MirrorLevelUpRewardEventHandler();

        public static event MirrorLevelUpRewardEventHandler LevelUpUpdate;

        public delegate void MirrorNewCharacterRewardEventHandler(RandomDungeonAcquireCharacterPanel panel);

        public static event MirrorNewCharacterRewardEventHandler NewCharacterUpdate;

        public delegate void MirrorEgoGiftRewardEventHandler(SelectEgoGiftPanel panel);

        public static event MirrorEgoGiftRewardEventHandler EgoGiftUpdate;

        public delegate void MirrorEgoGiftPopupEventHandler();

        public static event MirrorEgoGiftPopupEventHandler EgoGiftPopupUpdate;

        public delegate void MirrorFormationEventHandler();

        public static event MirrorFormationEventHandler FormationUpdate;

        public delegate void MirrorDungeonEventChoiceEventHandler(ChoiceEventController controller);

        public static event MirrorDungeonEventChoiceEventHandler MirrorDungeonEventChoiceUpdate;

        public delegate void MirrorDungeonShopEventHandler(MirrorDungeonShopUIController controller);

        public static event MirrorDungeonShopEventHandler MirrorDungeonShopUpdate;

        public delegate void MirrorDungeonMapEventHandler();

        public static event MirrorDungeonMapEventHandler MirrorDungeonMapUpdate;
        
        public delegate void TrainerUpdateEventHandler();
        
        public static event TrainerUpdateEventHandler TrainerUpdate;
    }
}