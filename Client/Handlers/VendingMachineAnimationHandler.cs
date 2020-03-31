using System.Collections.Generic;
using Attachment_Sync.Handlers;
using Client.Services;
using RAGE;
using RAGE.Game;

namespace Client.Handlers
{
    public class VendingMachineAnimationHandler : Events.Script
    {
        private RAGE.Elements.Player Player => RAGE.Elements.Player.LocalPlayer;
        private bool IsDictionaryLoaded { get; set; }
        private static string Dictionary => "MINI@SPRUNK@FIRST_PERSON";
        public VendingMachineAnimationHandler()
        {
            Events.Tick += OnUpdate;
        }

        public void Start()
        {
            Streaming.RequestAnimDict(Dictionary);

            while (!Streaming.HasAnimDictLoaded(Dictionary))
            {
                Invoker.Wait(0);
            }

            IsDictionaryLoaded = true;
            Audio.RequestAmbientAudioBank("VENDING_MACHINE", false, -1);
            Player.TaskPlayAnim(Dictionary, "PLYR_BUY_DRINK_PT1", 2f, -4f, -1, 1048576, 0, false, false, false);
        }
        private void OnUpdate(List<Events.TickNametagData> nametags)
        {
            if (!IsDictionaryLoaded) return;

            if (Player.IsPlayingAnim(Dictionary, "PLYR_BUY_DRINK_PT1", 1))
            {
                if (Player.GetAnimCurrentTime(Dictionary, "PLYR_BUY_DRINK_PT1") > 0.1f)
                {
                    AttachmentHandler.Add("soda");
                }

                if (Player.GetAnimCurrentTime(Dictionary, "PLYR_BUY_DRINK_PT1") > 0.98f)
                {
                    Player.TaskPlayAnim(Dictionary, "PLYR_BUY_DRINK_PT2", 4f, -1000f, -1, 1048576, 0f, false, false, false);
                    Invoker.Invoke(0x2208438012482A1A, Player.Handle, false, false); // PED::_SET_PED_FAST_ANIMATIONS
                }
            }

            if (Player.IsPlayingAnim(Dictionary, "PLYR_BUY_DRINK_PT2", 1))
            {
                if (Player.GetAnimCurrentTime(Dictionary, "PLYR_BUY_DRINK_PT2") > 0.98f)
                {
                    Player.TaskPlayAnim(Dictionary, "PLYR_BUY_DRINK_PT3", 1000f, -4f, -1, 1048624, 0f, false, false, false);
                    Invoker.Invoke(0x2208438012482A1A, Player.Handle, false, false); // PED::_SET_PED_FAST_ANIMATIONS
                }
            } 
            
            if (Player.IsPlayingAnim(Dictionary, "PLYR_BUY_DRINK_PT3", 1))
            {
                if (Player.GetAnimCurrentTime(Dictionary, "PLYR_BUY_DRINK_PT3") > 0.306f)
                {
                    AttachmentHandler.Remove("soda");
                }

                if (Player.GetAnimCurrentTime(Dictionary, "PLYR_BUY_DRINK_PT3") > 0.9f)
                {
                    Streaming.RemoveAnimDict(Dictionary);
                    Audio.ReleaseAmbientAudioBank();
                    IsDictionaryLoaded = false;
                    VendingMachineService.SetInUse(false);
                }
            }
        }
    }
}
