namespace FairgroundAPI.Utilities
{
    /// <summary>
    /// Extracts human-readable color names from State_Light material names.
    /// </summary>
    public static class MaterialHelper
    {
        /// <summary>
        /// Parses the material name to extract the color portion.
        /// Strips the "SB-" prefix if present and returns everything before the first space.
        /// </summary>
        public static string ExtractColorName(State_Light light)
        {
            if (light.material == null) return "Unknown";

            string materialName = light.material.name;
            int startIndex = materialName.StartsWith("SB-") ? 3 : 0;
            int spaceIndex = materialName.IndexOf(' ', startIndex);

            return spaceIndex > 0
                ? materialName.Substring(startIndex, spaceIndex - startIndex)
                : materialName.Substring(startIndex);
        }
    }
}
