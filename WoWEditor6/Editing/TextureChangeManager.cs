﻿using System;
using SharpDX;
using WoWEditor6.Scene;
using WoWEditor6.Utils;

namespace WoWEditor6.Editing
{
    enum TextureFalloffMode
    {
        Flat,
        Linear,
        Trigonometric
    }

    class TextureChangeParameters
    {
        public Vector3 Center;
        public string Texture;
        public float InnerRadius;
        public float OuterRadius;
        public float Amount;
        public float TargetValue;
        public TextureFalloffMode FalloffMode;
        public bool IsInverted;
    }

    class TextureChangeManager
    {
        public static TextureChangeManager Instance { get; private set; }
        
        public float Amount { get; set; }
        public float TargetValue { get; set; }
        public string SelectedTexture { get; set; }

        public TextureFalloffMode FalloffMode { get; set; }

        static TextureChangeManager()
        {
            Instance = new TextureChangeManager();
        }

        private TextureChangeManager()
        {
            FalloffMode = TextureFalloffMode.Linear;
            Amount = 0.0f;
            TargetValue = 255.0f;
            SelectedTexture = string.Empty;
        }

        public void OnChange(TimeSpan diff)
        {
            bool inverted;
            if (CheckRequirements(out inverted) == false)
                return;

            var parameters = new TextureChangeParameters
            {
                Center = EditManager.Instance.MousePosition,
                InnerRadius = EditManager.Instance.InnerRadius,
                OuterRadius = EditManager.Instance.OuterRadius,
                Texture = SelectedTexture,
                //Amount = 4 + Amount,
                Amount = Amount / 40.0f,
                FalloffMode = FalloffMode,
                TargetValue = TargetValue,
                IsInverted = inverted
            };

            WorldFrame.Instance.MapManager.OnTextureTerrain(parameters);
        }

        private bool CheckRequirements(out bool isInverted)
        {
            isInverted = false;

            if (EditManager.Instance.IsTerrainHovered == false)
                return false;

            var bindings = Settings.KeyBindings.Instance;
            var state = new byte[256];
            UnsafeNativeMethods.GetKeyboardState(state);

            if (!KeyHelper.IsKeyDown(state, System.Windows.Forms.Keys.LButton))
            {
                return false;
            }

            if (KeyHelper.AreKeysDown(state, bindings.Interaction.Edit) == false &&
                KeyHelper.AreKeysDown(state, bindings.Interaction.EditInverse) == false)
                return false;

            if (KeyHelper.AreKeysDown(state, bindings.Interaction.EditInverse))
                isInverted = true;

            return true;
        }
    }
}
