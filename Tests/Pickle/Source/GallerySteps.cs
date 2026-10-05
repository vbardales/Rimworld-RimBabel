using System.Reflection;
using RimBabel.Core;
using RimBabel.Game;
using RimWorks.Pickle;
using UnityEngine;

namespace RimBabel.PickleSteps
{
    /// <summary>
    /// Pictures for the Workshop gallery: the settings page with something in it, and scrolled to the part a picture should show.
    /// Nothing here asserts; the scenarios are tagged @review. The values are invented and the key is a placeholder, so no capture
    /// can hold a real key (the defaults step of SettingsSteps empties them first and puts the player's own back afterwards).
    /// </summary>
    [PickleSteps]
    public class GallerySteps
    {
        [Given("RimBabel: the settings hold example values for a picture")]
        public void Fill(PickleContext ctx)
        {
            ctx.Require(RimBabelMod.Settings != null, "RimBabel's settings are not loaded: the mod did not start");
            RimBabelSettings s = RimBabelMod.Settings;
            s.engine = EngineKind.DeepL;
            s.deeplKey = "00000000-0000-0000-0000-000000000000:fx";
            s.batchSize = 20;
            s.author = "Your name";
            s.dictionaryText = "colonist = colon\nraider = pillard\nhearth = foyer";
            s.blacklistText = "K:Credits_*\nre:^D:ThingDef/.*\\.defName$";
        }

        [When("RimBabel: the settings page is scrolled down by {int} pixels")]
        public void Scroll(PickleContext ctx, int pixels)
        {
            FieldInfo f = typeof(SettingsPage).GetField("scroll", BindingFlags.Static | BindingFlags.NonPublic);
            ctx.Require(f != null, "SettingsPage has no 'scroll' field any more");
            f.SetValue(null, new Vector2(0f, pixels));
        }
    }
}
