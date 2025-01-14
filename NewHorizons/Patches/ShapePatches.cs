using HarmonyLib;
using System.Collections.Generic;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public class ShapePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShapeManager), nameof(ShapeManager.Initialize))]
        public static bool ShapeManager_Initialize()
        {
            ShapeManager._exists = true;

            ShapeManager._detectors = new ShapeManager.Layer(256);
            for (int index = 0; index < 256; ++index)
                ShapeManager._detectors[index].contacts = new List<ShapeManager.ContactData>(64);

            ShapeManager._volumes = new ShapeManager.Layer[4];
            for (int index = 0; index < 4; ++index)
                ShapeManager._volumes[index] = new ShapeManager.Layer(2048);

            ShapeManager._locked = false;
            ShapeManager._frameFlag = false;

            return false;
        }
    }
}
