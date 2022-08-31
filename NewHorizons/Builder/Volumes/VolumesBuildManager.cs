using NewHorizons.Builder.Body;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Builder.Volumes;
using NewHorizons.External.Configs;
using OWML.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumesBuildManager
    {
        public static void Make(GameObject go, Sector sector, PlanetConfig config, IModBehaviour mod)
        {
            if (config.Volumes.revealVolumes != null)
            {
                foreach (var revealInfo in config.Volumes.revealVolumes)
                {
                    try
                    {
                        RevealBuilder.Make(go, sector, revealInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make reveal location [{revealInfo.reveals}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Volumes.audioVolumes != null)
            {
                foreach (var audioVolume in config.Volumes.audioVolumes)
                {
                    AudioVolumeBuilder.Make(go, sector, audioVolume, mod);
                }
            }
            if (config.Volumes.notificationVolumes != null)
            {
                foreach (var notificationVolume in config.Volumes.notificationVolumes)
                {
                    NotificationVolumeBuilder.Make(go, sector, notificationVolume, mod);
                }
            }
        }
    }
}
