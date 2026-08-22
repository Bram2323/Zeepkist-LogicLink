using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using ZeepSDK.Settings;

namespace LogicLink.Settings
{
    public static class SettingsManager
    {
        private static Dictionary<(string, string), object> ConfigEntries = [];
        private static Dictionary<string, string> Tabs = [];
        private static bool SkipConfigureTabs = true;

        private static Queue<ISetting> SettingsQueue = [];

        public static bool IsReady()
        {
            return Plugin.PluginLoaded && Plugin.Instance != null && Plugin.Instance.Config != null;
        }

        public static void QueueSetting(ISetting setting)
        {
            SettingsQueue.Enqueue(setting);
        }

        public static void HandleBacklog()
        {
            if (!IsReady()) return;

            SkipConfigureTabs = true;
            while (SettingsQueue.Count > 0)
            {
                ISetting setting = SettingsQueue.Dequeue();
                setting.Bind();
            }
            SkipConfigureTabs = false;
            ConfigureTabs();
        }

        public static ConfigEntry<T> CreateConfig<T>(string section, string key, T defaultValue, string description, string tab, bool hidden)
        {
            if (ConfigEntries.ContainsKey((section, key))) return (ConfigEntry<T>)ConfigEntries[(section, key)];

            if (hidden) description = "[hide] " + description;
            ConfigEntry<T> entry = Plugin.Instance.Config.Bind(section, key, defaultValue, description);
            ConfigEntries[(section, key)] = entry;

            if (!Tabs.ContainsKey(section))
            {
                Tabs[section] = tab;
                if (!SkipConfigureTabs) ConfigureTabs();
            }

            return entry;
        }

        public static void ConfigureTabs()
        {
            // Disabled until ConfigureModSettingsTabs is fixed
            return;

            Dictionary<string, List<string>> tabContents = [];
            SettingsApi.ClearModSettingsTabs(Plugin.Instance);

            foreach (KeyValuePair<string, string> section in Tabs)
            {
                if (!tabContents.ContainsKey(section.Value)) tabContents[section.Value] = [];
                tabContents[section.Value].Add(section.Key);
            }

            SettingsApi.ConfigureModSettingsTabs(Plugin.Instance, (tabs) =>
            {
                foreach (KeyValuePair<string, List<string>> tab in tabContents)
                {
                    string tabLabel = tab.Key;
                    List<string> sections = tab.Value;

                    tabs.Tab(tabLabel, [.. sections]);
                }
            });
        }
    }
}
