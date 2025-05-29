using LupercaliaMGCore.modules.ExternalView.API;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.Player.Components
{
    /// <summary>
    /// Manages player's preview model used by ModelViewCamera.
    /// </summary>
    public class PreviewModelHandlerComponent : BasePlayerComponent
    {
        private readonly IExternalViewCsApi _Api;
        private IExternalViewCsEntity? _ModelEntity;

        private Vector3? _Rotation;
        private float _DetachParentAt = -1;
        private float _NextModelCheckAt = -1;

        public PreviewModelHandlerComponent(
            IExternalViewCsPlayer player,
            IExternalViewCsApi api
        ) : base(player)
        {
            _Api = api;
        }

        public override void OnEnable()
        {
            Player.IsVisible = false;

            _ModelEntity = _Api.CreatePreviewModelEntity(Player);
            if (_ModelEntity == null)
                return;

            // Assigning the preview model's parent makes it the model load faster somehow
            // Not sure why! We'll detach its parent to prevent spinning as the camera rotates.
            _ModelEntity.Parent = Player;
            _Rotation = Player.Rotation;

            _DetachParentAt = _Api.CurrentTime + 0.25f;
            _NextModelCheckAt = _Api.CurrentTime + Consts.ModelViewPreviewModelCheckInterval * 3;
        }

        public override void OnDisable()
        {
            if (_ModelEntity != null)
            {
                if (_ModelEntity.IsValid)
                {
                    _ModelEntity.Remove();

                }
                _ModelEntity = null;
            }

            Player.IsVisible = true;
        }

        public override void OnUpdate()
        {
            if (_ModelEntity == null)
            {
                return;
            }

            // Update the origin
            // We don't attach via SetParent since the yaw angles of both entities must be independent.
            _ModelEntity.Teleport(Player.Origin, _Rotation, Player.Velocity);

            if (_Api.CurrentTime > _DetachParentAt)
            {
                _ModelEntity.Parent = null;
                _DetachParentAt = float.MaxValue;
            }

            if (_Api.CurrentTime > _NextModelCheckAt)
            {
                // Check the model
                var playerModel = Player.Model;
                if (_ModelEntity.Model != playerModel)
                {
                    // Update the model
                    _ModelEntity.Model = playerModel;

                    // Player's model might be visible again. Ensure it is hidden.
                    Player.IsVisible = true;
                    Player.IsVisible = false;
                }

                _NextModelCheckAt = _Api.CurrentTime + Consts.ModelViewPreviewModelCheckInterval;
            }
        }
    }
}
