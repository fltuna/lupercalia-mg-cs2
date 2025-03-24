using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore;

public class PlayerSlapEvent : IOmikujiEvent
{
    public string EventName => "Player Slap Event";

    public OmikujiType OmikujiType => OmikujiType.EVENT_BAD;

    public OmikujiCanInvokeWhen OmikujiCanInvokeWhen => OmikujiCanInvokeWhen.PLAYER_ALIVE;

    private static Random random = OmikujiEvents.random;

    public void execute(CCSPlayerController client)
    {
        SimpleLogging.LogDebug("Player drew a omikuji and invoked Player Slap Event");

        CCSPlayerPawn? pawn = client.PlayerPawn.Value;

        if (pawn == null)
        {
            SimpleLogging.LogDebug("Player Slap Event: Pawn is null! cancelling!");
            return;
        }

        Vector velo = pawn.AbsVelocity;

        int slapPowerMin = PluginSettings.GetInstance.m_CVOmikujiEventPlayerSlapPowerMin.Value;
        int slapPowerMax = PluginSettings.GetInstance.m_CVOmikujiEventPlayerSlapPowerMax.Value;

        SimpleLogging.LogTrace($"Player Slap Event: Random slap power - Min: {slapPowerMin}, Max: {slapPowerMax}");

        // Taken from sourcemod
        velo.X += ((random.NextInt64(slapPowerMin, slapPowerMax) % 180) + 50) *
                  (((random.NextInt64(slapPowerMin, slapPowerMax) % 2) == 1) ? -1 : 1);
        velo.Y += ((random.NextInt64(slapPowerMin, slapPowerMax) % 180) + 50) *
                  (((random.NextInt64(slapPowerMin, slapPowerMax) % 2) == 1) ? -1 : 1);
        velo.Z += random.NextInt64(slapPowerMin, slapPowerMax) % 200 + 100;
        SimpleLogging.LogTrace($"Player Slap Event: Player velocity - {velo.X} {velo.Y} {velo.Z}");

        Server.PrintToChatAll(
            $"{Omikuji.ChatPrefix} {Omikuji.GetOmikujiLuckMessage(OmikujiType, client)} {LupercaliaMGCore.getInstance().Localizer["Omikuji.BadEvent.PlayerSlapEvent.Notification.Slapped", client.PlayerName]}");
    }

    public void initialize()
    {
    }

    public double getOmikujiWeight()
    {
        return PluginSettings.GetInstance.m_CVOmikujiEventPlayerSlapSelectionWeight.Value;
    }
}