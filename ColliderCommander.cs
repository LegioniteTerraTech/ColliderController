using System;
using UnityEngine;

namespace TT_ColliderController
{
    class ColliderCommander
    {
        //The baseline controller for all colliders on a Tech.
        //  This mod is not intended for use in MP (will still let other players hit you fine) but if there is significant demand for it, I guess I can try netcode for this.

        // GLOBAL Variables
        public static bool areAllPossibleCollidersDisabled = false;
        public static bool AFFECT_ALL_TECHS = false; //togg for lols - also breaks the game if active at startup
        private static bool affectingAlltechs = false; //

        // Store in user preferences
        //int blockNamesCount = 0;
        //string[] loaded;


        public class RemoveColliderTank : MonoBehaviour
        {
            //Attach this to sync all collider removals on a Tech
            public Tank tank;

            // TECH Variables
            private bool lastLocalState = false;
            private static bool ForceUpdate = false;
            private static bool lastTankState = false;
            //private static bool lastPlayerTankState = false;
            //private static bool lastTankBlockState = false;

            //private bool PREPARE_FOR_CLUSTERBODY = false;

            public void Subscribe(Tank tank)
            {
                tank.AttachEvent.Subscribe(AddBlock);
                tank.DetachEvent.Subscribe(RemoveBlock);
                this.tank = tank;
            }
            public void AddBlock(TankBlock tankblock, Tank tank)
            {
                tankblock.GetComponent<ModuleRemoveColliders>().removeColliderTank = this;
            }

            public void RemoveBlock(TankBlock tankblock, Tank tank)
            {
                tankblock.GetComponent<ModuleRemoveColliders>().removeColliderTank = null;
            }

            public void RecursiveClusterBodyHandlerDestroy(Transform grabbedGameObject)
            {
                int childCB = grabbedGameObject.transform.childCount;
                for (int vCB = 0; vCB < childCB; ++vCB)
                {
                    Transform grabbedGameObjectCB = grabbedGameObject.transform.GetChild(vCB);
                    try
                    {
                        Debug.Log("\n(CB) Child " + grabbedGameObjectCB + " child of " + grabbedGameObject);
                        if (ForceUpdate == true && tank.PlayerFocused == true)
                        {
                            grabbedGameObjectCB.GetComponent<ModuleRemoveColliders>().ForceUpdateThis();
                            Debug.Log("FORCE-UPDATED!");
                        }
                        grabbedGameObjectCB.GetComponent<ModuleRemoveColliders>().DestroyCollidersOnBlock();
                    }
                    catch
                    {
                        Debug.Log("(CB) Object " + grabbedGameObjectCB + " in " + grabbedGameObject + " is slippery!");
                        if (grabbedGameObjectCB.transform.childCount >= 1)
                        {
                            Debug.Log("Performing Recursive Action on " + grabbedGameObjectCB + "! Confirmed Children " + grabbedGameObjectCB.transform.childCount);
                            RecursiveClusterBodyHandlerDestroy(grabbedGameObjectCB);
                        }
                    }
                }
            }

            public void RecursiveClusterBodyHandlerReturn(Transform grabbedGameObject)
            {
                int childCB = grabbedGameObject.transform.childCount;
                for (int vCB = 0; vCB < childCB; ++vCB)
                {
                    Transform grabbedGameObjectCB = grabbedGameObject.transform.GetChild(vCB);
                    try
                    {
                        Debug.Log("\n(CB) Child " + grabbedGameObjectCB + " child of " + grabbedGameObject);
                        if (ForceUpdate == true && tank.PlayerFocused == true)
                        {
                            grabbedGameObjectCB.GetComponent<ModuleRemoveColliders>().ForceUpdateThis();
                            Debug.Log("FORCE-UPDATED!");
                        }
                        grabbedGameObjectCB.GetComponent<ModuleRemoveColliders>().ReturnCollidersOnBlock();
                    }
                    catch
                    {
                        Debug.Log("(CB) Object " + grabbedGameObjectCB + " in " + grabbedGameObject + " is slippery!");
                        if (grabbedGameObjectCB.transform.childCount >= 1)
                        {
                            Debug.Log("Performing Recursive Action on " + grabbedGameObjectCB + "! Confirmed Children " + grabbedGameObjectCB.transform.childCount);
                            RecursiveClusterBodyHandlerReturn(grabbedGameObjectCB);
                        }
                    }
                }
            }


