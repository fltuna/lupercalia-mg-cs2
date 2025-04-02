using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore;

public class VelocityDisplay: IPluginModule
{
    private LupercaliaMGCore m_CSSPlugin;
    
    private readonly List<int> displayTarget = new();

    public VelocityDisplay(LupercaliaMGCore plugin)
    {
        m_CSSPlugin = plugin;

        m_CSSPlugin.AddCommand("css_vhud", "Displays current velocity and key input", CommandToggleVelocityDisplay);
        m_CSSPlugin.RegisterListener<Listeners.OnTick>(OnTickListener);
        m_CSSPlugin.RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
    }

    public string PluginModuleName => "VelocityDisplay";
    
    public void AllPluginsLoaded()
    {
    }

    public void UnloadModule()
    {
        m_CSSPlugin.RemoveCommand("css_vhud", CommandToggleVelocityDisplay);
        m_CSSPlugin.RemoveListener<Listeners.OnTick>(OnTickListener);
        m_CSSPlugin.RemoveListener<Listeners.OnClientDisconnect>(OnClientDisconnect);
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
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("VelocityDisplay.Command.Notification.HudDisabled"));
        }
        else
        {
            displayTarget.Add(client.Slot);
            client.PrintToChat(m_CSSPlugin.LocalizeStringWithPrefix("VelocityDisplay.Command.Notification.HudEnabled"));
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

            if (pawn.LifeState == (byte)LifeState_t.LIFE_ALIVE)
            {
                StringBuilder hud = new();


                hud.Append($"<font class='fontSize-{CenterHtmlSize.M.ToLowerString()}'>Speed</font>: <font color='#3791db' class='fontSize-{CenterHtmlSize.XL.ToLowerString()}'>{CalculateVelocity(pawn.Velocity):F2}</font>/s");
                hud.Append("<br>");
                hud.Append($"<font class='fontSize-{CenterHtmlSize.ML.ToLowerString()}'>" + GetPlayerPressedButtonText(player) + "</font>");
                
                
                player.PrintToCenterHtml(hud.ToString());
            }
            if (player.Team == CsTeam.Spectator)
            {
                // TODO(): when the player is spectator, then print the observe target velocity.
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