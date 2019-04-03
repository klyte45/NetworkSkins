﻿using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;

namespace NetworkSkins.Skins.Serialization
{
    public class NetworkSkinDataContainer : IDataContainer
    {
        public const int Version = 0;

        public readonly NetworkSkinLoadErrors Errors = new NetworkSkinLoadErrors();

        public List<NetworkSkin> AppliedSkins;
        public NetworkSkin[] SegmentSkins;
        public NetworkSkin[] NodeSkins;

        public void Serialize(DataSerializer s)
        {
            AppliedSkins = NetworkSkinManager.instance.AppliedSkins;
            SegmentSkins = NetworkSkinManager.SegmentSkins;
            NodeSkins = NetworkSkinManager.NodeSkins;

            if (AppliedSkins.Count >= ushort.MaxValue - 1)
            {
                throw new Exception("Too many applied skins, cannot serialize!");
            }

            s.WriteInt32(AppliedSkins.Count);
            foreach (var appliedSkin in AppliedSkins)
            {
                appliedSkin.Serialize(s);
            }

            s.WriteInt32(SegmentSkins.Length);
            foreach (var segmentSkin in SegmentSkins)
            {
                s.WriteUInt16(IndexOfAppliedSkin(segmentSkin));
            }

            s.WriteInt32(NodeSkins.Length);
            foreach (var nodeSkin in NodeSkins)
            {
                s.WriteUInt16(IndexOfAppliedSkin(nodeSkin));
            }
        }

        public ushort IndexOfAppliedSkin(NetworkSkin skin)
        {
            if (skin != null)
            {
                for (ushort j = 0; j < AppliedSkins.Count; j++)
                {
                    if (ReferenceEquals(skin, AppliedSkins[j]))
                    {
                        return (ushort)(j + 1);
                    }
                }
            }
            return 0;
        }

        public void Deserialize(DataSerializer s)
        {
            // this must be set here, as the constructor is not called on deserialization
            AppliedSkins = NetworkSkinManager.instance.AppliedSkins;
            SegmentSkins = NetworkSkinManager.SegmentSkins;
            NodeSkins = NetworkSkinManager.NodeSkins;

            var appliedSkinsLength = s.ReadInt32();
            AppliedSkins.Clear();
            for (var i = 0; i < appliedSkinsLength; i++)
            {
                AppliedSkins.Add(NetworkSkin.Deserialize(s, Errors));
            }

            var segmentSkinsLength = s.ReadInt32();
            for (var i = 0; i < segmentSkinsLength; i++)
            {
                var skinIndex = s.ReadUInt16();
                if (SegmentSkins.Length > i)
                {
                    SegmentSkins[i] = AppliedSkinForIndex(skinIndex);
                }
            }

            var nodeSkinsLength = s.ReadInt32();
            for (var i = 0; i < nodeSkinsLength; i++)
            {
                var skinIndex = s.ReadUInt16();
                if (NodeSkins.Length > i)
                {
                    NodeSkins[i] = AppliedSkinForIndex(skinIndex);
                }
            }
        }

        public NetworkSkin AppliedSkinForIndex(uint index)
        {
            if (index > 0 && AppliedSkins.Count > index - 1)
            {
                return AppliedSkins[(int)(index - 1)];
            }
            else
            {
                return null;
            }
        }

        // validation for the data:
        // This is important if the player loaded the savegame without the mod for a while
        public void AfterDeserialize(DataSerializer s)
        {
            var netManager = NetManager.instance;

            // Remove invalid segment data
            var removedSegmentCount = 0;
            for (var i = 0; i < SegmentSkins.Length; i++)
            {
                if (SegmentSkins[i] == null)
                {
                    continue;
                }

                if (!netManager.m_segments.m_buffer[i].m_flags.IsFlagSet(NetSegment.Flags.Created))
                {
                    SegmentSkins[i] = null;
                    removedSegmentCount++;
                    continue;
                }

                var segmentPrefab = netManager.m_segments.m_buffer[i].Info;
                if (segmentPrefab != SegmentSkins[i].Prefab)
                {
                    SegmentSkins[i] = null;
                    removedSegmentCount++;
                    continue;
                }

                SegmentSkins[i].UseCount++;
            }

            // remove invalid node data
            var removedNodeCount = 0;
            for (var i = 0; i < NodeSkins.Length; i++)
            {
                if (NodeSkins[i] == null)
                {
                    continue;
                }

                if (!netManager.m_nodes.m_buffer[i].m_flags.IsFlagSet(NetNode.Flags.Created))
                {
                    NodeSkins[i] = null;
                    removedNodeCount++;
                    continue;
                }

                var nodePrefab = netManager.m_nodes.m_buffer[i].Info;
                if (nodePrefab != NodeSkins[i].Prefab)
                {
                    NodeSkins[i] = null;
                    removedNodeCount++;
                    continue;
                }

                NodeSkins[i].UseCount++;
            }

            // remove unused and invalid skins
            var removedSkinCount = 0;
            for (var i = AppliedSkins.Count - 1; i >= 0; i--)
            {
                if (AppliedSkins[i] == null || AppliedSkins[i].UseCount <= 0)
                {
                    AppliedSkins[i]?.Destroy();
                    AppliedSkins.RemoveAt(i);
                    removedSkinCount++;
                }
            }

            Debug.Log($"NS: Savegame data validation succesful! {removedSegmentCount} | {removedNodeCount} | {removedSkinCount}");
            Debug.Log($"NS: {AppliedSkins.Count} applied skins");
        }
    }
}
