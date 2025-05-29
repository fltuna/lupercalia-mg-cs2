using CounterStrikeSharp.API;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.Utils
{
    /// <summary>
    /// Player's button state.
    /// </summary>
    public class ButtonState
    {
        private PlayerButtons _PrevButtons = 0;
        private PlayerButtons _CurrentButtons = 0;
        private PlayerButtons _ChangedButtons = 0;

        public void Update(PlayerButtons buttons)
        {
            _PrevButtons = _CurrentButtons;
            _CurrentButtons = buttons;
            _ChangedButtons = _PrevButtons ^ _CurrentButtons;
        }

        private PlayerButtons _Down(PlayerButtons mask)
        {
            return _CurrentButtons & mask;
        }

        private PlayerButtons _Changed(PlayerButtons mask)
        {
            return _ChangedButtons & mask;
        }

        /// <summary>
        /// Returns if the button is down.
        /// </summary>
        public bool IsDown(PlayerButtons mask)
        {
            return _Down(mask) != 0;
        }

        /// <summary>
        /// Returns if the button is pressed at the current tick.
        /// </summary>
        public bool IsPressed(PlayerButtons mask)
        {
            return (_Changed(mask) & _Down(mask)) != 0;
        }
    }
}
