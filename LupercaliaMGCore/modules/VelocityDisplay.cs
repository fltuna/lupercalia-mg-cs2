using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class VelocityDisplay(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    private readonly List<int> displayTarget = new();

    public override string PluginModuleName => "VelocityDisplay";
    
    public override string ModuleChatPrefix => "[VelocityDisplay]";

    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_vhud", "Displays current velocity and key input", CommandToggleVelocityDisplay);
        Plugin.RegisterListener<Listeners.OnTick>(OnTickListener);
        Plugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_vhud", CommandToggleVelocityDisplay);
        Plugin.RemoveListener<Listeners.OnTick>(OnTickListener);
        Plugin.RemoveListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
    }

    private void OnClientDisconnect(int playerSlot)
    {
        displayTarget.Remove(playerSlot);
    }

    private void CommandToggleVelocityDisplay(CCSPlayerController? client, CommandInfo info)
    {
        if (client == null)
            return;

        if (displayTarget.Contains(client.Slot))
        {
            displayTarget.Remove(client.Slot);
            client.PrintToChat(LocalizeWithPluginPrefix("VelocityDisplay.Command.Notification.HudDisabled"));
        }
        else
        {
            displayTarget.Add(client.Slot);
            client.PrintToChat(LocalizeWithPluginPrefix("VelocityDisplay.Command.Notification.HudEnabled"));
        }
    }


    private void OnTickListener()
    {
        if (displayTarget.Count < 1)
            return;

        foreach (int playerSlot in displayTarget)
        {
            // tuna: I don't do any real player check, since displayTarget list should only contain real player

            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
            
            if (player == null)
                continue;
            
            CCSPlayerPawn? pawn = player.PlayerPawn.Value;
            
            if (pawn == null)
                continue;

            if (PlayerUtil.IsPlayerAlive(player))
            {
                StringBuilder hud = new();


                hud.Append($"<font class='fontSize-{CenterHtmlSize.M.ToLowerString()}'>Speed</font>: <font color='#3791db' class='fontSize-{CenterHtmlSize.XL.ToLowerString()}'>{CalculateVelocity(pawn.Velocity):F2}</font>/s");
                hud.Append("<br>");
                hud.Append($"<font class='fontSize-{CenterHtmlSize.ML.ToLowerString()}'>" + GetPlayerPressedButtonText(player) + "</font>");
                
                
                player.PrintToCenterHtml(hud.ToString());
            }
            
            if (player.Team == CsTeam.Spectator || !PlayerUtil.IsPlayerAlive(player))
            {

                CPlayer_ObserverServices? observerServices = pawn.ObserverServices;
                
                if (observerServices == null)
                    continue;

                CBaseEntity? observeTarget = observerServices.ObserverTarget.Value;
                
                if (observeTarget == null || !observeTarget.IsValid)
                    continue;

                CCSPlayerPawn? observingPlayer = observeTarget as CCSPlayerPawn;
                
                if (observingPlayer == null)
                    continue;
                
                if (observingPlayer.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                    continue;
                
                if (observingPlayer.OriginalController.Value == null)
                    continue;
                
                StringBuilder hud = new();


                hud.Append($"<font class='fontSize-{CenterHtmlSize.M.ToLowerString()}'>Speed</font>: <font color='#3791db' class='fontSize-{CenterHtmlSize.XL.ToLowerString()}'>{CalculateVelocity(observingPlayer.Velocity):F2}</font>/s");
                hud.Append("<br>");
                hud.Append($"<font class='fontSize-{CenterHtmlSize.ML.ToLowerString()}'>" + GetPlayerPressedButtonText(observingPlayer.OriginalController.Value) + "</font>");
                
                
                player.PrintToCenterHtml(hud.ToString());
            }
        }
    }

    private const string ButtonViewTextBase = "%KEY_STRAFE_LEFT% %KEY_STRAFE_FORWARD% %KEY_STRAFE_BACK% %KEY_STRAFE_RIGHT% %KEY_JUMP% %KEY_DUCK%";

    private static readonly Dictionary<PlayerButtons, (string placeholder, string pressed, string notPressed)> ButtonMapping = new()
    {
        { PlayerButtons.Moveleft, ("%KEY_STRAFE_LEFT%", "A", "_") },
        { PlayerButtons.Back, ("%KEY_STRAFE_BACK%", "S", "_") },
        { PlayerButtons.Forward, ("%KEY_STRAFE_FORWARD%", "W", "_") },
        { PlayerButtons.Moveright, ("%KEY_STRAFE_RIGHT%", "D", "_") },
        { PlayerButtons.Jump, ("%KEY_JUMP%", "J", "_") },
        { PlayerButtons.Duck, ("%KEY_DUCK%", "C", "_") }
    };

    private string GetPlayerPressedButtonText(CCSPlayerController player)
    {
        StringBuilder resultBuilder = new(ButtonViewTextBase);
    
        foreach (var mapping in ButtonMapping)
        {
            var (placeholder, pressed, notPressed) = mapping.Value;
            bool isPressed = (player.Buttons & mapping.Key) != 0;
            string replacement = isPressed ? pressed : notPressed;
        
            resultBuilder.Replace(placeholder, replacement);
        }
    
        return resultBuilder.ToString();
    }

    private double CalculateVelocity(CNetworkVelocityVector vector)
    {
        return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2));
    }
}