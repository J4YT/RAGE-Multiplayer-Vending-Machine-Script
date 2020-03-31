using System.Collections.Generic;
using System.Linq;
using Client.Handlers;
using Client.Helpers;
using RAGE;
using RAGE.Elements;
using RAGE.Game;
using Entity = RAGE.Game.Entity;
using Player = RAGE.Elements.Player;

namespace Client.Services
{
    public class VendorService : Events.Script
    {
        private Player Player => Player.LocalPlayer;
        private static readonly List<uint> VendorMachines = new List<uint> { Objects.VendorMachine, Objects.VendorMachine2 };
        private static bool IsUsingVendorMachine { get; set; }
        private bool IsNearVendorMachine { get; set; }
        private VendorAnimationHandler VendorAnimation { get; set; }
        public VendorService()
        {
            Events.Tick += OnUpdate;

            VendorAnimation = new VendorAnimationHandler();
        }

        #region Events

        #endregion
        private void ActivateVendorMachine()
        {
            if (IsUsingVendorMachine) return;

            int? vendorHandle = GetNearestVendorHandle();

            if (!vendorHandle.HasValue) return;

            Vector3 vendorOffset = GetVendorOffsetFromWordCoords(vendorHandle.Value);

            if (vendorOffset == null) return;

            Player.LocalPlayer.SetData("IsUsingVendorMachine", new Dictionary<int, bool> { { vendorHandle.Value, true } });

            Player.SetCurrentWeaponVisible(false, true, true, false);
            Player.SetStealthMovement(false, "DEFAULT_ACTION");
            Player.TaskLookAtEntity(vendorHandle.Value, 2000, 2048, 2);
            Player.SetResetFlag(322, true);
            Player.TaskGoStraightToCoord(vendorOffset.X, vendorOffset.Y, vendorOffset.Z, 1f, 20000, Entity.GetEntityHeading(vendorHandle.Value), 0.1f);

            while (Ai.GetScriptTaskStatus(Player.Handle, 2106541073) != 7 && !Player.IsAtCoord(vendorOffset.X, vendorOffset.Y, vendorOffset.Z, 0.1f, 0.0f, 0.0f, false, true, 0))
            {
                Invoker.Wait(0);
            }

            VendorAnimation.Start();
        }

        private Vector3 GetVendorOffsetFromWordCoords(int vendorHandle)
        {
            return Entity.GetOffsetFromEntityInWorldCoords(vendorHandle, 0.0f, -0.97f, 0.05f);
        }
        private int? GetNearestVendorHandle()
        {
            foreach (var vendormachine in VendorMachines)
            {
                var vendorHandle = Object.GetClosestObjectOfType(Player.Position.X, Player.Position.Y, Player.Position.Z, 0.6f, vendormachine, false, false, false);

                if (Entity.IsAnEntity(vendorHandle))
                {
                    return vendorHandle;
                }
            }

            return null;
        }
        private void Listeners()
        {
            if (Pad.IsControlJustPressed(Constants.AllInputGroups, (int)Control.Context) && IsNearVendorMachine && !IsUsingVendorMachine)
            {
                SetVendorMachineInUse(true);
            }

            if (Pad.IsControlJustPressed(Constants.AllInputGroups, (int)Control.FrontendCancel) && IsUsingVendorMachine)
            {
                SetVendorMachineInUse(false);
            }
        }

        private void SetVendorMachineInUse(bool setInUse)
        {
            if (setInUse)
            {
                ActivateVendorMachine();
            }
            else
            {
                Player.LocalPlayer.ClearTasksImmediately();
            }

            SetInUse(setInUse);
        }
        public void OnUpdate(List<Events.TickNametagData> nametags)
        {
            Listeners();

            if (!IsUsingVendorMachine)
            {
                var vendorMachine = GetNearestVendorHandle();

                if (vendorMachine.HasValue && !IsAnyOneUsingVendorMachine(vendorMachine.Value))
                {
                    IsNearVendorMachine = true;
                    DisplayHelpText(Constants.VendorBuyHelpText);
                }

                else
                {
                    IsNearVendorMachine = false;
                }
            }
        }

        public static void SetInUse(bool setInUse)
        {
            IsUsingVendorMachine = setInUse;

            if (!setInUse)
            {
                Player.LocalPlayer.ResetData("IsUsingVendorMachine");
            }
        }

        private bool IsAnyOneUsingVendorMachine(int vendorHandle)
        {
            return Entities.Players.Streamed.Any(player =>
                player.HasData("IsUsingVendorMachine") && player.GetData<Dictionary<int, bool>>("IsUsingVendorMachine")
                    .ContainsKey(vendorHandle));
        }

        protected void DisplayHelpText(string text)
        {
            Ui.BeginTextCommandDisplayHelp("STRING");
            Ui.AddTextComponentSubstringPlayerName(text);
            Ui.EndTextCommandDisplayHelp(0, false, true, -1);
        }
    }
}
