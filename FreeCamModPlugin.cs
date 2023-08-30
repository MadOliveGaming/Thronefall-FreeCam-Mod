using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using System.Data.Common;
using UnityEngine;
using UnityEngine.LowLevel;

namespace FreeCamMod
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class FreeCamModPlugin : BaseUnityPlugin
    {
        private const string MyGUID = "com.MadOliveGaming.FreeCamMod";
        private const string PluginName = "FreeCamMod";
        private const string VersionString = "0.1.0";

        public float maxZoom = 99f;
        public float minZoom = 3f;
        public float zoomStep = 1f;
        public bool FreecamOn = false;
        public float camFollowSpeed = 60f;
        public bool hasPlayer = false;

        public Vector3 orCam = new Vector3(0, 0, 0);
        public Vector3 orCamRig = new Vector3(0, 0, 0);
        public float defaultOrtographic = 0f;

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        private void Awake()
        {
            // Apply patches
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");

            // Set up our static Log
            Log = Logger;
        }

        private void Update()
        {
            // Simply look for player to make sure we are in a scene where freecam should be enabled and not in a main menu or something
            var player = GameObject.Find("Horse_LOD1");

            if (player != null)
            {
                Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>(); // FindFirstObjectByType<Camera>();
                Camera ui = GameObject.Find("UI and HP Bar Cam").GetComponent<Camera>();
                CameraRig cameraRig = FindFirstObjectByType<CameraRig>();
                Player input = ReInput.players.GetPlayer(0);

                if (!this.hasPlayer)
                {
                    this.defaultOrtographic = camera.orthographicSize;
                    this.hasPlayer = true;
                }

                this.Freecam(camera, ui, cameraRig, input);
                this.ScrollZoom(camera, ui, input);
            } else
            {
                this.hasPlayer = false;
            }
        }

        void Freecam(Camera camera, Camera ui, CameraRig cameraRig, Player input)
        {
            if (input.controllers.hasKeyboard && input.controllers.Keyboard.GetKey(KeyCode.F))
            {
                
                if (!FreecamOn)
                {
                    this.orCam = camera.transform.position;
                    this.orCamRig = cameraRig.transform.position;

                    this.FreecamOn = true;
                    Logger.LogInfo("Freecam Enabled");  
                }

                int width = Screen.width;
                int minWidht = (width / 2) + (width / 8 * 3);
                int maxWidth = (width / 2) - (width / 8 * 3);
                int height = Screen.height;
                int minHeight = (height / 2) + (height / 8 * 3);
                int maxHeight = (height / 2) - (height / 8 * 3);

                Vector3 mousepos = Input.mousePosition;
                float xMove = 0f;
                float yMove = 0f;

                if (mousepos.x > minWidht)
                {
                    xMove = this.camFollowSpeed;
                }
                else if (mousepos.x < maxWidth)
                {
                    xMove = this.camFollowSpeed * -1;
                }

                if (mousepos.y > minHeight)
                {
                    yMove = this.camFollowSpeed;
                }
                else if (mousepos.y < maxHeight)
                {
                    yMove = this.camFollowSpeed * -1;
                }

                camera.transform.Translate(xMove * Time.deltaTime, yMove * Time.deltaTime, 0f);
                ui.transform.Translate(xMove * Time.deltaTime, yMove * Time.deltaTime, 0f);
            }
            else if (FreecamOn)
            {
                this.FreecamOn = false;

                float newX = cameraRig.transform.position.x + (this.orCam.x - this.orCamRig.x);
                float newY = cameraRig.transform.position.y + (this.orCam.y - this.orCamRig.y);
                float newZ = cameraRig.transform.position.z + (this.orCam.z - this.orCamRig.z);

                camera.transform.position = new Vector3(newX, newY, newZ);
                ui.transform.position = new Vector3(newX, newY, newZ);

                Logger.LogInfo("Freecam Disabled");
            }
        }

        void ScrollZoom(Camera camera, Camera ui, Player input)
        {
            if (input.controllers.hasMouse)
            {
                float scroll = input.controllers.Mouse.GetAxisRaw(2);

                if (scroll < 0f && camera.orthographicSize + this.zoomStep <= this.maxZoom)
                {
                    camera.orthographicSize += this.zoomStep;
                    ui.orthographicSize += this.zoomStep;
                    Logger.LogInfo("Camera zoom changed to: " + camera.orthographicSize);
                }
                else if (scroll > 0f && camera.orthographicSize - this.zoomStep >= this.minZoom)
                {
                    camera.orthographicSize -= this.zoomStep;
                    ui.orthographicSize -= this.zoomStep;
                    Logger.LogInfo("Camera zoom changed to: " + camera.orthographicSize);
                }

                // Reset to default zoom
                if (input.controllers.Mouse.GetButtonDown(2))
                {
                    camera.orthographicSize = this.defaultOrtographic;
                    ui.orthographicSize = this.defaultOrtographic;
                    Logger.LogInfo("Camera zoom reset to: " + this.defaultOrtographic);
                }
            }
        }
    }
}
