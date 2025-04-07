using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore;

public class PlayerSlapEvent(Omikuji omikuji, LupercaliaMGCore plugin) : OmikujiEventBase(omikuji, plugin)
{
    public override string EventName => "Player Slap Event";

    public override OmikujiType OmikujiType => OmikujiType.EventBad;

    public override OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PlayerAlive;

    public override void Execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player Slap Event");

        CCSPlayerPawn? pawn = client.PlayerPawn.Value;

        if (pawn == null)
        {
            SimpleLogging.LogDebug("Player Slap Event: Pawn is null! cancelling!");
            return;
        }

        Vector velo = pawn.AbsVelocity;

        int slapPowerMin = PluginSettings.m_CVOmikujiEventPlayerSlapPowerMin.Value;
        int slapPowerMax = PluginSettings.m_CVOmikujiEventPlayerSlapPowerMax.Value;

        SimpleLogging.LogTrace($"Player Slap Event: Random slap power - Min: {slapPowerMin}, Max: {slapPowerMax}");

        // Taken from sourcemod
        velo.X += ((Random.NextInt64(slapPowerMin, slapPowerMax) % 180) + 50) * (((Random.NextInt64(slapPowerMin, slapPowerMax) % 2) == 1) ? -1 : 1);
        velo.Y += ((Random.NextInt64(slapPowerMin, slapPowerMax) % 180) + 50) * (((Random.NextInt64(slapPowerMin, slapPowerMax) % 2) == 1) ? -1 : 1);
        velo.Z += Random.NextInt64(slapPowerMin, slapPowerMax) % 200 + 100;
        SimpleLogging.LogTrace($"Player Slap Event: Player velocity - {velo.X} {velo.Y} {velo.Z}");

        Server.PrintToChatAll($"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {Plugin.Localizer["Omikuji.BadEvent.PlayerSlapEvent.Notification.Slapped", client.PlayerName]}");
    }

    public override double GetOmikujiWeight()
    {
        return PluginSettings.m_CVOmikujiEventPlayerSlapSelectionWeight.Value;
    }
}