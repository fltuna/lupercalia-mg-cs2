using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.API
{
    /// <summary>
    /// A wrapper interface for BaseEntity class.
    /// </summary>
    public interface IExternalViewCsEntity : IEquatable<IExternalViewCsEntity>
    {
        bool IsValid { get; }

        uint HandleRaw { get; }

        Vector3 Origin { get; }
        Vector3 Rotation { get; }
        Vector3 Velocity { get; }

        void Teleport(Vector3? origin, Vector3? angle, Vector3? velocity);

        void Remove();

        bool IsVisible { get; set; }

        IExternalViewCsEntity? Parent { set; }
        string Model { get; set; }
    }
}