            public void Update ()
            {

                if (KickStart.updateToggle)
                {
                    Debug.Log("COLLIDER CONTROLLER: UPDATE REQUEST - manual intervention!");
                    if (areAllPossibleCollidersDisabled == true)
                        lastLocalState = false;
                    else lastLocalState = true;
                    KickStart.updateToggle = false;
                    ForceUpdate = true;
                }
                else if (AFFECT_ALL_TECHS != affectingAlltechs)
                {
                    Debug.Log("COLLIDER CONTROLLER: UPDATE REQUEST - toggled setting!");
                    if (areAllPossibleCollidersDisabled == true)
                        lastLocalState = false;
                    else lastLocalState = true;
                    KickStart.updateToggle = false;
                    affectingAlltechs = AFFECT_ALL_TECHS;
                    //No forced update here - no need to force the updates as that causes mass lag
                }
                else if (lastTankState != tank.FirstUpdateAfterSpawn)
                {   //Block update
                    Debug.Log("COLLIDER CONTROLLER: UPDATE REQUEST - TechTankUpdate!" + tank.FirstUpdateAfterSpawn + lastTankState);
                    if (areAllPossibleCollidersDisabled == true)
                        lastLocalState = false;
                    else lastLocalState = true;
                    lastTankState = tank.FirstUpdateAfterSpawn;
                    //No forced update here - no need to force the updates as that causes mass lag
                }
                /* //Cannot use the below as block detecting operations are unstable as f^bron
                else if (lastTankBlockState != tank.blockman.changed)
                {   //Block update
                    Debug.Log("COLLIDER CONTROLLER: UPDATE REQUEST - TechBlockUpdate!" + tank.blockman.changed + lastTankBlockState);
                    if (areAllPossibleCollidersDisabled == true)
                        lastLocalState = false;
                    else lastLocalState = true;
                    lastTankBlockState = tank.blockman;
                    //No forced update here - no need to force the updates as that causes mass lag
                }
                //Cannot use the below as both player detecting operations (tank.PlayerFocused and tank.IsPlayer) are unstable as f^bron
                else if (lastPlayerTankState != tank.PlayerFocused)
                {
                    Debug.Log("COLLIDER CONTROLLER: UPDATE REQUEST - TechPlayerUpdate!" + tank.PlayerFocused + lastPlayerTankState);
                    if (areAllPossibleCollidersDisabled == true)
                        lastLocalState = false;
                    else lastLocalState = true;
                    lastPlayerTankState = tank.PlayerFocused;
                    //ForceUpdate = true;
                    //No forced update here - no need to force the updates as that causes mass lag
                }
                */

                if (areAllPossibleCollidersDisabled != lastLocalState)
                {
                    Debug.Log("COLLIDER CONTROLLER: Launched Collider Controller for " + gameObject + "!");
                    if ((areAllPossibleCollidersDisabled == true && tank.PlayerFocused == true) || (AFFECT_ALL_TECHS == true && areAllPossibleCollidersDisabled == true && ManNetwork.inst.IsMultiplayer() == false))
                    {
                        Debug.Log("COLLIDER CONTROLLER: Collider Disabling!");
                        try
                        {
                            int child = gameObject.transform.childCount;
                            for (int v = 0; v < child; ++v)
                            {
                                Transform grabbedGameObject = gameObject.transform.GetChild(v);
                                try
                                {
                                    Debug.Log("\nChild " + grabbedGameObject);
                                    if (ForceUpdate == true || KickStart.updateToggle == true)
                                    {
                                        grabbedGameObject.GetComponent<ModuleRemoveColliders>().ForceUpdateThis();
                                        Debug.Log("FORCE-UPDATED!");
                                    }
                                    grabbedGameObject.GetComponent<ModuleRemoveColliders>().DestroyCollidersOnBlock();
                                }
                                catch 
                                { 
                                    Debug.Log("Oop slippery object " + grabbedGameObject + "! Confirmed Children " + grabbedGameObject.transform.childCount);
                                    if (grabbedGameObject.transform.childCount >= 1)
                                    {
                                        Debug.Log("Multiple GameObjects detected from within! - Will Attempt ClusterBodyDecoder!");
                                        RecursiveClusterBodyHandlerDestroy(grabbedGameObject);
                                    }
                                }
                            }
                            //gameObject.GetComponent<ModuleRemoveColliders>().DestroyCollidersOnBlock();
                            Debug.Log("COLLIDER CONTROLLER: SET ALL POSSIBLE COLLIDERS ON " + gameObject + " DISABLED!");
                        }
                        catch
                        {
                            Debug.Log("COLLIDER CONTROLLER: FetchFailiure on " + gameObject + " disable");
                        }
                    }
                    else if (ManNetwork.inst.IsMultiplayer() == false)
                    {
                        Debug.Log("COLLIDER CONTROLLER: Collider Enabling!");
                        try
                        {
                            int child = gameObject.transform.childCount;
                            for (int v = 0; v < child; ++v)
                            {
                                Transform grabbedGameObject = gameObject.transform.GetChild(v);
                                try
                                {
                                    Debug.Log("\nChild " + grabbedGameObject);
                                    if (ForceUpdate == true && tank.PlayerFocused == true)
                                    {
                                        grabbedGameObject.GetComponent<ModuleRemoveColliders>().ForceUpdateThis();
                                        Debug.Log("FORCE-UPDATED!");
                                    }
                                    grabbedGameObject.GetComponent<ModuleRemoveColliders>().ReturnCollidersOnBlock();
                                }
                                catch 
                                { 
                                    Debug.Log("Oop slippery object " + grabbedGameObject + "! Confirmed Children " + grabbedGameObject.transform.childCount);
                                    if (grabbedGameObject.transform.childCount >= 1)
                                    {
                                        Debug.Log("Multiple GameObjects detected from within! - Will Attempt ClusterBodyDecoder!");
                                        RecursiveClusterBodyHandlerReturn(grabbedGameObject);
                                    }
                                }
                            }
                            //gameObject.GetComponent<ModuleRemoveColliders>().ReturnCollidersOnBlock();
                            Debug.Log("COLLIDER CONTROLLER: SET ALL POSSIBLE COLLIDERS ON " + gameObject + " ENABLED!");
                        }
                        catch
                        {
                            Debug.Log("COLLIDER CONTROLLER: FetchFailiure on " + gameObject + " enable");
                        }
                    }
                    else { Debug.Log("COLLIDER CONTROLLER: Initiation failiure - player status is " + tank.IsPlayer + " on " + tank.name); }
                    ForceUpdate = false;
                    lastLocalState = areAllPossibleCollidersDisabled;
                }
            }
        }


