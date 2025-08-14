using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using LupercaliaMGCore.modules.ExternalView.API;
using System.Drawing;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal class ExternalViewCsApi : IExternalViewCsApi
    {
        private readonly ILocalizer _Localizer;
        private readonly IExternalViewConVars _ConVars;

        public ExternalViewCsApi(ILocalizer localizer, IExternalViewConVars conVars)
        {
            _Localizer = localizer;
            _ConVars = conVars;
        }

        public float DeltaTime => Server.FrameTime;

        public float CurrentTime => Server.CurrentTime;

        public IExternalViewConVars ConVars => _ConVars;

        public IEnumerable<IExternalViewCsPlayer> AllPlayers => Utilities.GetPlayers().Select(controller => CreatePlayer(controller));

        public IExternalViewCsPlayer? GetPlayer(ulong id)
        {
            // Don't use this.
            //var controller = Utilities.GetPlayerFromSteamId(id);
            var controller = Utilities.GetPlayers().FirstOrDefault(player => player.SteamID == id);

            if (controller == null)
                return null;
            return CreatePlayer(controller);
        }

        public IExternalViewCsPlayer? GetPlayerBySlot(int slot)
        {
            var controller = Utilities.GetPlayerFromSlot(slot);
            if (controller == null)
                return null;
            return CreatePlayer(controller);
        }

        public IExternalViewCsEntity? CreateCameraEntity()
        {
            var cameraEnt = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            if (cameraEnt == null)
                return null;

            cameraEnt.Spawnflags = 256; // No collisions. Maybe we don't need it because the prop model is empty.
            cameraEnt.DispatchSpawn();

            cameraEnt.Render = Color.FromArgb(0, 255, 255, 255);
            Utilities.SetStateChanged(cameraEnt, "CBaseModelEntity", "m_clrRender");

            return new ExternalViewCsEntity(cameraEnt);
        }

        public IExternalViewCsEntity? CreatePreviewModelEntity(IExternalViewCsPlayer player)
        {
            var modelEnt = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");
            if (modelEnt == null)
                return null;

            modelEnt.SetModel(player.Model);
            modelEnt.Spawnflags = 2097152; // No collisions
            var ent = new ExternalViewCsEntity(modelEnt);
            ent.Teleport(player.Origin, player.Rotation, player.Velocity);

            modelEnt.DispatchSpawn();

            return ent;
        }

        private ExternalViewCsPlayer CreatePlayer(CCSPlayerController controller)
        {
            return new ExternalViewCsPlayer(controller, _Localizer);
        }
    }
}
