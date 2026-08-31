using HarmonyLib;
using Verse;

namespace Styled_Identity_Framework.Patches
{
    [HarmonyPatch(typeof(VerbTracker), "InitVerb")]
    public static class VerbTracker_VerbStyle_Patch
    {
        public static void Postfix(Verb verb)
        {
            VerbStyleUtility.Refresh(verb);
        }
    }
}
