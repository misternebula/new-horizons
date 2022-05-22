﻿using NewHorizons.External.Configs;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class VolumesBuilder
    {
        private static readonly int FogColor = Shader.PropertyToID("_FogColor");

        public static void Make(GameObject planetGO, PlanetConfig config, float sphereOfInfluence)
        {
            var innerRadius = config.Base.SurfaceSize;

            GameObject volumesGO = new GameObject("Volumes");
            volumesGO.SetActive(false);
            volumesGO.transform.parent = planetGO.transform;

            GameObject rulesetGO = new GameObject("Ruleset");
            rulesetGO.SetActive(false);
            rulesetGO.transform.parent = volumesGO.transform;

            SphereShape SS = rulesetGO.AddComponent<SphereShape>();
            SS.SetCollisionMode(Shape.CollisionMode.Volume);
            SS.SetLayer(Shape.Layer.Sector);
            SS.layerMask = -1;
            SS.pointChecksOnly = true;
            SS.radius = sphereOfInfluence;

            rulesetGO.AddComponent<OWTriggerVolume>();

            PlanetoidRuleset PR = rulesetGO.AddComponent<PlanetoidRuleset>();
            PR._altitudeFloor = innerRadius;
            PR._altitudeCeiling = sphereOfInfluence;
            PR._useMinimap = config.Base.ShowMinimap;
            PR._useAltimeter = config.Base.ShowMinimap;

            rulesetGO.AddComponent<AntiTravelMusicRuleset>();

            EffectRuleset ER = rulesetGO.AddComponent<EffectRuleset>();
            ER._type = EffectRuleset.BubbleType.Underwater;
            var gdRuleset = GameObject.Find("GiantsDeep_Body/Sector_GD/Volumes_GD/RulesetVolumes_GD").GetComponent<EffectRuleset>();

            ER._material = gdRuleset._material;

            var cloudMaterial = new Material(gdRuleset._cloudMaterial);
            if (config.Atmosphere?.Clouds?.Tint != null)
            {
                cloudMaterial.SetColor(FogColor, config.Atmosphere.Clouds.Tint.ToColor32());
            }
            ER._cloudMaterial = cloudMaterial;

            volumesGO.transform.position = planetGO.transform.position;
            rulesetGO.SetActive(true);
            volumesGO.SetActive(true);
        }
    }
}
