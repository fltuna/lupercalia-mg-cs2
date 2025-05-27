using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules
{
    /// <summary>
    /// Duplicates spawn points to increase the maximum players for the map.
    /// 
    /// You can test this via adding bots in your test server.
    /// Make sure no block is enabled.
    /// 
    /// NOTE:
    /// In the first round where players (or bots) join, the game will assign
    /// the same spawn point for all extra players.
    /// To evenly distribute the all spawn points, just start a new round after join.
    /// 
    /// Handy commands for testing:
    /// ```
    /// sv_cheats 1
    /// ent_find info_player_
    /// 
    /// bot_add;bot_add;bot_add;bot_add;bot_add;bot_add;bot_add;bot_add;
    /// ```
    /// </summary>
    public sealed class SpawnPointDuplicator(IServiceProvider serviceProvider)
        : PluginModuleBase(serviceProvider)
    {
        private static int MAX_PLAYERS = 64;

        private static string SPAWN_CT = "info_player_counterterrorist";
        private static string SPAWN_T = "info_player_terrorist";

        public override string PluginModuleName => "Spawn Point Duplicator";
        public override string ModuleChatPrefix => "[Spawn Point Duplicator]";
        protected override bool UseTranslationKeyInModuleChatPrefix => false;

        // ConVars
        public readonly FakeConVar<bool> IsModuleEnabled =
            new("lp_mp_spawn_point_duplicator_enabled", "Spawn point duplicator is enabled.", true);

        protected override void OnInitialize()
        {
            Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        }

        protected override void OnUnloadModule()
        {
            Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        }

        private void OnMapStart(string mapName)
        {
            Server.NextFrame(ValidateSpawnPoints);
        }

        private void ValidateSpawnPoints()
        {
            var ctSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(SPAWN_CT).ToList();
            var tSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(SPAWN_T).ToList();

            if (ctSpawns.Count > 0 && tSpawns.Count > 0)
            {
                // Both sides have their spawn points
                EnsureSpawnPoints(SPAWN_CT, MAX_PLAYERS / 2);
                EnsureSpawnPoints(SPAWN_T, MAX_PLAYERS / 2);
            }
            else if (ctSpawns.Count > 0)
            {
                // Only CT
                EnsureSpawnPoints(SPAWN_CT, MAX_PLAYERS);
            }
            else if (tSpawns.Count > 0)
            {
                // Only T
                EnsureSpawnPoints(SPAWN_T, MAX_PLAYERS);
            }
            else
            {
                DebugLogger.LogError($"[Spawn Point Duplicator] No spawn points found.");
            }
        }

        private void EnsureSpawnPoints(string designerName, int numSpawnsDesired)
        {
            var origSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(designerName)
                    .OrderBy(spawn => spawn.Priority)
                    .ToList();

            // Find the maximum priority.
            var maxPriority = Math.Min(origSpawns.Last().Priority, 100000);

            var numSpawnsToCreate = Math.Max(numSpawnsDesired - origSpawns.Count, 0);

            for (int i = 0; i < numSpawnsToCreate; i++)
            {
                var iSpawnGroup = i / origSpawns.Count;
                var origSpawnIdx = i % origSpawns.Count;

                var origSpawn = origSpawns[origSpawnIdx];
                var priority = origSpawn.Priority;
                priority += (maxPriority + 1) * (iSpawnGroup + 1);

                var spawn = Utilities.CreateEntityByName<SpawnPoint>(designerName);
                if (spawn == null)
                {
                    // Failed to create spawn
                    return;
                }
                spawn.Priority = priority;
                spawn.Teleport(origSpawn.AbsOrigin, origSpawn.AbsRotation);
                spawn.DispatchSpawn();

                Logger.LogTrace($"[Spawn Point Duplicator] <{designerName}> #{i}: priority = {priority}, position = {spawn.AbsOrigin}");
            }

            Logger.LogTrace($"[Spawn Point Duplicator] {numSpawnsToCreate}/{numSpawnsDesired} of {designerName} have been created.");
        }
    }
}
