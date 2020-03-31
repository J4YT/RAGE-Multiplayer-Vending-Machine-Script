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
    public class VendingMachineService : Events.Script
    {
        private Player Player => Player.LocalPlayer;
        private static readonly List<uint> VendingMachines = new List<uint> { Objects.VendingMachine, Objects.VendingMachine2 };
        private static bool IsUsingVendingMachine { get; set; }
        private bool IsNearVendingMachine { get; set; }
        private VendingMachineAnimationHandler VendingAnimation { get; set; }
        public VendingMachineService()
        {
            Events.Tick += OnUpdate;

            VendingAnimation = new VendingMachineAnimationHandler();
        }
        private void ActivateVendingMachine()
        {
            if (IsUsingVendingMachine) return;

            int? handle = GetNearestVendingHandle();

            if (!handle.HasValue) return;

            Vector3 offsetFromWordCoords = GetVendingOffsetFromWordCoords(handle.Value);

            if (offsetFromWordCoords == null) return;

            Player.LocalPlayer.SetData("IsUsingVendingMachine", new Dictionary<int, bool> { { handle.Value, true } });

            Player.SetCurrentWeaponVisible(false, true, true, false);
            Player.SetStealthMovement(false, "DEFAULT_ACTION");
            Player.TaskLookAtEntity(handle.Value, 2000, 2048, 2);
            Player.SetResetFlag(322, true);
            Player.TaskGoStraightToCoord(offsetFromWordCoords.X, offsetFromWordCoords.Y, offsetFromWordCoords.Z, 1f, 20000, Entity.GetEntityHeading(handle.Value), 0.1f);

            while (Ai.GetScriptTaskStatus(Player.Handle, 2106541073) != 7 && !Player.IsAtCoord(offsetFromWordCoords.X, offsetFromWordCoords.Y, offsetFromWordCoords.Z, 0.1f, 0.0f, 0.0f, false, true, 0))
            {
                Invoker.Wait(0);
            }

            VendingAnimation.Start();
        }

        private Vector3 GetVendingOffsetFromWordCoords(int handle)
        {
            return Entity.GetOffsetFromEntityInWorldCoords(handle, 0.0f, -0.97f, 0.05f);
        }
        private int? GetNearestVendingHandle()
        {
            foreach (var vendingMachine in VendingMachines)
            {
                var handle = Object.GetClosestObjectOfType(Player.Position.X, Player.Position.Y, Player.Position.Z, 0.6f, vendingMachine, false, false, false);

                if (Entity.IsAnEntity(handle))
                {
                    return handle;
                }
            }

            return null;
        }
        private void Listeners()
        {
            if (Pad.IsControlJustPressed(Constants.AllInputGroups, (int)Control.Context) && IsNearVendingMachine && !IsUsingVendingMachine)
            {
                SetVendingMachineInUse(true);
            }

            if (Pad.IsControlJustPressed(Constants.AllInputGroups, (int)Control.FrontendCancel) && IsUsingVendingMachine)
            {
                SetVendingMachineInUse(false);
            }
        }

        private void SetVendingMachineInUse(bool setInUse)
        {
            if (setInUse)
            {
                ActivateVendingMachine();
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

            if (!IsUsingVendingMachine)
            {
                var handle = GetNearestVendingHandle();

                if (handle.HasValue && !IsAnyOneUsingVendingMachine(handle.Value))
                {
                    IsNearVendingMachine = true;
                    DisplayHelpText(Constants.VendingBuyHelpText);
                }

                else
                {
                    IsNearVendingMachine = false;
                }
            }
        }

        public static void SetInUse(bool setInUse)
        {
            IsUsingVendingMachine = setInUse;

            if (!setInUse)
            {
                Player.LocalPlayer.ResetData("IsUsingVendingMachine");
            }
        }

        private bool IsAnyOneUsingVendingMachine(int VendingHandle)
        {
            return Entities.Players.Streamed.Any(player =>
                player.HasData("IsUsingVendingMachine") && player.GetData<Dictionary<int, bool>>("IsUsingVendingMachine")
                    .ContainsKey(VendingHandle));
        }

        protected void DisplayHelpText(string text)
        {
            Ui.BeginTextCommandDisplayHelp("STRING");
            Ui.AddTextComponentSubstringPlayerName(text);
            Ui.EndTextCommandDisplayHelp(0, false, true, -1);
        }
    }
}
