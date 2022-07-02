using HarmonyLib;
using NewHorizons.Builder.Body;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NewHorizons.External.Modules.BrambleModule;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{

    // TODO
    //3) support for existing dimensions?
    //5) test whether nodes can lead to vanilla dimensions


    
    [HarmonyPatch]
    public static class TestPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerFogWarpDetector), nameof(PlayerFogWarpDetector.LateUpdate))]
        private static void PlayerFogWarpDetector_LateUpdate(PlayerFogWarpDetector __instance)
        {
	        if (!(PlanetaryFogController.GetActiveFogSphere() != null))
	        {
		        return;
	        }
	        float num = __instance._targetFogFraction;
	        if (PlayerState.IsInsideShip())
	        {
		        num = Mathf.Max(__instance._shipFogDetector.GetTargetFogFraction(), num);
	        }
	        if (num < __instance._fogFraction)
	        {
		        float num2 = (__instance._closestFogWarp.UseFastFogFade() ? 1f : 0.2f);
		        __instance._fogFraction = Mathf.MoveTowards(__instance._fogFraction, num, Time.deltaTime * num2);
	        }
	        else
	        {
		        __instance._fogFraction = num;
	        }
	        if (__instance._targetFogColorWarpVolume != __instance._closestFogWarp)
	        {
		        __instance._targetFogColorWarpVolume = __instance._closestFogWarp;
		        __instance._startColorCrossfadeTime = Time.time;
		        __instance._startCrossfadeColor = __instance._fogColor;
	        }
	        if (__instance._targetFogColorWarpVolume != null)
	        {
		        Color fogColor = __instance._targetFogColorWarpVolume.GetFogColor();
		        if (__instance._fogFraction <= 0f)
		        {
			        __instance._fogColor = fogColor;
		        }
		        else
		        {
			        float t = Mathf.InverseLerp(__instance._startColorCrossfadeTime, __instance._startColorCrossfadeTime + 1f, Time.time);
			        __instance._fogColor = Color.Lerp(__instance._startCrossfadeColor, fogColor, t);
		        }
	        }
	        if (__instance._playerEffectBubbleController != null)
	        {
		        __instance._playerEffectBubbleController.SetFogFade(__instance._fogFraction, __instance._fogColor);
	        }
	        if (__instance._shipLandingCamEffectBubbleController != null)
	        {
		        __instance._shipLandingCamEffectBubbleController.SetFogFade(__instance._fogFraction, __instance._fogColor);
	        }
        }
    }






    public static class BrambleNodeBuilder
    {
        // keys are all dimension names that have been referenced by at least one node but do not (yet) exist
        // values are all nodes' warp controllers that link to a given dimension
        // unpairedNodes[name of dimension that doesn't exist yet] => List{warp controller for node that links to that dimension, ...}
        private static Dictionary<string, List<InnerFogWarpVolume>> unpairedNodes = new();

        public static Dictionary<string, InnerFogWarpVolume> namedNodes = new();

        public static void FinishPairingNodesForDimension(string dimensionName, AstroObject dimensionAO = null)
        {
            if (!unpairedNodes.ContainsKey(dimensionName)) return;

            foreach (var nodeWarpController in unpairedNodes[dimensionName])
            {
                PairEntrance(nodeWarpController, dimensionName, dimensionAO);    
            }

            unpairedNodes.Remove(dimensionName);
        }

        private static void RecordUnpairedNode(InnerFogWarpVolume warpVolume, string linksTo)
        {
            if (!unpairedNodes.ContainsKey(linksTo)) unpairedNodes[linksTo] = new();
            
            unpairedNodes[linksTo].Add(warpVolume);
        }

        private static OuterFogWarpVolume GetOuterFogWarpVolumeFromAstroObject(GameObject go)
        {
            var outerWarpGO = go.FindChild("Sector/OuterWarp");
            if (outerWarpGO == null) return null;

            var outerFogWarpVolume = outerWarpGO.GetComponent<OuterFogWarpVolume>();
            return outerFogWarpVolume;
        }

        private static bool PairEntrance(InnerFogWarpVolume nodeWarp, string destinationName, AstroObject dimensionAO = null)
        {
            var destinationAO = dimensionAO ?? AstroObjectLocator.GetAstroObject(destinationName); // find child "Sector/OuterWarp"
            if (destinationAO == null) return false;
            
            // add the destination dimension's signals to this node
            var dimensionNewHorizonsBody = PlanetCreationHandler.GetNewHorizonsBody(destinationAO);
            if (dimensionNewHorizonsBody != null && dimensionNewHorizonsBody.Config?.Signal?.signals != null)
            {
                var body = nodeWarp.GetComponentInParent<AstroObject>().gameObject;
                var sector = nodeWarp.GetComponentInParent<Sector>();
                
                foreach(var signalConfig in dimensionNewHorizonsBody.Config?.Signal?.signals)
                {
                    var signalGO = SignalBuilder.Make(body, sector, signalConfig, dimensionNewHorizonsBody.Mod);
                    signalGO.GetComponent<AudioSignal>()._identificationDistance = 0;
                    signalGO.GetComponent<AudioSignal>()._sourceRadius = 1;
                    signalGO.transform.position = nodeWarp.transform.position;
                    signalGO.transform.parent = nodeWarp.transform;
                }        
            }

            // link the node's warp volume to the destination's
            var destination = GetOuterFogWarpVolumeFromAstroObject(destinationAO.gameObject);
            if (destination == null) return false;

            nodeWarp._linkedOuterWarpVolume = destination;
            destination.RegisterSenderWarp(nodeWarp);
            return true;
        }


        // DB_EscapePodDimension_Body/Sector_EscapePodDimension/Interactables_EscapePodDimension/InnerWarp_ToAnglerNest // need to change the light shaft color
        // DB_ExitOnlyDimension_Body/Sector_ExitOnlyDimension/Interactables_ExitOnlyDimension/InnerWarp_ToExitOnly  // need to change the colors
        // DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster   // need to delete the child "Signal_Harmonica"

        public static void Make(GameObject go, Sector sector, BrambleNodeInfo[] configs)
        {
            foreach(var config in configs)
            {
                Make(go, sector, config);
            }
        }

        public static GameObject Make(GameObject go, Sector sector, BrambleNodeInfo config)
        {
            //
            // spawn the bramble node
            //

            var brambleSeedPrefabPath = "DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension/SeedWarp_ToPioneer (1)";
            var brambleNodePrefabPath = "DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster";
            
            var path = config.isSeed ? brambleSeedPrefabPath : brambleNodePrefabPath;
            var brambleNode = DetailBuilder.MakeDetail(go, sector, path, config.position, config.rotation, 1, false);
            brambleNode.name = "Bramble Node to " + config.linksTo;    
            var warpController = brambleNode.GetComponent<InnerFogWarpVolume>();

            // this node comes with Feldspar's signal, we don't want that though
            GameObject.Destroy(brambleNode.FindChild("Signal_Harmonica"));
                

            //
            // Set the scale
            //
            brambleNode.transform.localScale = Vector3.one * config.scale;
            warpController._warpRadius *= config.scale;
            warpController._exitRadius *= config.scale;
            

            //
            // change the colors
            //
            
            if (config.isSeed) SetSeedColors(brambleNode, config.fogTint.ToColor(), config.lightTint.ToColor());
            else               SetNodeColors(brambleNode, config.fogTint.ToColor(), config.lightTint.ToColor());

            //
            // set up warps
            //

            warpController._sector = sector;
            warpController._attachedBody = go.GetComponent<OWRigidbody>(); // I don't think this is necessary, it seems to be set correctly on its own
            warpController._containerWarpVolume = GetOuterFogWarpVolumeFromAstroObject(go); // the OuterFogWarpVolume of the dimension this node is inside of (null if this node is not inside of a bramble dimension (eg it's sitting on a planet or something))
            var success = PairEntrance(warpController, config.linksTo);
            if (!success) RecordUnpairedNode(warpController, config.linksTo);

            warpController.Awake(); // I can't spawn this game object disabled, but Awake needs to run after _sector is set. That means I need to call Awake myself

            //
            // Cleanup for dimension exits
            //
            if (config.name != null)
            {
                namedNodes[config.name] = warpController;
                BrambleDimensionBuilder.FinishPairingDimensionsForExitNode(config.name);
            }


            // Done!
            return brambleNode;
        }

        public static void SetNodeColors(GameObject brambleNode, Color fogTint, Color lightTint)
        {
            if (fogTint != null) 
            { 
                var fogRenderer = brambleNode.GetComponent<InnerFogWarpVolume>();
                
                fogRenderer._fogColor = fogTint;
                fogRenderer._useFarFogColor = false;
            } 

            if (lightTint != null)
            {
                var lightShafts = brambleNode.FindChild("Effects/DB_BrambleLightShafts");
                
                var lightShaft1 = lightShafts.FindChild("BrambleLightShaft1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = lightTint;
                
                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = lightShafts.FindChild($"BrambleLightShaft{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }

        public static void SetSeedColors(GameObject brambleSeed, Color fogTint, Color lightTint)
        {
            if (fogTint != null) 
            { 
                var fogRenderer = brambleSeed.FindChild("VolumetricFogSphere (2)");
                
                var fogMeshRenderer = fogRenderer.GetComponent<MeshRenderer>();
                var mat = fogMeshRenderer.material;
                mat.color = fogTint;
                fogMeshRenderer.sharedMaterial = mat;
            } 
            
            if (lightTint != null)
            {
                var lightShafts = brambleSeed.FindChild("Terrain_DB_BrambleSphere_Seed_V2 (2)/DB_SeedLightShafts");
                
                var lightShaft1 = lightShafts.FindChild("DB_SeedLightShafts1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = lightTint;
                
                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = lightShafts.FindChild($"DB_SeedLightShafts{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }
    }
}
