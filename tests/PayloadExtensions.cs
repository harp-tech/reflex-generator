#pragma warning disable IDE0005
using System;
#pragma warning restore IDE0005
using Bonsai.Harp;

namespace Interface.Tests;

internal static partial class PayloadExtensions
{
    internal static HarpVersion ToHarpVersion(this ArraySegment<byte> segment)
    {
        var major = segment.Array[segment.Offset];
        var minor = segment.Array[segment.Offset + 1];
        return new HarpVersion(major, minor);
    }

    internal static void WriteBytes(this ArraySegment<byte> segment, HarpVersion value)
    {
        segment.Array[segment.Offset] = (byte)value.Major.GetValueOrDefault();
        segment.Array[segment.Offset + 1] = (byte)value.Major.GetValueOrDefault();
    }
}