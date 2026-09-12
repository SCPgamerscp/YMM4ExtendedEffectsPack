"""Portable structural checks. These do not substitute for Windows GPU tests."""
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
PKG = ROOT / "YMM4.ExtendedEffectsPack"
EFFECTS = PKG / "Effects"
SHADERS = PKG / "Shaders"
TRANS = PKG / "Transitions"

def csharp_types(folder, suffix):
    names = []
    for path in folder.glob(f"*{suffix}.cs"):
        text = path.read_text(encoding="utf-8")
        found = re.findall(rf"class (\w+){suffix}", text)
        names.extend(found)
    return sorted(set(names))

class CatalogTests(unittest.TestCase):
    def test_effect_and_shader_counts(self):
        effects = csharp_types(EFFECTS, "Effect")
        # VideoEffect classes are *Effect.cs excluding *CustomEffect.cs
        video = [p.stem.replace("Effect", "") for p in EFFECTS.glob("*Effect.cs") if "Custom" not in p.name]
        shaders = [p.stem for p in SHADERS.glob("*.hlsl")]
        self.assertEqual(len(video), 35, video)
        self.assertEqual(sorted(video), sorted(shaders), "each effect needs a matching HLSL file")

    def test_unique_custom_effect_types(self):
        customs = [p.stem for p in EFFECTS.glob("*CustomEffect.cs")]
        self.assertEqual(len(customs), len(set(customs)))
        self.assertEqual(len(customs), 35)

    def test_native_transitions(self):
        plugins = []
        for path in TRANS.glob("*Transition.cs"):
            text = path.read_text(encoding="utf-8")
            self.assertIn(": ITransitionPlugin", text)
            self.assertIn(": PackTransitionParameter", text)
            self.assertRegex(text, r'Name => "EEP / ')
            self.assertIn("CreateTransitionParameter", text)
            plugins.append(path.stem)
        self.assertEqual(len(plugins), 16, plugins)

    def test_video_effect_attributes_are_japanese(self):
        names = []
        for path in EFFECTS.glob("*Effect.cs"):
            if "Custom" in path.name:
                continue
            text = path.read_text(encoding="utf-8")
            m = re.search(r'\[VideoEffect\("([^"]+)"', text)
            self.assertIsNotNone(m, path.name)
            names.append(m.group(1))
            self.assertIn("拡張エフェクト", text)
            self.assertIn("IsAviUtlSupported = false", text)
            self.assertIn("GetAnimatables()", text)
            self.assertLessEqual(text.count("[AnimationSlider"), 9, path.name)
        self.assertEqual(len(names), len(set(names)), names)

    def test_motion_ghost_uses_real_history(self):
        effect = (EFFECTS / "MotionGhostEffect.cs").read_text(encoding="utf-8")
        self.assertIn("new MotionGhostProcessor", effect)
        custom = (EFFECTS / "MotionGhostCustomEffect.cs").read_text(encoding="utf-8")
        self.assertIn("[CustomEffect(2)]", custom)
        hlsl = (SHADERS / "MotionGhost.hlsl").read_text(encoding="utf-8")
        self.assertIn("InputTexture1", hlsl)
        hist = (PKG / "Common" / "FrameHistory.cs").read_text(encoding="utf-8")
        self.assertIn("128L * 1024 * 1024", hist)

    def test_prism_whip_present(self):
        self.assertTrue((EFFECTS / "PrismWhipEffect.cs").exists())
        self.assertTrue((SHADERS / "PrismWhip.hlsl").exists())
        self.assertTrue((TRANS / "PrismWhipTransition.cs").exists())
        text = (EFFECTS / "PrismWhipEffect.cs").read_text(encoding="utf-8")
        self.assertIn("プリズムウィップ", text)

    def test_csproj_embeds_shaders_and_does_not_ship_sdk(self):
        project = (PKG / "YMM4.ExtendedEffectsPack.csproj").read_text(encoding="utf-8")
        self.assertIn("<Private>false</Private>", project)
        self.assertIn("net10.0-windows10.0.19041.0", project)
        self.assertIn("EmbeddedResource", project)
        self.assertNotIn("PackageReference", project)
        self.assertIn("EnableWindowsTargeting", project)

    def test_transition_envelope_covers_pr1_set(self):
        required = {
            "TheaterCurtain", "Blink", "BlockWave", "FilmBurn",
            "InkBleed", "LumaMelt", "MatrixRain", "PrismWhip",
        }
        have = {p.stem.replace("Transition", "") for p in TRANS.glob("*Transition.cs")}
        self.assertTrue(required <= have, required - have)

if __name__ == "__main__":
    unittest.main(verbosity=2)
