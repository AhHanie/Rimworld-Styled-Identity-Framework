using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Styled_Identity_Framework
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "StyledIdentityFramework.LoadingLabel", doAsynchronously: true, null);
        }

        private void Init()
        {
            StyledStatBaseUtility.Initialize();

            Harmony harmony = new Harmony("sk.styledidframework");
            harmony.PatchAll();
            Patches.MVCF_CommandVerbTargetExtended_StyleIdentity_Patch.TryPatch(harmony);
        }
    }
}
