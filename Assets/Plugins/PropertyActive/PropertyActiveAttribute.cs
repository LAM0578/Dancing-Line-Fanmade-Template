using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CompareType
{
    NotEqul = 0,
    Less = 1 << 0,
    Equl = 1 << 1,
    Greater = 1 << 2,
    LessAndEqul = Less | Equl,
    GreaterAndEqul = Greater | Equl,
}
public enum CalculateType
{
    None = 0,
    /// <summary>
    ///或
    /// <summary>
    Or = 1 << 0,
    /// <summary>
    ///与
    /// <summary>
    And = 1 << 1,
    /// <summary>
    ///异或
    /// <summary>
    Xor = 1 << 2,
    /// <summary>
    /// 取反运算,有点问题，取反是单目运算暂时废弃
    /// </summary>
    Not = 1 << 3,
}
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class PropertyActiveAttribute : PropertyAttribute
{
    //字段路径
    public string FieldPath { get; set; }
    //说明一下，逻辑是这样的
    //获取字段路径所在位置的字段，然后如果需要计算则与计算的值进行一个计算
    //然后使用计算得到的值和比较的值进行对应的比较，如果为真那么会隐藏掉
    //比较类型
    public CompareType Compare { get; set; }
    //比较的值
    public object CompareValue { get; set; }
    //计算的类型
    public CalculateType Calculate { get; set; } = CalculateType.None;
    //计算的值
    public object CalculateValue { get; set; }

    public PropertyActiveAttribute(string fieldPath, object value, CompareType compare = CompareType.Equl)
    {
        FieldPath = fieldPath;
        CompareValue = value;
        Compare = compare;
    }
    public PropertyActiveAttribute(string fieldPath, CalculateType calculate, object calculateValue, object value, CompareType compare = CompareType.Equl)
    {
        FieldPath = fieldPath;
        Calculate = calculate;
        CalculateValue = calculateValue;
        CompareValue = value;
        Compare = compare;
    }
}
