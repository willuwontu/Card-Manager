using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using UnityEngine;

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

        public static CardUpgrade GodMode = null;

        void Awake()
        {
            instance = this;

            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }

        void Start()
        {
            var playergo = UnityEngine.Resources.Load<GameObject>("Player");
            playergo.AddComponent<PL_CardHandler>();

            var godgo = new GameObject("GodMode", typeof(CardUpgrade), typeof(Card_Stats));
            GodMode = godgo.GetComponent<CardUpgrade>();

            DontDestroyOnLoad(godgo);

            var stats = GodMode.GetComponent<Card_Stats>();
            stats.stats = new PlayerStats();
            stats.stats.Health = new PlayerStatsEntry();
            stats.stats.LifeSteal = new PlayerStatsEntry();
            stats.stats.Speed = new PlayerStatsEntry();
            stats.stats.AbilityCooldown = new PlayerStatsEntry();
            stats.stats.FireRate = new PlayerStatsEntry();
            stats.stats.Spread = new PlayerStatsEntry();
            stats.stats.ReloadSpeed = new PlayerStatsEntry();
            stats.stats.Ammo = new PlayerStatsEntry();
            stats.stats.NrOfProjectiles = new PlayerStatsEntry();
            stats.stats.Damage = new PlayerStatsEntry();
            stats.stats.ProjectileSpeed = new PlayerStatsEntry();
            stats.stats.ProjectileBounces = new PlayerStatsEntry();
            stats.stats.Health.multiplier = 100;
            stats.stats.LifeSteal.baseValue = 1;
            stats.stats.LifeSteal.multiplier = 100;
            stats.stats.FireRate.multiplier = 0.1f;
            stats.stats.Ammo.baseValue = 100;
            stats.stats.Damage.multiplier = 100;
            stats.stats.ProjectileSpeed.multiplier = 1f;
            stats.stats.Spread.baseValue = 5f;
            stats.stats.Spread.multiplier = 5f;

            CardManager.RegisterCard("God", GodMode, "Card Manager", 1, true, true);
        }
    }
}
