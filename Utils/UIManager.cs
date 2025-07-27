using Avalonia.Controls;
using System.Collections.Generic;

namespace ShakyDoodle.Utils
{
    public class UIManager
    {
        private readonly Dictionary<string, Button[]> _buttonGroups = new Dictionary<string, Button[]>();
        private readonly Dictionary<string, MenuItem[]> _menuItemGroups = new Dictionary<string, MenuItem[]>();

        public void RegisterButtonGroup(string groupName, Button[] buttons)
        {
            _buttonGroups[groupName] = buttons;
        }

        public void RegisterMenuItemGroup(string groupName, MenuItem[] menuItems)
        {
            _menuItemGroups[groupName] = menuItems;
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

        public void UpdateMenuSelection(string groupName, MenuItem selectedMenuItem)
        {
            if (!_menuItemGroups.ContainsKey(groupName)) return;

            foreach (var menuItem in _menuItemGroups[groupName])
            {
                if (menuItem != null)
                {
                    menuItem.Opacity = 0.5;
                }
            }

            if (selectedMenuItem != null)
            {
                selectedMenuItem.Opacity = 1.0;
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