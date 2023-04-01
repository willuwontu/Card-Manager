using BepInEx;
using HarmonyLib;

namespace R3DCore
{
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("ROUNDS 3D.exe")]
    internal class CMBL : BaseUnityPlugin
    {
        private const string ModId = "com.willuwontu.rounds3d.cardmanager";
        private const string ModName = "Card Manager";
        private const string Version = "0.0.0"; 

        public static CMBL instance { get; private set; }

        void Awake()
        {
            instance = this;

            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
    }
}
