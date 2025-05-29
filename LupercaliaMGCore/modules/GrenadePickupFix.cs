using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;

namespace LupercaliaMGCore.modules
{
    /// <summary>
    /// Fixes grenade pickup.
    /// 
    /// Players can't select grenades via `slot4` if they are spawned by the map.
    /// This module swaps those inconvenient grenades with non-corrupted versions on pickup
    /// to fix the issue.
    /// 
    /// NOTE: healthshot will be also fixed!
    /// </summary>
    public sealed class GrenadePickupFix(IServiceProvider serviceProvider)
        : PluginModuleBase(serviceProvider)
    {
        public override string PluginModuleName => "Grenade Pickup Fix";
        public override string ModuleChatPrefix => "[Grenade Pickup Fix]";
        protected override bool UseTranslationKeyInModuleChatPrefix => false;

        public readonly FakeConVar<bool> IsModuleEnabled =
            new("lp_mg_grenade_pickup_fix", "Grenade pickup fix is enabled", true);

        private static readonly HashSet<string> GrenadeClassnames = new()
        {
            "weapon_flashbang",     // defindex = 43, m_GearSlotPosition = 1, m_DefaultLoadoutPosition = LOADOUT_POSITION_GRENADE2
            "weapon_hegrenade",     // defindex = 44, m_GearSlotPosition = 0, m_DefaultLoadoutPosition = LOADOUT_POSITION_GRENADE3
            "weapon_smokegrenade",  // defindex = 45, m_GearSlotPosition = 2, m_DefaultLoadoutPosition = LOADOUT_POSITION_GRENADE4
            "weapon_molotov",       // defindex = 46, m_GearSlotPosition = 4, m_DefaultLoadoutPosition = LOADOUT_POSITION_FIRST_WHEEL_GRENADE
            "weapon_decoy",         // defindex = 47, m_GearSlotPosition = 3, m_DefaultLoadoutPosition = LOADOUT_POSITION_GRENADE1
            "weapon_incgrenade",    // defindex = 48, m_GearSlotPosition = 4, m_DefaultLoadoutPosition = LOADOUT_POSITION_FIRST_WHEEL_GRENADE
        };

        protected override void OnInitialize()
        {
            TrackConVar(IsModuleEnabled);

            Plugin.HookEntityOutput("*", "OnPlayerPickup", OnPlayerPickup);
        }

        protected override void OnUnloadModule()
        {
            Plugin.UnhookEntityOutput("*", "OnPlayerPickup", OnPlayerPickup);
        }

        private HookResult OnPlayerPickup(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
        {
            FixGrenade(activator, caller);

            return HookResult.Continue;
        }

        private void FixGrenade(CEntityInstance activator, CEntityInstance caller)
        {
            var isGrenade = GrenadeClassnames.Contains(caller.DesignerName);
            if (!isGrenade)
                return;

            if (!IsModuleEnabled.Value)
                return;

            var grenade = new CCSWeaponBase(caller.Handle);
            if (!grenade.IsValid)
                return;

            var pawn = new CCSPlayerPawn(activator.Handle);
            if (!pawn.IsValid)
                return;

            var controller = new CCSPlayerController(pawn.Controller?.Value?.Handle ?? 0);
            if (!controller.IsValid)
                return;

            // Checks if the grenade is spawned by the map
            var isCorrupted = grenade.VData?.GearSlotPosition == -1;
            if (!isCorrupted)
                return;

            //
            // We can change the following values which are different from the proper grenades,
            // but it won't make the grenade selectable via "slot4" key unfortunately.
            //
            // - grenade.PlayerFireEventAttackType (corrupted: ePrimary, should be: eCount)
            // - grenade.vdata.GearSlotPosition    (corrupted: -1)
            // - grenade.vdata.DefaultLoadoutSlot  (corrupted: LOADOUT_SLOT_INVALID)
            //

            // First, search child entities attached to the grenade.
            var children = new List<CEntityInstance>();

            var childNode = grenade.CBodyComponent?.SceneNode?.Child;
            while (childNode != null)
            {
                var child = childNode.Owner;
                if (child != null)
                {
                    children.Add(child);
                }
                childNode = childNode.NextSibling;
            }

            // Detach the children from the grenade temporarily
            foreach (var child in children)
            {
                child.AcceptInput("SetParent", null, null);
            }

            // Keep grenade entity's info
            var origClassname = grenade.DesignerName;
            var origOrigin = grenade.AbsOrigin;
            var origRotation = grenade.AbsRotation;

            // Remove the corrupted grenade
            //player.RemovePlayerItem(grenade);  // It also removes new grenade we will create later. Don't use this.
            grenade.Remove();

            // In the current tick, the player still holds the corrupted grenade
            // Let's defer adding the new grenade
            Server.NextFrame(() =>
            {
                // Add new grenade
                var newGrenade = controller.GiveNamedItem<CBaseCSGrenade>(origClassname);
                if (newGrenade == null)
                    return;

                newGrenade.Teleport(origOrigin, origRotation);

                // Restore children
                foreach (var child in children)
                {
                    child.AcceptInput("SetParent", newGrenade, null, "!activator");
                }

                Logger.LogTrace($"[Grenade Pickup Fix] Fixed grenade <{origClassname}>");
            });
        }
    }
}