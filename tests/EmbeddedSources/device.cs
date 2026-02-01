#pragma warning disable IDE0005
using System;
#pragma warning restore IDE0005
using Bonsai.Harp;

namespace Interface.Tests
{
    public partial class CustomPayload
    {
        private static partial HarpVersion ParsePayload(uint[] payload)
        {
            return new HarpVersion((int)payload[0], (int)payload[1]);
        }

        private static partial uint[] FormatPayload(HarpVersion value)
        {
            return new[] { (uint)value.Major.GetValueOrDefault(), (uint)value.Minor.GetValueOrDefault() };
        }
    }

    public partial class CustomRawPayload
    {
        private static partial HarpVersion ParsePayload(ArraySegment<byte> payload)
        {
            return PayloadMarshal.ReadHarpVersion(payload);
        }

        private static partial ArraySegment<byte> FormatPayload(HarpVersion value)
        {
            var result = new ArraySegment<byte>(new byte[sizeof(uint) * RegisterLength]);
            PayloadMarshal.Write(result, value);
            return result;
        }
    }

    public partial class CustomMemberConverter
    {
        private static partial int ParsePayloadData(ArraySegment<byte> payloadData)
        {
            return PayloadMarshal.ReadInt16(payloadData);
        }

        private static partial byte[] FormatPayloadData(int data)
        {
            var result = new ArraySegment<byte>(new byte[2]);
            PayloadMarshal.Write(result, (short)data);
            return result.Array;
        }
    }
    
    public partial class BitmaskSplitter
    {
        private static partial int ParsePayloadLow(byte payloadLow)
        {
            return payloadLow;
        }

        private static partial byte FormatPayloadLow(int low)
        {
            return (byte)low;
        }
    }
}