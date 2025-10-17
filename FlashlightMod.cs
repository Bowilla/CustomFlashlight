using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BBModMenu;
using MelonLoader;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomFlashlight
{
    public class FlashlightMod : MelonMod
    {
        private Light flashlight;
        private bool toggle_enable = true;
        private float intensity = 3f;
        private Color color = new Color(1,1,1);
        private string toggleKey;
        private float[] custom_light;

        override public void OnLateInitializeMelon()
        {
            MelonLogger.Msg("CustomFlashlight starting to load.");
            
            flashlight = Light.GetLights(LightType.Spot, 0)[0];

            GameObject gameUI = GameObject.Find("GameUI");
            GameUI _gameUI = gameUI.GetComponent<GameUI>();
            List<UIScreen> screens = typeof(GameUI)?.GetField("screens", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(_gameUI) as List<UIScreen>;

            ModMenu _modMenu = screens?.FirstOrDefault(screen => screen is ModMenu) as ModMenu;
            if (_modMenu is null)
            {
                MelonLogger.Msg("ModMenu not found");
                return;
            }

            string categoryName = "Flashlight";
            var flashlightSettings = _modMenu.AddSetting(categoryName);

            // Toggles
            var cameraGlow = _modMenu.CreateToggle(categoryName, "CameraGlow", true);
            cameraGlow.RegisterValueChangedCallback(delegate (ChangeEvent<bool> b)
            {
                Camera.main.GetComponentInChildren<Light>().intensity = (b.newValue) ? 1 : 0;
            });

            var key = _modMenu.CreateHotKey(categoryName, "ToggleHotkey", KeyCode.L);
            toggleKey = key.Value;
            key.OnChanged += newKey =>
            {
                MelonLogger.Msg($"Flashlight Toggle Hotkey : {newKey}");
                toggleKey = newKey;
            };


            // Custom light sliders
            var intensitySlider = _modMenu.CreateSlider(categoryName, "Intensity", 0, 10, 3, false);
            intensitySlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                intensity = f.newValue;
                if (flashlight.intensity > 0f) flashlight.intensity = intensity;
                custom_light[0] = f.newValue;
            });
            var rangeSlider = _modMenu.CreateSlider(categoryName, "Range", 0, 500, 15, true);
            rangeSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                flashlight.range = f.newValue;
                custom_light[1] = f.newValue;
            });
            var angleSlider = _modMenu.CreateSlider(categoryName, "Angle", 0, 160, 120, true);
            angleSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                flashlight.spotAngle = f.newValue;
                custom_light[2] = f.newValue;
            });


            // RGB sliders
            var rSlider = _modMenu.CreateSlider(categoryName, "Red", 0, 1, 1, false);
            rSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                color.r = f.newValue;
                flashlight.color = color;
                custom_light[3] = f.newValue;
            });
            var gSlider = _modMenu.CreateSlider(categoryName, "Green", 0, 1, 1, false);
            gSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                color.g = f.newValue;
                flashlight.color = color;
                custom_light[4] = f.newValue;
            });
            var bSlider = _modMenu.CreateSlider(categoryName, "Blue", 0, 1, 1, false);
            bSlider.RegisterValueChangedCallback(delegate (ChangeEvent<float> f)
            {
                color.b = f.newValue;
                flashlight.color = color;
                custom_light[5] = f.newValue;
            });

            var sliderGroup = _modMenu.CreateGroup("CustomLightSliders");

            List<string> lightOptions = new List<string>()
            {
                "Default Flashlight",
                "Cheap Flashlight",
                "Cold Headlight",
                "Warm Headlight",
                "Night Vision Goggles",
                "UV Flashlight",
                "Builder's Super Torch",
                "Custom Flashlight",
            };
            Dictionary<string, float[]> lightValues = new Dictionary<string, float[]>
            {
                { "Default Flashlight", new float[6]{ 3f, 15f, 120f, 1f, 1f, 1f } },
                { "Cheap Flashlight", new float[6]{ 4f, 5f, 85f, 1f, 1f, 1f } },
                { "Cold Headlight", new float[6]{ 3f, 150f, 120f, 0.7f, 0.7f, 1f } },
                { "Warm Headlight", new float[6]{ 2f, 150f, 120f, 1f, 0.7f, 0.5f } },
                { "Night Vision Goggles", new float[6]{ 3f, 500f, 160f, 0.3f, 1f, 0.3f } },
                { "UV Flashlight", new float[6]{ 10f, 15f, Camera.main.fieldOfView, 0.3f, 0f, 1f } },
                { "Builder's Super Torch", new float[6]{ 2f, 500f, 160f, 1f, 1f, 1f } },
                { "Custom Flashlight", new float[6]{ intensitySlider.value, rangeSlider.value, angleSlider.value, rSlider.value, gSlider.value, bSlider.value } },
            };

            var presets = _modMenu.CreateCarousel(categoryName, "FlashlightPresets", lightOptions, (_key) =>
            {
                MelonLogger.Msg("Flashlight changed to " + _key);

                lightValues.TryGetValue(_key, out float[] val);

                flashlight.intensity = intensity = val[0];
                flashlight.range = val[1];
                flashlight.spotAngle = val[2];
                flashlight.color = new Color(val[3], val[4], val[5]);

                if (_key == "Custom Flashlight")
                {
                    custom_light = val;

                    intensitySlider.value = val[0];
                    rangeSlider.value = val[1];
                    angleSlider.value = val[2];
                    rSlider.value = val[3];
                    gSlider.value = val[4];
                    bSlider.value = val[5];

                    flashlightSettings.Add(sliderGroup);
                }
                else
                {
                    flashlightSettings.Remove(sliderGroup);
                }

            }, "Default Flashlight");


            var togglesGroup = _modMenu.CreateGroup("Toggles");

            var toggleKeyWrapper = _modMenu.CreateWrapper();
            toggleKeyWrapper.Add(_modMenu.CreateLabel("Flashlight Power Button"));
            toggleKeyWrapper.Add(key.Root);

            var cameraGlowWrapper = _modMenu.CreateWrapper();
            cameraGlowWrapper.Add(_modMenu.CreateLabel("Enable Camera Glow"));
            cameraGlowWrapper.Add(cameraGlow);

            var presetWrapper = _modMenu.CreateWrapper();
            presetWrapper.Add(_modMenu.CreateLabel("Flashlight Presets"));
            presetWrapper.Add(presets.Root);

            togglesGroup.Add(toggleKeyWrapper);
            togglesGroup.Add(cameraGlowWrapper);
            togglesGroup.Add(presetWrapper);



            var intensityWrapper = _modMenu.CreateWrapper();
            intensityWrapper.Add(_modMenu.CreateLabel("Intensity"));
            intensityWrapper.Add(intensitySlider);

            var rangeWrapper = _modMenu.CreateWrapper();
            rangeWrapper.Add(_modMenu.CreateLabel("Range"));
            rangeWrapper.Add(rangeSlider);

            var angleWrapper = _modMenu.CreateWrapper();
            angleWrapper.Add(_modMenu.CreateLabel("Angle"));
            angleWrapper.Add(angleSlider);

            var rWrapper = _modMenu.CreateWrapper();
            rWrapper.Add(_modMenu.CreateLabel("Red"));
            rWrapper.Add(rSlider);

            var gWrapper = _modMenu.CreateWrapper();
            gWrapper.Add(_modMenu.CreateLabel("Green"));
            gWrapper.Add(gSlider);

            var bWrapper = _modMenu.CreateWrapper();
            bWrapper.Add(_modMenu.CreateLabel("Blue"));
            bWrapper.Add(bSlider);

            sliderGroup.Add(intensityWrapper);
            sliderGroup.Add(rangeWrapper);
            sliderGroup.Add(angleWrapper);
            sliderGroup.Add(rWrapper);
            sliderGroup.Add(gWrapper);
            sliderGroup.Add(bWrapper);


            flashlightSettings.Add(togglesGroup);


            
            lightValues.TryGetValue(presets.Value, out float[] _val);

            flashlight.intensity = intensity = _val[0];
            flashlight.range = _val[1];
            flashlight.spotAngle = _val[2];
            flashlight.color = new Color(_val[3], _val[4], _val[5]);

            if (presets.Value == "Custom Flashlight")
            {
                custom_light = _val;

                flashlightSettings.Add(sliderGroup);
            }

            Camera.main.GetComponentInChildren<Light>().intensity = (cameraGlow.value) ? 1 : 0;
            
        }


        public override void OnUpdate()
        {
            if (!toggle_enable) return;

            if (Utils.IsHotkeyPressed(toggleKey))
            {

                if (flashlight.intensity > 0f)
                {
                    flashlight.intensity = 0f;
                }
                else
                {
                    flashlight.intensity = intensity;
                }
            }

        }
    }
}
