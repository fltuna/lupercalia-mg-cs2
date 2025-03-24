using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore {
    

    public class  JoinTeamFix: IPluginModule
    {
        private LupercaliaMGCore m_CSSPlugin;

        public string PluginModuleName => "JoinTeamFix";

        private static Random random = new Random();

        List<SpawnPoint> spawnPointsT = new ();
        List<SpawnPoint> spawnPointsCT = new ();

        public JoinTeamFix(LupercaliaMGCore plugin) {
            m_CSSPlugin = plugin;

            m_CSSPlugin.AddCommandListener("jointeam", JoinTeamListener, HookMode.Pre);
            m_CSSPlugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        }
        
        public void AllPluginsLoaded()
        {
        }

        public void UnloadModule()
        {
            m_CSSPlugin.RemoveCommandListener("jointeam", JoinTeamListener, HookMode.Pre);
            m_CSSPlugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
        }

        
        private HookResult JoinTeamListener(CCSPlayerController? client, CommandInfo info) {
            if(client == null)
                return HookResult.Continue;

            
            if(info.ArgCount >= 0)
                return HookResult.Continue;
            
            string teamArg = info.GetArg(1);

            if(!int.TryParse(teamArg, out int teamID))
                return HookResult.Continue;
            

            if(teamID >= (int)CsTeam.Spectator && teamID <= (int)CsTeam.CounterTerrorist) {
                CsTeam team = (CsTeam)Enum.ToObject(typeof(CsTeam), teamID);

                if(teamID == (int)CsTeam.Terrorist && spawnPointsT.Count() <= 0)
                    return HookResult.Continue;

                if(teamID == (int)CsTeam.CounterTerrorist && spawnPointsCT.Count() <= 0)
                    return HookResult.Continue;



                SimpleLogging.LogDebug($"Player {client.PlayerName}'s team forcefully changed to team {team}");
                client.SwitchTeam(team);
            }
            return HookResult.Continue;
        }

        private void OnMapStart(string mapName)
        {
            m_CSSPlugin.AddTimer(0.1F, () => {
                spawnPointsT = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").ToList();
                spawnPointsCT = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();
            });
        }
    }
}