using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace LogicLink;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("ZeepSDK")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony;

    ConfigEntry<KeyboardShortcut> ChangeMoveMode;
    ConfigEntry<KeyboardShortcut> ToggleTriggerPlane;


    private void Awake()
    {
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        ChangeMoveMode = Config.Bind("Settings", "Change Move Mode", KeyboardShortcut.Empty, "Press to change the move mode");
        ToggleTriggerPlane = Config.Bind("Settings", "Toggle Trigger Plane", KeyboardShortcut.Empty, "Press to toggle a plane to visualize the available movement of a trigger");

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Update()
    {
        if (ToggleTriggerPlane.Value.IsDown())
        {

        }
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
        harmony = null;
    }
}
