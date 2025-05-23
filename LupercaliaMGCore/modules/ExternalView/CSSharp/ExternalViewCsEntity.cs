using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System.Drawing;
using System.Numerics;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal class ExternalViewCsEntity : IExternalViewCsEntity
    {
        public delegate CBaseEntity GetEntityDelegate();

        private readonly GetEntityDelegate _GetEntity;

        private RenderMode_t? _Hide_LastRenderMode;
        private Color? _Hide_LastRenderColor;

        private CBaseEntity _Entity => _GetEntity();

        public ExternalViewCsEntity(CBaseEntity entity) : this(() => entity)
        {
        }

        public ExternalViewCsEntity(GetEntityDelegate getEntity)
        {
            _GetEntity = getEntity;
        }

        public bool IsValid => _Entity.IsValid;

        public uint HandleRaw => _Entity.EntityHandle.Raw;

        public Vector3 Origin => MathUtils.ToVector3(_Entity.AbsOrigin);
        public Vector3 Rotation => MathUtils.ToVector3(_Entity.AbsRotation);
        public Vector3 Velocity => MathUtils.ToVector3(_Entity.AbsVelocity);

        public void Teleport(Vector3? origin, Vector3? angle, Vector3? velocity)
        {
            _Entity.Teleport(
                MathUtils.ToVectorOrNull(origin),
                MathUtils.ToQAngleOrNull(angle),
                MathUtils.ToVectorOrNull(velocity)
            );
        }

        public void Remove()
        {
            _Entity.Remove();
        }

        public bool IsVisible
        {
            get => !_Hide_LastRenderMode.HasValue;
            set
            {
                if (value != _Hide_LastRenderMode.HasValue)
                    return;

                var modelEntity = _Entity as CBaseModelEntity;
                if (modelEntity == null)
                    return;

                if (value)
                {
                    modelEntity.RenderMode = _Hide_LastRenderMode!.Value;
                    modelEntity.Render = _Hide_LastRenderColor!.Value;

                    _Hide_LastRenderMode = null;
                    _Hide_LastRenderColor = null;
                }
                else
                {
                    _Hide_LastRenderMode = modelEntity.RenderMode;
                    _Hide_LastRenderColor = modelEntity.Render;

                    modelEntity.RenderMode = RenderMode_t.kRenderTransAlpha;
                    modelEntity.Render = Color.FromArgb(0, 255, 255, 255);
                }
                Utilities.SetStateChanged(modelEntity, "CBaseModelEntity", "m_clrRender");
            }
        }

        public IExternalViewCsEntity? Parent
        {
            set
            {
                if (value == null)
                {
                    _Entity.AcceptInput("SetParent", null, null, "");
                    return;
                }

                var target = value as ExternalViewCsEntity;
                if (target == null)
                    return;

                _Entity.AcceptInput("SetParent", target._Entity, null, "!activator");
            }
        }

        public string Model
        {
            get => _Entity.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? "";
            set {
                var modelEntity = _Entity as CBaseModelEntity;
                if (modelEntity == null)
                    return;

                modelEntity.SetModel(value);
            }
        }

        public bool Equals(IExternalViewCsEntity? other)
        {
            var otherEntity = other as ExternalViewCsEntity;

            if (otherEntity == null)
                return false;

            return _Entity.Handle == otherEntity._Entity.Handle;
        }
    }
}
