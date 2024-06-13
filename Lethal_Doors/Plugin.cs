using BepInEx;
using HarmonyLib;
using Lethal_Doors.Patches;

namespace LethalDoors
{
    [BepInPlugin(GUID, NAME, VERSION)]
	public class LethalDoors : BaseUnityPlugin
	{
        const string GUID = "Entity378.LethalDoorsFixed";
        const string NAME = "Lethal Doors Fixed";
        const string VERSION = "1.0.10";

        public static LethalDoors Instance { get; private set; }
        private readonly Harmony harmony = new Harmony(GUID);

        private void Awake()
		{
			if (Instance == null)
			{
                Instance = this;
			}
			harmony.PatchAll(typeof(DoorInteractionPatch));
			Logger.LogInfo("Lethal Doors Fixed Loaded");
		}
	}
}
