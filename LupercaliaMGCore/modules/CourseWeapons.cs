using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class CourseWeapons(IServiceProvider serviceProvider) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "Course Weapons";

    public override string ModuleChatPrefix => "[Course Weapons]";

    
    public readonly FakeConVar<bool> IsModuleEnabled =
        new("lp_mg_course_weapon_enabled", "Course weapon feature is enabled", false);
    
    
    protected override void OnInitialize()
    {
        Plugin.AddCommand("css_glock", "Gives a glock", CommandGiveGlock);
        Plugin.AddCommand("css_usp", "Gives a usp", CommandGiveUsp);
        Plugin.AddCommand("css_he", "Gives a he grenade", CommandGiveHeGrenade);
    }

    protected override void OnUnloadModule()
    {
        Plugin.RemoveCommand("css_glock", CommandGiveGlock);
        Plugin.RemoveCommand("css_usp", CommandGiveUsp);
        Plugin.RemoveCommand("css_he", CommandGiveHeGrenade);
    }


    private void CommandGiveGlock(CCSPlayerController? client, CommandInfo info)
    {
        if(!CommandIsExecutable(client))
            return;

        CsItem glock = CsItem.Glock18;
        
        if(!CanGiveItem(client!, glock))
            return;

        GiveItemToPlayer(client!, glock);
    }


    private void CommandGiveUsp(CCSPlayerController? client, CommandInfo info)
    {
        if(!CommandIsExecutable(client))
            return;

        // TODO: USPS is recognized as HKP2000 in game so we cannot do a duplicate test.
        CsItem usps = CsItem.USPS;
        
        if(!CanGiveItem(client!, usps))
            return;

        GiveItemToPlayer(client!, usps);
    }

    private void CommandGiveHeGrenade(CCSPlayerController? client, CommandInfo info)
    {
        if(!CommandIsExecutable(client))
            return;

        CsItem grenade = CsItem.HEGrenade;
        
        if(!CanGiveItem(client!, grenade))
            return;
        
        GiveItemToPlayer(client!, grenade);
    }


    private bool CommandIsExecutable(CCSPlayerController? client)
    {
        SimpleLogging.LogDebug($"[Course Weapons] Checking command is executable from {client?.PlayerName}");
        if (client == null)
            return false;
        
        if (!PlayerUtil.IsPlayerAlive(client))
        {
            SimpleLogging.LogDebug($"[Course Weapons] [{client.PlayerName}] Player is already dead");
            
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Notification.ShouldBeAlive"));
            return false;
        }

        bool isCourseWeaponEnabled = IsModuleEnabled.Value;

        if (!isCourseWeaponEnabled)
        {
            SimpleLogging.LogDebug($"[Course Weapons] [{client.PlayerName}] Course Weapon feature is disabled");
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Notification.CourseMapOnly"));
            return false;
        }

        return true;
    }

    private void GiveItemToPlayer(CCSPlayerController client, CsItem item)
    {
        client.GiveNamedItem(item);
        client.PrintToChat(LocalizeWithPluginPrefix("CourseWeapon.Command.Notification.Retrieved", item.ToString()));
        
        SimpleLogging.LogDebug($"[Course Weapons] [{client.PlayerName}] Gave {item}");
    }

    private bool CanGiveItem(CCSPlayerController client, CsItem item)
    {
        SimpleLogging.LogTrace($"[Course Weapons] [{client.PlayerName}] Checking player can get {item.ToString()}");
        CPlayer_WeaponServices? weaponServices = client.PlayerPawn.Value!.WeaponServices;

        if (weaponServices == null)
        {
            SimpleLogging.LogTrace($"[Course Weapons] [{client.PlayerName}] Failed to obtain a WeaponServices instance.");
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Notification.UnknownError"));
            return false;
        }

        var weapons = weaponServices.MyWeapons;

        List<string?> clientWeapons = weapons.Select(it => it.Value?.DesignerName).ToList();

        
        // To find EnumMember attribute value from CsItem Enum

        string? itemName = EnumUtils.GetEnumMemberAttributeValue(item);

        if (itemName == null)
        {
            SimpleLogging.LogTrace($"[Course Weapons] [{client.PlayerName}] Failed to find weapon.");
            client.PrintToChat(LocalizeWithPluginPrefix("General.Command.Notification.UnknownError"));
            return false;
        }
        
        if (clientWeapons.Contains(itemName))
        {
            SimpleLogging.LogTrace($"[Course Weapons] [{client.PlayerName}] Player already have a {item.ToString()}.");
            
            client.PrintToChat(LocalizeWithPluginPrefix("CourseWeapon.Command.Notification.AlreadyHave", item.ToString()));
            return false;
        }
        
        
        SimpleLogging.LogTrace($"[Course Weapons] [{client.PlayerName}] We can give a {item.ToString()}.");
        return true;
    }
}