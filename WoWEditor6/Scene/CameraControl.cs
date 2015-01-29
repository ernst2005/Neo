﻿using System;
using System.Drawing;
using System.Windows.Forms;
using WoWEditor6.Settings;
using WoWEditor6.UI;

namespace WoWEditor6.Scene
{
    /// <summary>
    /// TODO: CameraControl should use some sort of key bindings and not hard coded keys
    /// </summary>
    class CameraControl
    {
        private readonly MainWindow mWindow;
        private Point mLastCursorPos;
        private DateTime mLastUpdate = DateTime.Now;

        private float speedFactor = 100.0f;
        private float speedFactorWheel = 0.5f;
        private float turnFactor = 0.2f;
        public bool Invert { get; set; } = false;

        public CameraControl(MainWindow window)
        {
            mWindow = window;
        }

        public void Update()
        {
            if (mWindow.Focused == false || WorldFrame.Instance.State != AppState.World)
            {
                mLastCursorPos = Cursor.Position;
                mLastUpdate = DateTime.Now;
                return;
            }

            var keyState = new byte[256];
            UnsafeNativeMethods.GetKeyboardState(keyState);

            var positionChanged = false;
            var updateTerrain = false;
            var cam = WorldFrame.Instance.ActiveCamera;
            var diff = (float)(DateTime.Now - mLastUpdate).TotalSeconds;

            var camBind = KeyBindings.Instance.Camera;

            if (AreKeysDown(keyState, camBind.Forward))
            {
                positionChanged = true;
                updateTerrain = true;
                cam.MoveForward(diff * speedFactor);
            }

            if (AreKeysDown(keyState, camBind.Backward))
            {
                positionChanged = true;
                updateTerrain = true;
                cam.MoveForward(-diff * speedFactor);
            }

            if (AreKeysDown(keyState, camBind.Right))
            {
                positionChanged = true;
                updateTerrain = true;
                cam.MoveRight(diff * speedFactor);
            }

            if (AreKeysDown(keyState, camBind.Left))
            {
                positionChanged = true;
                updateTerrain = true;
                cam.MoveRight(-diff * speedFactor);
            }

            if (AreKeysDown(keyState, camBind.Up))
            {
                positionChanged = true;
                cam.MoveUp(diff * speedFactor);
            }

            if (AreKeysDown(keyState, camBind.Down))
            {
                positionChanged = true;
                cam.MoveUp(-diff * speedFactor);
            }

            if (IsDown(keyState, Keys.RButton))
            {
                var curPos = Cursor.Position;
                var dx = curPos.X - mLastCursorPos.X;
                var dy = curPos.Y - mLastCursorPos.Y;

                if (dx != 0)
                    cam.Yaw(dx * turnFactor * (Invert ? -1 : 1));
                if (dy != 0)
                    cam.Pitch(dy * turnFactor * (Invert ? -1 : 1));
            }

            if (positionChanged)
                WorldFrame.Instance.MapManager.UpdatePosition(cam.Position, updateTerrain);

            mLastUpdate = DateTime.Now;
            mLastCursorPos = Cursor.Position;
        }

        private static bool AreKeysDown(byte[] keyState, Keys[] keys)
        {
            if (keys.Length == 0)
                return false;

            foreach(var key in keys)
            {
                if ((int) key >= keyState.Length)
                    return false;

                if (((keyState[(int) key]) & 0x80) == 0)
                    return false;
            }

            return true;
        }

        private static bool IsDown(byte[] keyState, Keys key)
        {
            return ((keyState[(int)key]) & 0x80) != 0;
        }

        public void HandleMouseWheel(int delta)
        {
            var keyState = new byte[256];
            UnsafeNativeMethods.GetKeyboardState(keyState);

            if (IsDown(keyState, Keys.RButton))
            {
                var cam = WorldFrame.Instance.ActiveCamera;
                cam.MoveForward(delta * speedFactorWheel);
                WorldFrame.Instance.MapManager.UpdatePosition(cam.Position, true);
            }
        }
    }
}