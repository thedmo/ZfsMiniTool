using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZfsMiniTool.Core.Extensions;
internal static class StringExtentions
{
    /// <summary>
    /// Converts a hex string (optionally containing spaces or new‑lines) into a byte array.
    /// Throws FormatException if the string contains non‑hex characters or an odd length.
    /// </summary>
    public static byte[] HexToBytes(this string hex)
    {
        // Remove any whitespace that might be present
        hex = string.Concat(hex.Where(c => !char.IsWhiteSpace(c)));

        if (hex.Length % 2 != 0)
            throw new FormatException("Hex string must contain an even number of characters.");

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            string pair = hex.Substring(i * 2, 2);
            bytes[i] = Convert.ToByte(pair, 16);
        }
        return bytes;
    }
}
