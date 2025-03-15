using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using ModLoader;
using static CardTCLib.LuaBridge.CommonValue;
using Base64 = gfoidl.Base64.Base64;

namespace CardTCLib.LuaBridge;

public readonly struct CommonValue
{
    public enum ValType : byte
    {
        String,
        Long,
        Double,
    }

    public readonly ValType Type;
    public readonly string Str;
    public readonly long L;
    public readonly double D;

    public CommonValue(object value)
    {
        Str = "";
        if (value is string s)
        {
            Str = s;
            Type = ValType.String;
        }
        else if (value is long l)
        {
            L = l;
            Type = ValType.Long;
        }
        else if (value is double d)
        {
            D = d;
            Type = ValType.Double;
        }
    }

    public object? Value => Type switch
    {
        ValType.String => Str,
        ValType.Long => L,
        ValType.Double => D % 1.0 == 0.0 ? (long)D : D,
        _ => null,
    };
}

public static class RWUtils
{
    public static void Write(this BinaryWriter writer, CommonValue value)
    {
        writer.Write((byte)value.Type);
        switch (value.Type)
        {
            case ValType.String:
                writer.Write(value.Str);
                break;
            case ValType.Long:
                writer.Write(value.L);
                break;
            case ValType.Double:
                writer.Write(value.D);
                break;
        }
    }

    public static void Write(this BinaryWriter writer, Dictionary<string, CommonValue> values)
    {
        writer.Write(values.Count);
        foreach (var (key, value) in values)
        {
            writer.Write(key);
            writer.Write(value);
        }
    }

    public static CommonValue ReadCommonValue(this BinaryReader reader)
    {
        var valType = (ValType)reader.ReadByte();
        switch (valType)
        {
            case ValType.String:
                var str = reader.ReadString();
                return new CommonValue(str);
            case ValType.Long:
                var l = reader.ReadInt64();
                return new CommonValue(l);
            case ValType.Double:
                var d = reader.ReadDouble();
                return new CommonValue(d);
        }

        return new CommonValue("");
    }

    public static Dictionary<string, CommonValue> ReadCommonValueTable(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var commonValues = new Dictionary<string, CommonValue>();
        for (var i = 0; i < count; i++)
        {
            var key = reader.ReadString();
            var value = reader.ReadCommonValue();
            commonValues[key] = value;
        }

        return commonValues;
    }

    private static void OverwriteCommonValueTable(this BinaryReader reader,
        Dictionary<string, CommonValue> table)
    {
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var key = reader.ReadString();
            var value = reader.ReadCommonValue();
            table[key] = value;
        }
    }

    public static Dictionary<string, CommonValue> DecodeB64CommonValueTable(string b64String,
        Dictionary<string, CommonValue>? table = null)
    {
        table ??= new Dictionary<string, CommonValue>();
        var decode = Base64.Default.Decode(b64String.ToCharArray());
        using var reader = new BinaryReader(new MemoryStream(decode));
        reader.OverwriteCommonValueTable(table);

        return table;
    }

    public static string EncodeB64CommonValueTable(this Dictionary<string, CommonValue> table)
    {
        var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);
        writer.Write(table);
        writer.Flush();
        var base64Str = Base64.Default.Encode(memoryStream.ToArray());
        return base64Str;
    }
}