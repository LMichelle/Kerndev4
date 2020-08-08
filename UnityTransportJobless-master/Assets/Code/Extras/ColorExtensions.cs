using UnityEngine;

public static class ColorExtensions
{
    public static uint ColorToUInt(this Color color) => ((Color32)color).ColorToUInt();
    public static Color ColorFromUInt(this Color color, uint value) => ((Color32)color).ColorFromUInt(value);

    public static uint ColorToUInt(this Color32 color32)
    {
        return (uint)((color32[0] << 24) | (color32[1] << 16) | (color32[2] << 8) | color32[3]);
    }

    public static Color32 ColorFromUInt(this Color32 color32, uint value)
    {
        color32[0] = (byte)((value >> 24) & 0xFF);
        color32[1] = (byte)((value >> 16) & 0xFF);
        color32[2] = (byte)((value >> 8) & 0xFF);
        color32[3] = (byte)((value) & 0xFF);

        return color32;
    }

    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }
}
