using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Cameras;
using LupercaliaMGCore.modules.ExternalView.Player;

namespace LupercaliaMGCore.modules.ExternalView
{
    public class ExternalViewSystem
    {
        private IExternalViewCsApi _Api;

        private Dictionary<ulong, PlayerConfig> _PlayerCfgs = new();
        private Dictionary<ulong, ExternalViewPlayer> _Players = new();

        public IEnumerable<ExternalViewPlayer> Players => _Players.Values;

        public ExternalViewSystem(IExternalViewCsApi api)
        {
            _Api = api;
        }

        public void Update()
        {
            // Update all players
            List<ulong> playerIdsToRemove = new();
            foreach (var (id, player) in _Players)
            {
                var isValid = player.Update();
                if (!isValid)
                {
                    playerIdsToRemove.Add(id);
                }
            }

            foreach (var id in playerIdsToRemove)
            {
                RemovePlayerCamera(id);
            }
        }

        public void Unload()
        {
            foreach (var player in _Players.Values)
            {
                player.ActivateFirstPerson();
            }
            _Players.Clear();
        }

        /// <summary>
        /// Reverts the camera to primary camera mode.
        ///
        /// Call this whenever the player spawns or killed.
        /// </summary>
        public void ActivatePrimaryCamera(ulong id, bool isReverting = false)
        {
            var playerCfg = EnsurePlayerConfig(id);
            var player = EnsurePlayer(id);
            if (player == null)
                return;

            if (playerCfg.PrimaryCameraMode == PrimaryCameraModes.FirstPerson)
            {
                player.ActivateFirstPerson();

                if (isReverting)
                {
                    player.Player.PrintToChat("ExternalView.FirstPerson.Revert");
                }
            }
            else
            {
                player.ActivateThirdPerson();

                if (isReverting)
                {
                    player.Player.PrintToChat("ExternalView.ThirdPerson.Revert");
                }
            }
        }

        public void ToggleThirdPerson(ulong id, float? cameraDistance, float? yawOffset)
        {
            // Update the preferences
            var playerCfg = EnsurePlayerConfig(id);
            var thirdPersonSettingsHasBeenChanged = false;
            if (cameraDistance.HasValue)
            {
                if (playerCfg.ThirdPersonCameraDistance != cameraDistance.Value)
                {
                    playerCfg.ThirdPersonCameraDistance = ValidateCameraDistance(cameraDistance.Value, id);
                    thirdPersonSettingsHasBeenChanged = true;
                }
            }
            if (yawOffset.HasValue)
            {
                if (playerCfg.ThirdPersonCameraYawOffset != yawOffset.Value)
                {
                    playerCfg.ThirdPersonCameraYawOffset = yawOffset.Value;
                    thirdPersonSettingsHasBeenChanged = true;
                }
            }

            // Update the camera mode
            var player = EnsurePlayer(id);
            if (player == null)
                return;

            // Check if we should revert to the first person
            var isInThirdPerson = player.Camera is ThirdPersonCamera;
            var shouldRevertToFirstPerson = isInThirdPerson && !thirdPersonSettingsHasBeenChanged;
            if (shouldRevertToFirstPerson)
            {
                playerCfg.PrimaryCameraMode = PrimaryCameraModes.FirstPerson;
                player.ActivateFirstPerson();

                if (player.Player.IsAlive)
                {
                    player.Player.PrintToChat("ExternalView.FirstPerson.Revert");
                }
                else
                {
                    player.Player.PrintToChat("ExternalView.FirstPerson.OnNextSpawn");
                }
            }
            else
            {
                playerCfg.PrimaryCameraMode = PrimaryCameraModes.ThirdPerson;
                player.ActivateThirdPerson();

                if (!isInThirdPerson)
                {
                    if (player.Player.IsAlive)
                    {
                        player.Player.PrintToChat("ExternalView.ThirdPerson.Start");
                    }
                    else
                    {
                        player.Player.PrintToChat("ExternalView.ThirdPerson.OnNextSpawn");
                    }
                }
                else
                {
                    player.Player.PrintToChat("ExternalView.ThirdPerson.Update");
                }
            }
        }

        public void ToggleModelView(ulong id)
        {
            var player = EnsurePlayer(id);
            if (player == null)
                return;

            if (player.Camera is ModelViewCamera)
            {
                ActivatePrimaryCamera(id, true);
            }
            else
            {
                player.ActivateModelView();
                player.Player.PrintToChat("ExternalView.ModelView.Start");
            }
        }

        public void ToggleFreeCam(ulong id)
        {
            var player = EnsurePlayer(id);
            if (player == null)
                return;

            if (player.Camera is FreeCamera)
            {
                ActivatePrimaryCamera(id, true);
                return;
            }

            if (!player.CanUseObserverView)
            {
                player.Player.PrintToChat("ExternalView.FreeCam.RequirePermission");
                return;
            }

            player.ActivateFreeCam();
            player.Player.PrintToChat("ExternalView.FreeCam.Start");
        }

        public void ToggleWatch(ulong id, string? target)
        {
            var player = EnsurePlayer(id);
            if (player == null)
                return;

            if (player.Camera is WatchCamera)
            {
                if (target == null)
                {
                    ActivatePrimaryCamera(id, true);
                    return;
                }
            }

            if (!player.CanUseObserverView)
            {
                player.Player.PrintToChat("ExternalView.Watch.RequirePermission");
                return;
            }

            player.ActivateWatchCam(target);
            player.Player.PrintToChat("ExternalView.Watch.Start");
        }

        public void SetThirdPersonCameraDistance(ulong id, float? cameraDistance)
        {
            if (!cameraDistance.HasValue)
                cameraDistance = Consts.DefaultCameraDistance;

            var playerCfg = EnsurePlayerConfig(id);
            playerCfg.ThirdPersonCameraDistance = ValidateCameraDistance(cameraDistance.Value, id);

            _Api.GetPlayer(id)?.PrintToChat(
                "ExternalView.ThirdPerson.CameraDistance.Set",
                playerCfg.ThirdPersonCameraDistance
            );
        }

        private PlayerConfig EnsurePlayerConfig(ulong id)
        {
            if (!_PlayerCfgs.ContainsKey(id))
            {
                _PlayerCfgs[id] = new PlayerConfig();
            }
            return _PlayerCfgs[id];
        }

        private ExternalViewPlayer? EnsurePlayer(ulong id)
        {
            if (!_Players.ContainsKey(id))
            {
                var csPlayer = _Api.GetPlayer(id);
                if (csPlayer == null)
                    return null;

                _Players[id] = new ExternalViewPlayer(_Api, csPlayer, EnsurePlayerConfig(id));
            }
            return _Players[id];
        }

        private void RemovePlayerCamera(ulong id)
        {
            if (!_Players.ContainsKey(id))
                return;

            var playerCamera = _Players[id];
            playerCamera.OnRemove();
            _Players.Remove(id);
        }

        private float ValidateCameraDistance(float cameraDistance, ulong id)
        {
            if (cameraDistance == 0)
                return Consts.DefaultCameraDistance;

            var clampedCameraDistance = Math.Clamp(
                cameraDistance,
                _Api.ConVars.ThirdPersonMinDistance,
                _Api.ConVars.ThirdPersonMaxDistance
            );

            if (clampedCameraDistance != cameraDistance)
            {
                _Api.GetPlayer(id)?.PrintToChat(
                    "ExternalView.ThirdPerson.CameraDistance.OutOfRange",
                    _Api.ConVars.ThirdPersonMinDistance,
                    _Api.ConVars.ThirdPersonMaxDistance
                );
            }

            return clampedCameraDistance;
        }
    }
}
