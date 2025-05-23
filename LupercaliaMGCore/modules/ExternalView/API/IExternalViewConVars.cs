namespace LupercaliaMGCore.modules.ExternalView.API
{
    public interface IExternalViewConVars
    {
        float ThirdPersonMinDistance { get; }
        float ThirdPersonMaxDistance { get; }
        float ModelViewCameraSpeed { get; }
        float ModelViewCameraAltSpeed { get; }
        float ModelViewCameraRadius { get; }
        float FreeCameraSpeed { get; }
        float FreeCameraAltSpeed { get; }
        bool IsObserverViewEnabled { get; }
        bool IsAdminPrevilegesEnabled { get; }
    }
}
