using CounterStrikeSharp.API.Modules.Commands.Targeting;
using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Cameras;
using LupercaliaMGCore.modules.ExternalView.Player.Components;

namespace LupercaliaMGCore.modules.ExternalView.Player
{
    /// <summary>
    /// An instance created for camera entity per player.
    /// 
    /// Observers are not instantiated for those players with
    /// the default view (i.e., the first-person camera).
    /// </summary>
    public class ExternalViewPlayer : ICameraContext
    {
        public IExternalViewCsApi Api { get; }
        public IExternalViewCsPlayer Player { get; }
        public PlayerConfig Config { get; }

        public IExternalViewCsEntity? CameraEntity => _ViewEntityHandler?.GetCameraEnt();
        public BaseCamera? Camera { get; private set; }

        // Components
        private ViewEntityHandlerComponent? _ViewEntityHandler;
        private PlayerLockerComponent? _PlayerLocker;
        private PreviewModelHandlerComponent? _PreviewModelHandler;


        public ExternalViewPlayer(
            IExternalViewCsApi api,
            IExternalViewCsPlayer player,
            PlayerConfig config
        )
        {
            Api = api;
            Player = player;
            Config = config;
        }

        public void OnRemove()
        {
            ActivateFirstPerson();
        }

        public bool Update()
        {
            if (Player == null)
            {
                return false;
            }

            if (!Player.IsValid)
            {
                // The player has been disconnected from the server.
                return false;
            }

            Player.UpdateButtonState();

            if (Player.IsSpectator)
            {
                // The player has been moved to spectators.
                return false;
            }

            if (Camera == null)
            {
                // The first-person camera is active.
                return false;
            }

            // Validate the camera entity
            var cameraEntity = CameraEntity;
            if (cameraEntity == null || !cameraEntity.IsValid)
            {
                // Recreate camera
                _ViewEntityHandler?.Disable();
                _ViewEntityHandler?.Enable();

                if (cameraEntity == null || !cameraEntity.IsValid)
                {
                    return false;
                }
            }

            // Update the camera
            var isValidCamera = Camera.Update();
            if (!isValidCamera)
            {
                // Camera is no longer valid
                if (!IsTemporaryCamera)
                {
                    // Must be in third person. Back to the first person.
                    Player.PrintToChat("ExternalView.FirstPerson.Revert");
                    return false;
                }
                
                if (Config.PrimaryCameraMode == PrimaryCameraModes.FirstPerson)
                {
                    // Primary mode is first person.
                    Player.PrintToChat("ExternalView.FirstPerson.Revert");
                    return false;
                }

                // If the preferred camera mode is third person, we can switch back to third-person camera.
                ActivateThirdPerson();
                Player.PrintToChat("ExternalView.ThirdPerson.Revert");
            }

            // Update components
            _ViewEntityHandler?.Update();
            _PlayerLocker?.Update();
            _PreviewModelHandler?.Update();

            return true;
        }

        public void ActivateFirstPerson()
        {
            // This instance will be removed in the next tick.
            SetCamera(null);
        }

        public void ActivateThirdPerson()
        {
            SetCamera(new ThirdPersonCamera(this));
        }

        public void ActivateModelView()
        {
            SetCamera(new ModelViewCamera(this));
        }

        public void ActivateFreeCam()
        {
            SetCamera(new FreeCamera(this));
        }

        public void ActivateWatchCam(string? target)
        {
            var watchCamera = Camera as WatchCamera;
            if (watchCamera != null && target != null)
            {
                watchCamera.FindTarget(target);
                return;
            }
            SetCamera(new WatchCamera(this, target));
        }

        private void SetCamera(BaseCamera? camera)
        {
            if (Camera?.GetType() == camera?.GetType())
            {
                // No need to swap the camera
                return;
            }

            Camera = camera;

            VerifyComponents();

            if (Camera == null)
                return;

            Camera.Update();
        }


        public bool CanUseObserverView
        {
            get
            {
                if (Api.ConVars.IsObserverViewEnabled)
                    return true;

                if (Api.ConVars.IsAdminPrevilegesEnabled)
                    return Player.IsAdmin;

                return false;
            }
        }

        private bool IsTemporaryCamera
        {
            get
            {
                var isFirstPersonCamera = Camera == null;
                var isThirdPersonCamera = (Camera as ThirdPersonCamera) != null;

                if (isFirstPersonCamera || isThirdPersonCamera)
                {
                    return false;
                }

                // All camera types other than FP and TP cameras are considered as observer cameras.
                return true;
            }
        }

        void VerifyComponents()
        {
            var isViewEntityHandlerEnabled = Camera != null;
            var isPlayerLockerEnabled = IsTemporaryCamera;
            var isPreviewModelHandlerEnabled = Camera is ModelViewCamera;

            if (isViewEntityHandlerEnabled)
            {
                if (_ViewEntityHandler == null)
                {
                    _ViewEntityHandler = new ViewEntityHandlerComponent(Player, Api);
                    _ViewEntityHandler.Enable();
                }
            }
            else
            {
                if (_ViewEntityHandler != null)
                {
                    _ViewEntityHandler.Disable();
                    _ViewEntityHandler = null;
                }
            }

            if (isPlayerLockerEnabled)
            {
                if (_PlayerLocker == null)
                {
                    _PlayerLocker = new PlayerLockerComponent(Player);
                    _PlayerLocker.Enable();
                }
            }
            else
            {
                if (_PlayerLocker != null)
                {
                    _PlayerLocker.Disable();
                    _PlayerLocker = null;
                }
            }

            if (isPreviewModelHandlerEnabled)
            {
                if (_PreviewModelHandler == null)
                {
                    _PreviewModelHandler = new PreviewModelHandlerComponent(Player, Api);
                    _PreviewModelHandler.Enable();
                }
            }
            else
            {
                if (_PreviewModelHandler != null)
                {
                    _PreviewModelHandler.Disable();
                    _PreviewModelHandler = null;
                }
            }
        }
    }
}
