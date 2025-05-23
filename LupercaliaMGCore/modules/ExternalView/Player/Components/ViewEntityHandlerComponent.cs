using LupercaliaMGCore.modules.ExternalView.API;

namespace LupercaliaMGCore.modules.ExternalView.Player.Components
{
    /// <summary>
    /// Manages player's view entity.
    /// 
    /// It won't override the view entity when player's view entity is already set
    /// by other (e.g., CPointViewControl).
    /// </summary>
    public class ViewEntityHandlerComponent : BasePlayerComponent
    {
        private readonly IExternalViewCsApi _Api;
        private IExternalViewCsEntity? _CameraEntity;

        public ViewEntityHandlerComponent(
            IExternalViewCsPlayer player,
            IExternalViewCsApi api
        ) : base(player)
        {
            _Api = api;
        }

        public IExternalViewCsEntity? GetCameraEnt()
        {
            return _CameraEntity;
        }

        public override void OnEnable()
        {
            // Create a camera entity.
            _CameraEntity = _Api.CreateCameraEntity();
            _CameraEntity?.Teleport(Player.Origin + Consts.EyeOffset, Player.ViewAngle, null);
        }

        public override void OnDisable()
        {
            if (_CameraEntity != null)
            {
                // Restore player's camera entity
                var viewEntity = Player.ViewEntity;
                if (viewEntity != null && viewEntity == _CameraEntity)
                {
                    Player.ViewEntity = null;
                }

                // Remove the camera entity.
                if (_CameraEntity.IsValid)
                {
                    _CameraEntity.Remove();
                }
                _CameraEntity = null;
            }
        }

        public override void OnUpdate()
        {
            if (_CameraEntity == null)
            {
                // Something went wrong...
                return;
            }

            if (Player.ViewEntity != null)
            {
                // The camera entity is already set by this component or other (e.g., CPointViewControl)
                return;
            }

            // Set the camera entity
            Player.ViewEntity = _CameraEntity;
        }
    }
}
