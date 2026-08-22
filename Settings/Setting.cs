using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLink.Settings
{
    public class Setting<T> : ISetting
    {
        private string Section;
        private string Key;
        private T DefaultValue;
        private string Description;
        private string Tab;
        private bool Hidden;

        private ConfigEntry<T> ConfigEntry;
        public T Value
        {
            get
            {
                return ConfigEntry.Value;
            }
            set
            {
                ConfigEntry.Value = value;
            }
        }

        public event EventHandler SettingChanged;

        public Setting(string section, string key, T defaultValue, string description, bool hidden = false) : this(section, key, defaultValue, description, "LogicV2", hidden) { }

        public Setting(string section, string key, T defaultValue, string description, string tab, bool hidden = false)
        {
            Section = section;
            Key = key;
            DefaultValue = defaultValue;
            Description = description;
            Tab = tab;
            Hidden = hidden;

            Bind();
        }

        public void Bind()
        {
            if (!SettingsManager.IsReady())
            {
                SettingsManager.QueueSetting(this);
                return;
            }

            ConfigEntry = SettingsManager.CreateConfig(Section, Key, DefaultValue, Description, Tab, Hidden);
            ConfigEntry.SettingChanged += SettingChanged;
        }



        public static implicit operator T(Setting<T> setting) => setting.Value;

        public override string ToString() => Value.ToString();
    }
}