        /*
    public class ModuleColliderLock : Module
    {
        //   WIP!
        //This simply disables ModuleRemoveCollider from doing anything nasty like removing colliders within a player-changable box.

        //Variables
        //  Player-Settables
        private int localX = 1;
        private int localY = 1;
        private int localZ = 1;
    }
        */

        public class ModuleRemoveColliders : Module
        {
            //This is shoehorned into every block to control enabling of existing colliders
            //  MINIMISE UPDATE CYCLES FOR THIS AS IT CAN AND WILL LAG GREATLY IF USED WRONG!
            //     MANUAL ACTIONS REQUIRED
            // Will ignore:  
            //   - Colliders set the WheelCollider because if it touches those then CRASH
            //   - Blocks with ModuleRemoveColliders.DoNotDisableColliders = true as those are important to keep colliders for
            //   - All blocks when the player in any gamemode MP (no exploit-y)


            //Variables

            public RemoveColliderTank removeColliderTank;
            public TankBlock TankBlock;
            public bool DoNotDisableColliders = false;//set this to "true" through your JSON to deny collider disabling
            private bool areAllCollidersDisabledOnThisBlock = false;
            private bool UpdateNow = true;


            /*
            public void OnPool()
            { //When i figure out how to reload on Tech Spawn
                areAllCollidersDisabledOnThisBlock = false;
            }
            */
            public void ForceUpdateThis()
            {
                UpdateNow = true;
            }

