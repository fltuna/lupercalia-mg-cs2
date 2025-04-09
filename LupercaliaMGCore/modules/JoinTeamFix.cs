using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class JoinTeamFix(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "JoinTeamFix";

    public override string ModuleChatPrefix => "[Join Team Fix]";

    private static Random random = new();

    private List<SpawnPoint> spawnPointsT = new();
    private List<SpawnPoint> spawnPointsCt = new();

    protected override void OnInitialize()
    {
        Plugin.AddCommandListener("jointeam", JoinTeamListener, HookMode.Pre);
        Plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommandListener("jointeam", JoinTeamListener, HookMode.Pre);
        Plugin.RemoveListener<Listeners.OnMapStart>(OnMapStart);
    }


    private HookResult JoinTeamListener(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return HookResult.Continue;


        if (info.ArgCount >= 0)
            return HookResult.Continue;

        string teamArg = info.GetArg(1);

        if (!int.TryParse(teamArg, out int teamId))
            return HookResult.Continue;


        if (teamId >= (int)CsTeam.Spectator && teamId <= (int)CsTeam.CounterTerrorist)
        {
            CsTeam team = (CsTeam)Enum.ToObject(typeof(CsTeam), teamId);

            if (teamId == (int)CsTeam.Terrorist && !spawnPointsT.Any())
                return HookResult.Continue;

            if (teamId == (int)CsTeam.CounterTerrorist && !spawnPointsCt.Any())
                return HookResult.Continue;


            DebugLogger.LogDebug($"Player {client.PlayerName}'s team forcefully changed to team {team}");
            client.SwitchTeam(team);
        }

        return HookResult.Continue;
    }

    private void OnMapStart(string mapName)
    {
        Plugin.AddTimer(0.1F, () =>
        {
            spawnPointsT = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").ToList();
            spawnPointsCt = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();
        });
    }
}