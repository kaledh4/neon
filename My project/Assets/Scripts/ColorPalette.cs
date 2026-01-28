using UnityEngine;

namespace NeonSplash
{
    public class ColorPalette
    {
        public Color Primary;
        public Color Secondary;
        public Color Shooting;
        public Color Fog;
        public Color Background;
        public string PaletteName;

        public ColorPalette(int seed)
        {
            float hue1 = (seed * 137.508f) % 360f / 360f; 
            float hue2 = (hue1 + 0.3f) % 1.0f; // Complementary/Alternate
            float hueShoot = (hue1 + 0.7f) % 1.0f; // High contrast alternate

            Primary = Color.HSVToRGB(hue1, 0.8f, 1.0f);
            Secondary = Color.HSVToRGB(hue2, 0.7f, 0.9f);
            Shooting = Color.HSVToRGB(hueShoot, 0.9f, 1.0f);
            Fog = Color.HSVToRGB(hue1, 0.5f, 0.1f);
            Background = new Color(0.02f, 0.02f, 0.03f); 

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
