using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Player;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public interface ICameraContext
    {
        public IExternalViewCsApi Api { get; }
        public IExternalViewCsEntity? CameraEntity { get; }
        public IExternalViewCsPlayer Player { get; }
        public PlayerConfig Config { get; }
    }
}
