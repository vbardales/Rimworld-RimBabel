using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using RimBabel.Game;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace RimBabel.PickleSteps
{
    /// <summary>
    /// A real click on a radio button of the settings page. Pickle records the vanilla buttons by label but not a
    /// Listing_Standard radio, so this suite tags each one as it is drawn (a postfix, test mod only) and Pickle then clicks
    /// the tag: the click goes through the game's own event handling, which a step that sets the engine by code never does.
    /// </summary>
    [PickleSteps]
    public class RadioSteps
    {
        private static Harmony harmony;

        [Given("RimBabel: the radio buttons of the settings page can be clicked")]
        public void TagRadios(PickleContext ctx)
        {
            if (harmony != null) return;
            MethodInfo radio = typeof(Listing_Standard).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "RadioButton" && m.GetParameters().Length > 1 && m.GetParameters()[0].ParameterType == typeof(string));
            ctx.Require(radio != null, "Listing_Standard has no RadioButton(string, bool, ...) in this game version");
            harmony = new Harmony("nelim.rimbabel.pickle.radios");
            harmony.Patch(radio, postfix: new HarmonyMethod(typeof(RadioSteps).GetMethod("Tag", BindingFlags.Static | BindingFlags.NonPublic)));
        }

        // Listing_Standard.RadioButton takes one text line, then a vertical gap: the rect just drawn is that line.
        private static void Tag(Listing_Standard __instance, string label)
        {
            float top = __instance.CurHeight - __instance.verticalSpacing - Text.LineHeight;
            PickleUI.Tag("rimbabel-radio:" + label, new Rect(0f, top, __instance.ColumnWidth, Text.LineHeight));
        }

        [When("RimBabel: I click the radio keyed {string}")]
        public async Task Click(PickleContext ctx, string key)
        {
            string text = key.Translate().Resolve();
            ctx.Require(text != key, "no translation for '" + key + "' in this language");
            await ctx.Click("rimbabel-radio:" + text);
        }

        [Then("RimBabel: the engine of the settings is {string}")]
        public void EngineIs(PickleContext ctx, string engine)
        {
            ctx.Assert(RimBabelMod.Settings.engine.ToString() == engine, "the engine is " + RimBabelMod.Settings.engine + ", expected " + engine + ": the click did not reach the page");
        }
    }
}
