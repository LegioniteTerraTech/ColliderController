using System;
using System.Reflection;
using Harmony;
using ModHelper.Config;
using UnityEngine;
using Nuterra.NativeOptions;

namespace TT_ColliderController
{
    //This Mod exists solely to increase frames under load of a ton of blocks, and nothing more.  DO NOT USE WHILE BUILDING!
    //  We take no responsability for the following:
    //  Invincible Techs, unlimited power, collider abuse, crash spam.

    public class KickStart
    {
        //This kickstarts the whole mod.  
        // We add in multiple things including makeshift colliders that only enable when the mod is active.

        //Let hooks happen i guess
        const string ModName = "ColliderController";

        //Make a Config File to store user preferences
        public static ModConfig _thisModConfig;

        //Variables
        public static bool colliderGUIActive = false; //Is the display up
        public static bool collidersEnabled = true; //do we obliterate all colliders in the world
        public static bool updateToggle = false;//Just update the darn thing; //do we obliterate all colliders in the world

        public static KeyCode hotKey;
        public static int keyInt = 93;//default to be ]
        public static OptionKey GUIMenuHotKey;
        public static OptionToggle UIActive;

        public static void Main()
        {
            //Where the fun begins

            //Initiate the madness
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("Legionite.collidercommand.core");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            GUIColliderController.Initiate();


            Debug.Log("\nCOLLIDER CONTROLLER: Config Loading");
            ModConfig thisModConfig = new ModConfig();
            Debug.Log("\nCOLLIDER CONTROLLER: Config Loaded.");

            thisModConfig.BindConfig<KickStart>(null, "keyInt");
            hotKey = (KeyCode)keyInt;
            thisModConfig.BindConfig<KickStart>(null, "colliderGUIActive");
            _thisModConfig = thisModConfig;

            //Nativeoptions
            var ColliderProperties = ModName + " - Collider Menu Settings";
            GUIMenuHotKey = new OptionKey("GUI Menu button", ColliderProperties, hotKey);
            GUIMenuHotKey.onValueSaved.AddListener(() => { keyInt = (int)(hotKey = GUIMenuHotKey.SavedValue); });

            UIActive = new OptionToggle("Collider GUI Active", ColliderProperties, KickStart.colliderGUIActive);
            UIActive.onValueSaved.AddListener(() => { KickStart.colliderGUIActive = UIActive.SavedValue; });
        }
    }

    internal class Patches
    {
        //SHOVE IN ModuleRemoveCollider in EVERYTHING!
        [HarmonyPatch(typeof(TankBlock))]
        [HarmonyPatch("OnPool")]//On Block Creation
        private class PatchBlock
        {
            private static void Postfix(TankBlock __instance)
            {
                var wEffect = __instance.gameObject.AddComponent<ColliderCommander.ModuleRemoveColliders>();
                wEffect.TankBlock = __instance;
                /*
                if (__instance.BlockCategory == BlockCategories.Flight)
                {
                    var component = __instance.GetComponentInChildren<FanJet>();
                    if (component != null)
                    {
                    }
                }
                */
            }
        }


        [HarmonyPatch(typeof(TankBlock))]
        [HarmonyPatch("OnRecycle")]
        private class TankBlockRecycle
        {
            private static void Postfix(TankBlock __instance)
            {
                try
                {
                    //__instance.gameObject.GetComponent<ColliderCommander.ModuleRemoveColliders>().TryRemoveSurface();
                }
                catch { }
            }
        }


        [HarmonyPatch(typeof(Tank))]
        [HarmonyPatch("OnPool")]
        private class PatchTank
        {
            private static void Postfix(Tank __instance)
            {
                var wEffect = __instance.gameObject.AddComponent<ColliderCommander.RemoveColliderTank>();
                wEffect.Subscribe(__instance);
            }
        }
    }
        public class GUIColliderController : MonoBehaviour
    {
        //We handle the GUI for the ColliderController system here, toggle when to run real colliders or not.

        static private bool GUIIsActive = false;
        static private Rect MainWindow = new Rect(200, 0, 200, 130);
        static public GameObject GUIWindow;

        static private void GUIHandler(int ID)
        {
            //Toggle if the colliders be gone
            KickStart.collidersEnabled = GUI.Toggle(new Rect(15, 40, 100, 20), KickStart.collidersEnabled, "Colliders Switch");
            KickStart.updateToggle = GUI.Toggle(new Rect(15, 60, 100, 20), KickStart.updateToggle, "Update Colliders");
            GUI.Label(new Rect(20, 85, 120, 20), "Below is only SP");
            ColliderCommander.AFFECT_ALL_TECHS = GUI.Toggle(new Rect(15, 100, 100, 20), ColliderCommander.AFFECT_ALL_TECHS, "ALL COLLIDERS");
            GUI.DragWindow();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KickStart.hotKey))
            {
                GUIIsActive = !GUIIsActive;
                GUIWindow.SetActive(GUIIsActive);
                if (!GUIIsActive)
                {
                    Debug.Log("\nCOLLIDER CONTROLLER: Writing to Config...");
                    KickStart._thisModConfig.WriteConfigJsonFile();
                }
            }
            if (KickStart.collidersEnabled)
            {
                ColliderCommander.areAllPossibleCollidersDisabled = false;
            }
            else
            {
                ColliderCommander.areAllPossibleCollidersDisabled = true;
            }
        }

        public static void Initiate()
        {
            new GameObject("GUIColliderController").AddComponent<GUIColliderController>();
            GUIWindow = new GameObject();
            GUIWindow.AddComponent<GUIDisplay>();
            GUIWindow.SetActive(false);
        }
        internal class GUIDisplay : MonoBehaviour
        {
            private void OnGUI()
            {
                if (GUIIsActive)
                {
                    MainWindow = GUI.Window(2199, MainWindow, GUIHandler, "Player Collider Control");
                }
            }
        }
    }
}
