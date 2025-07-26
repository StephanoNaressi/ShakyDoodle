using Avalonia.Controls;
using System.Collections.Generic;

namespace ShakyDoodle.Utils
{
    public class UIManager
    {
        private readonly Dictionary<string, Button[]> _buttonGroups = new Dictionary<string, Button[]>();

        public void RegisterButtonGroup(string groupName, Button[] buttons)
        {
            _buttonGroups[groupName] = buttons;
        }

        public void UpdateSelection(string groupName, Button selectedButton)
        {
            if (!_buttonGroups.ContainsKey(groupName)) return;

            foreach (var button in _buttonGroups[groupName])
            {
                if (button != null)
                {
                    button.Opacity = 0.5;
                }
            }

            if (selectedButton != null)
            {
                selectedButton.Opacity = 1.0;
            }
        }

        public void UpdateButtonsState(Button[] buttons, bool isEnabled)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.IsEnabled = isEnabled;
                }
            }
        }
    }
}