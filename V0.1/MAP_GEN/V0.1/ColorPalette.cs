using UnityEngine;

namespace NeonSplash
{
    public class ColorPalette
    {
        public Color Primary;
        public Color Secondary;
        public Color Fog;
        public Color Background;
        public string PaletteName;

        public ColorPalette(int seed)
        {
            float hue1 = (seed * 137.508f) % 360f / 360f; // 0 to 1 for Color.HSVToRGB
            float hue2 = (hue1 + 0.5f) % 1.0f;

            Primary = Color.HSVToRGB(hue1, 0.8f, 0.9f);
            Secondary = Color.HSVToRGB(hue2, 0.7f, 0.8f);
            Fog = Color.HSVToRGB(hue1, 0.5f, 0.15f);
            Background = new Color(0.04f, 0.04f, 0.06f); // 0x0a0a0f

            PaletteName = GenerateName(hue1 * 360f);
        }

        private string GenerateName(float hue)
        {
            if (hue < 30) return "Sunset Red";
            if (hue < 60) return "Toxic Green";
            if (hue < 120) return "Acid Lime";
            if (hue < 180) return "Neon Blue";
            if (hue < 240) return "Electric Purple";
            if (hue < 300) return "Magenta Dream";
            return "Cyberpunk Pink";
        }
    }
}