            public void DestroyCollidersOnBlock()
            {
                string CB = gameObject.name;
                Debug.Log("Processing " + gameObject + " " + CB);

                bool thisIsACab = gameObject.transform.GetComponent<ModuleTechController>();
                if (thisIsACab)
                {
                    Debug.Log("CAB DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                bool thisIsABubble = gameObject.transform.GetComponent<ModuleShieldGenerator>();
                if (thisIsABubble)
                {
                    Debug.Log("SHIELD DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                /*
                bool thisIsAntiGrav = gameObject.transform.GetComponent<ModuleAntiGravityEngine>();
                if (thisIsAntiGrav)
                {
                    Debug.Log("ANTIGRAV DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                foreach () {
                    if (CB == "EXP_TowSet_1_Hook_111")
                    { //The RR Multi-Tech parts are to remain uneffected.
                        Debug.Log("MULTI-TECH BLOCK " + gameObject + " CANCELING!");
                        return;//End it NOW!
                    }
                }
                */

                if (CB == "EXP_TowSet_1_Hook_111" || CB == "EXP_TowSet_1_Ring_111" || CB == "EXP_TowSet_2_Hook_223" || CB == "EXP_TowSet_2_Ring_222" || CB == "EXP_TowSet_3_Hook_223" || 
                    CB == "EXP_TowSet_3_Ring_222" || CB == "EXP_TowRing_332" || CB == "EXP_TowSet_4_Hook_322" || CB == "EXP_TowSet_4_Ring_322" || CB == "EXP_TowSet_4_Lock_311" || 
                    CB == "EXP_JointSet_1_Bearing_111" || CB == "EXP_JointSet_1_Axle_111" || CB == "EXP_JointSet_1_Cap_1_111" || CB == "EXP_JointSet_1_Cap_2_111" || CB == "EXP_JointSet_2_Bearing_212" ||
                    CB == "EXP_JointSet_2_Axle_222" || CB == "EXP_JointSet_Pole_121" || CB == "EXP_JointSet_Pole_111" || CB == "EXP_JointSet_3_Ball_111" || CB == "EXP_JointSet_3_Socket_111" || 
                    CB == "EXP_JointSet_4_Ball_332" || CB == "EXP_JointSet_4_Socket_332")
                { //The RR Multi-Tech parts are to remain uneffected.
                    Debug.Log("MULTI-TECH BLOCK " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                if (CB == "EXP_Platform_Ramp_214" || CB == "EXP_Platform_Ramp_213" || CB == "EXP_Platform_Ramp_224" || CB == "EXP_Platform_414")
                { //The RR Ramps are to remain uneffected.
                    Debug.Log("TECH PLATFORM BLOCK " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                if (CB == "_C_BLOCK:1293831" || CB == "_C_BLOCK:1293830" || CB == "_C_BLOCK:1293700" || CB == "_C_BLOCK:1293701" || CB == "_C_BLOCK:1293702" || CB == "_C_BLOCK:1293703")
                { //GC Small Friction Pad and Non Slip-A<Tron 3000  |  Every MTMag.
                    Debug.Log("UNEDITABLE CONTROL BLOCK DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }

                if (CB == "_C_BLOCK:1293838" || CB == "_C_BLOCK:129380" || CB == "_C_BLOCK:129381" || CB == "_C_BLOCK:6194710" || CB == "_C_BLOCK:1293834" || CB == "_C_BLOCK:1293837" ||
                    CB == "_C_BLOCK:1980325" || CB == "_C_BLOCK:1293835" || CB == "_C_BLOCK:1393838" || CB == "_C_BLOCK:1393837" || CB == "_C_BLOCK:1393836" || CB == "_C_BLOCK:1393835" ||
                    CB == "_C_BLOCK:29571436")
                { //EVERY PISTON AND SWIVEL
                    Debug.Log("CLUSTERBODY CONTROL BLOCK DETECTED IN " + gameObject + "!  Handing off operation to RemoveColliderTank!");
                    return;//End it NOW!
                }

                if (DoNotDisableColliders != true && areAllCollidersDisabledOnThisBlock == false || UpdateNow)
                {
                    UpdateNow = false;
                    try//Sometimes there are colliders in the very base
                    {
                        gameObject.GetComponent<Collider>().enabled = false;
                        gameObject.GetComponent<MeshCollider>().enabled = false;
                        Debug.Log("Disabled Collider in " + gameObject);
                    }
                    catch
                    {
                        Debug.Log("Could not find Collider in " + gameObject);
                    }
                    try
                    {
                        //Try to cycle through EVERY GameObject on this block to disable EVERY COLLIDER
                        int child = gameObject.transform.childCount;
                        for (int v = 0; v < child; ++v) 
                        { 
                            Transform grabbedGameObject = gameObject.transform.GetChild(v);
                            try
                            {
                                if (grabbedGameObject.gameObject.layer != 20)
                                {   //DON'T DISABLE WHEELS IT WILL CRASH THE GAME!
                                    //   Also let specialized hoverbug-causing blocks continue working like intended.
                                    grabbedGameObject.GetComponent<Collider>().enabled = false;
                                    grabbedGameObject.GetComponent<MeshCollider>().enabled = false;
                                    Debug.Log("Disabled Collider in " + grabbedGameObject);
                                }
                                else
                                {
                                    Debug.Log("Skipped over Wheel Collider in " + grabbedGameObject);
                                }
                                //Debug.Log("Dee");
                            }
                            catch
                            {
                                Debug.Log("Could not find Collider in " + grabbedGameObject);
                            }
                        }
                        /*
                        try
                        {
                            //This no work
                            gameObject.transform.GetComponent<ModuleWeapon>().enabled = false;
                            Debug.Log("Disarmed " + gameObject);
                        }
                        catch { }
                        */

                    }
                    catch 
                    {
                        Debug.Log("EoB error");//END OF BLOCK
                    }
                    areAllCollidersDisabledOnThisBlock = true;

                }
                //Otherwise take no action
            
            }
            public void ReturnCollidersOnBlock()
            {
                string CB = gameObject.name;
                Debug.Log("Processing " + gameObject + " " + CB);

                bool thisIsACab = gameObject.transform.GetComponent<ModuleTechController>();
                if (thisIsACab)
                {
                    Debug.Log("CAB DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                bool thisIsABubble = gameObject.transform.GetComponent<ModuleShieldGenerator>();
                if (thisIsABubble)
                {
                    Debug.Log("SHIELD DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }

                if (CB == "EXP_TowSet_1_Hook_111" || CB == "EXP_TowSet_1_Ring_111" || CB == "EXP_TowSet_2_Hook_223" || CB == "EXP_TowSet_2_Ring_222" || CB == "EXP_TowSet_3_Hook_223" ||
                    CB == "EXP_TowSet_3_Ring_222" || CB == "EXP_TowRing_332" || CB == "EXP_TowSet_4_Hook_322" || CB == "EXP_TowSet_4_Ring_322" || CB == "EXP_TowSet_4_Lock_311" ||
                    CB == "EXP_JointSet_1_Bearing_111" || CB == "EXP_JointSet_1_Axle_111" || CB == "EXP_JointSet_1_Cap_1_111" || CB == "EXP_JointSet_1_Cap_2_111" || CB == "EXP_JointSet_2_Bearing_212" ||
                    CB == "EXP_JointSet_2_Axle_222" || CB == "EXP_JointSet_Pole_121" || CB == "EXP_JointSet_Pole_111" || CB == "EXP_JointSet_3_Ball_111" || CB == "EXP_JointSet_3_Socket_111" ||
                    CB == "EXP_JointSet_4_Ball_332" || CB == "EXP_JointSet_4_Socket_332")
                { //The RR Multi-Tech parts are to remain uneffected.
                    Debug.Log("MULTI-TECH BLOCK " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                if (CB == "EXP_Platform_Ramp_214" || CB == "EXP_Platform_Ramp_213" || CB == "EXP_Platform_Ramp_224" || CB == "EXP_Platform_414")
                { //The RR Ramps are to remain uneffected.
                    Debug.Log("TECH PLATFORM BLOCK " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }
                if (CB == "_C_BLOCK:1293831" || CB == "_C_BLOCK:1293830" || CB == "_C_BLOCK:1293700" || CB == "_C_BLOCK:1293701" || CB == "_C_BLOCK:1293702" || CB == "_C_BLOCK:1293703")
                { //GC Small Friction Pad and Non Slip-A<Tron 3000  |  Every MTMag.
                    Debug.Log("UNEDITABLE CONTROL BLOCK DETECTED IN " + gameObject + " CANCELING!");
                    return;//End it NOW!
                }

                if (DoNotDisableColliders != true && areAllCollidersDisabledOnThisBlock == true)
                {
                    UpdateNow = false;
                    try//Sometimes there are colliders in the very base
                    {
                        gameObject.GetComponent<Collider>().enabled = true;
                        gameObject.GetComponent<MeshCollider>().enabled = true;
                        Debug.Log("Enabled Collider in " + gameObject);
                    }
                    catch 
                    {
                        Debug.Log("Could not find Collider in " + gameObject);
                    }
                    try
                    {
                        //Try to cycle through EVERY GameObject on this block to disable EVERY COLLIDER
                        int child = gameObject.transform.childCount;
                        for (int v = 0; v < child; ++v)
                        {
                            Transform grabbedGameObject = gameObject.transform.GetChild(v);
                            try
                            {
                                if (grabbedGameObject.gameObject.layer != 20)
                                {   //DON'T DISABLE WHEELS IT WILL CRASH THE GAME!
                                    grabbedGameObject.GetComponent<Collider>().enabled = true;
                                    grabbedGameObject.GetComponent<MeshCollider>().enabled = true;
                                    Debug.Log("Enabled Collider on " + grabbedGameObject);
                                }
                                else
                                {
                                    Debug.Log("Skipped over Wheel Collider in " + grabbedGameObject);
                                }
                                //Debug.Log("Dee");
                            }
                            catch
                            {
                                Debug.Log("Could not find Collider in " + grabbedGameObject);
                            }
                        }
                        /*
                        try
                        {
                            //This no work
                            gameObject.transform.GetComponent<ModuleWeapon>().enabled = true;
                            Debug.Log("Re-armed " + gameObject);
                        }
                        catch { }
                        */
                    }
                    catch
                    {
                        Debug.Log("EoB error");//END OF BLOCK
                    }

                    areAllCollidersDisabledOnThisBlock = false;
                }
                //Otherwise take no action

            }

            public void TimedUpdate()
            {
                //Update when the player clikx
                Debug.Log((removeColliderTank == null ? "Uh-oh RemoveColliderTank is null!" + (TankBlock.tank == null ? " And so is the tonk" : "The tonk is not") : "RemoveColliderTank exists") 
                    + (TankBlock.rbody == null ? "\nTankBlock Rigidbody is null" : "\nWhat?") + (TankBlock.IsAttached ? "\nThe block appears to be attached" : "\nThe block is not attached"));
            }
        }
    }
}
