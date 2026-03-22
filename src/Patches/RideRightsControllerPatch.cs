using FairgroundAPI.Managers;
using HarmonyLib;

namespace FairgroundAPI.Patches
{
    /// <summary>
    /// Harmony postfix patch on <see cref="Ride_Rights_Controller.Current_Player"/> setter.
    /// Detects when the local player gains or loses control of a ride's control panel.
    /// </summary>
    [HarmonyPatch(typeof(Ride_Rights_Controller), nameof(Ride_Rights_Controller.Current_Player), MethodType.Setter)]
    public class RideRightsControllerPatch
    {
        public static void Postfix(Ride_Rights_Controller __instance)
        {
            var player = __instance.Current_Player;
            bool isLocalPlayer = player != null && player.Is_Me;
            SessionManager.ProcessRightsChange(__instance, isLocalPlayer);
        }
    }
}