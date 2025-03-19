using System.Text;

namespace CourseProject.Lab4;

public class BitPacker
{
    public static byte[] PackBits(string bitString)
    {
        var numOfBytes = (bitString.Length + 7) / 8;
        var bytes = new byte[numOfBytes];
        for (var i = 0; i < numOfBytes; i++)
        {
            var remaining = bitString.Length - i * 8;
            var byteString = remaining >= 8 
                ? bitString.Substring(i * 8, 8) 
                : bitString.Substring(i * 8).PadRight(8, '0');
            bytes[i] = Convert.ToByte(byteString, 2);
        }
        return bytes;
    }
    public static string UnpackBits(byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
        }
        return sb.ToString();
    }
}