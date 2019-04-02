﻿using UnityEngine;

namespace NetworkSkins.GUI
{
    public class ListItem
    {
        public readonly string ID;
        public readonly string DisplayName;
        public readonly Texture2D Thumbnail;
        public bool IsSelected;
        public bool IsFavourite;

        public ListItem(string id, string displayName, Texture2D thumbnail, bool isSelected, bool isFavourite) {
            ID = id;
            DisplayName = displayName;
            Thumbnail = thumbnail;
            IsSelected = isSelected;
            IsFavourite = isFavourite;
        }
    }
}