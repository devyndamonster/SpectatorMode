using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using FistVR;
using HarmonyLib;
using Sodalite;
using Sodalite.Api;
using UnityEngine;

namespace SpectatorMode
{
    [BepInPlugin("devyndamonster.h3vr.spectator", "H3VR Spectator", "0.1.0")]
    [BepInDependency(SodaliteConstants.Guid, SodaliteConstants.Version)]
    public class SpectatorMode : BaseUnityPlugin
    {

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(SpectatorMode));

            WristMenuAPI.Buttons.Add(new WristMenuButton("Increase Player Scale", IncreaseScale));
            WristMenuAPI.Buttons.Add(new WristMenuButton("Decrease Player Scale", DecreaseScale));
            WristMenuAPI.Buttons.Add(new WristMenuButton("Set Spectator", SetSpectatorMode));
            WristMenuAPI.Buttons.Add(new WristMenuButton("Undo Spectator", UndoSpectatorMode));
        }


        public void IncreaseScale(object sender, ButtonClickEventArgs args)
        {
            GM.CurrentPlayerRoot.transform.localScale += Vector3.one;
            WristMenuAPI.Instance.transform.localScale = GM.CurrentPlayerRoot.transform.localScale;
        }


        public void DecreaseScale(object sender, ButtonClickEventArgs args)
        {
            if (GM.CurrentPlayerRoot.transform.localScale.x < 2f) GM.CurrentPlayerRoot.transform.localScale = Vector3.one;
            else GM.CurrentPlayerRoot.transform.localScale -= Vector3.one;

            WristMenuAPI.Instance.transform.localScale = GM.CurrentPlayerRoot.transform.localScale;
        }


        public void SetSpectatorMode(object sender, ButtonClickEventArgs args)
        {
            GM.CurrentMovementManager.Mode = (FVRMovementManager.MovementMode)10;
            GM.CurrentPlayerBody.DisableHitBoxes();
            GM.CurrentPlayerBody.SetPlayerIFF(-3);
        }


        public void UndoSpectatorMode(object sender, ButtonClickEventArgs args)
        {
            GM.CurrentMovementManager.Mode = GM.Options.MovementOptions.CurrentMovementMode;
            GM.CurrentPlayerBody.EnableHitBoxes();
            GM.CurrentPlayerBody.SetPlayerIFF(0);
        }


        [HarmonyPatch(typeof(FVRMovementManager), "FU")] // Specify target method with HarmonyPatch attribute
        [HarmonyPostfix]
        public static void UpdateSpectatorMode(FVRMovementManager __instance)
        {
            if(__instance.Mode == (FVRMovementManager.MovementMode)10)
            {
                Vector3 moveVel = Vector3.zero;

                foreach(FVRViveHand hand in __instance.Hands)
                {
                    if (hand.IsInStreamlinedMode)
                    {
                        if((hand.CMode == ControlMode.Index || hand.CMode == ControlMode.WMR) && hand.Input.Secondary2AxisInputAxes.y > 0.1f)
                        {
                            moveVel += (hand.PointingTransform.forward * hand.Input.Secondary2AxisInputAxes.y) * Time.fixedDeltaTime * GM.CurrentPlayerRoot.localScale.y;
                        }

                        else if (hand.Input.TouchpadAxes.y > 0.1f)
                        {
                            moveVel += (hand.PointingTransform.forward * hand.Input.TouchpadAxes.y) * Time.fixedDeltaTime * GM.CurrentPlayerRoot.localScale.y;
                        }
                    }
                    else
                    {
                        if (hand.Input.BYButtonPressed)
                        {
                            moveVel += (hand.PointingTransform.forward) * Time.fixedDeltaTime * GM.CurrentPlayerRoot.localScale.y;
                        }
                    }
                }

                GM.CurrentPlayerRoot.position += moveVel;
            }
        }

    }
}
