using TreeRoutine.DefaultBehaviors.Helpers;
using System.Windows.Forms;
using TreeRoutine.TreeSharp;

namespace TreeRoutine.DefaultBehaviors.Actions
{
    public delegate Keys? HotkeyDelegate(object context);


    public class UseHotkeyAction : TreeSharp.Action
    {
        private KeyboardHelper KeyboardHelper { get; set; }
        private HotkeyDelegate Hotkey { get; set; }

        public UseHotkeyAction(KeyboardHelper helper, HotkeyDelegate hotkey) : base()
        {
            KeyboardHelper = helper;
            Hotkey = hotkey;
        }

        public UseHotkeyAction(KeyboardHelper helper, ActionDelegate action, HotkeyDelegate hotkey) : base(action)
        {
            KeyboardHelper = helper;
            Hotkey = hotkey;
        }

        public UseHotkeyAction(KeyboardHelper helper, ActionSucceedDelegate action, HotkeyDelegate hotkey) : base(action)
        {
            KeyboardHelper = helper;
            Hotkey = hotkey;
        }

        protected override RunStatus Run(object context)
        {
            Keys? key = Hotkey(context);
            if (key == null)
            {
                return RunStatus.Failure;
            }

            if (!KeyboardHelper.KeyPressRelease((Keys)key))
            {
                return RunStatus.Failure;
            }

            return RunStatus.Success;
        }

    }
}
