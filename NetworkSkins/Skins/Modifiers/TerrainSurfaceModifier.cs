﻿using ColossalFramework.IO;
using NetworkSkins.Skins.Serialization;

namespace NetworkSkins.Skins.Modifiers
{
    public class TerrainSurfaceModifier : NetworkSkinModifier
    {
        public readonly NetworkGroundType GroundType;

        public TerrainSurfaceModifier(NetworkGroundType groundType) : base(NetworkSkinModifierType.TerrainSurface)
        {
            GroundType = groundType;
        }

        public override void Apply(NetworkSkin skin)
        {
            if (GroundType == NetworkGroundType.Pavement)
            {
                skin.m_createPavement = true;
                skin.m_createGravel = false;
                skin.m_createRuining = false;
            }
            else if (GroundType == NetworkGroundType.Gravel)
            {
                skin.m_createPavement = false;
                skin.m_createGravel = true;
                skin.m_createRuining = false;
            }
            else if (GroundType == NetworkGroundType.Ruined)
            {
                skin.m_createPavement = false;
                skin.m_createGravel = false;
                skin.m_createRuining = true;
            }
            else if (GroundType == NetworkGroundType.None)
            {
                skin.m_createPavement = false;
                skin.m_createGravel = false;
                skin.m_createRuining = false;
            }
        }

        #region Serialization
        protected override void SerializeImpl(DataSerializer s)
        {
            s.WriteUInt8((uint)GroundType);
        }

        public static TerrainSurfaceModifier DeserializeImpl(DataSerializer s, NetworkSkinLoadErrors errors)
        {
            var groundType = (NetworkGroundType)s.ReadUInt8();

            return new TerrainSurfaceModifier(groundType);
        }
        #endregion

        #region Equality
        protected bool Equals(TerrainSurfaceModifier other)
        {
            return GroundType == other.GroundType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((TerrainSurfaceModifier) obj);
        }

        public override int GetHashCode()
        {
            return (int) GroundType;
        }
        #endregion
    }

    public enum NetworkGroundType
    {
        Unchanged = 0,
        Pavement = 1,
        Gravel = 2,
        Ruined = 3,
        /// <summary>
        /// None only works when the network does not clip terrain. The option should be hidden for network which do clip terrain
        /// </summary>
        None = 4
    }
}