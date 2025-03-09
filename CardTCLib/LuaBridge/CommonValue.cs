using System;
using System.Collections.Generic;
using System.IO;
using ModLoader;
using static CardTCLib.LuaBridge.CommonValue;

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
        ValType.Double => D,
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
}