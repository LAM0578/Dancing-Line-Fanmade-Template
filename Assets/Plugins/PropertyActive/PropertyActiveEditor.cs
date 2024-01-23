#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(PropertyActiveAttribute), false)]
public class PropertyActiveEditor : PropertyDrawer
{
    //字段路径
    private string field;
    //比较的值
    private object compareValue;
    //比较类型
    private CompareType compare;
    //计算的值
    private object calculateValue;
    //计算类型
    private CalculateType calculate;
    //是否隐藏
    private bool flag;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //attribute是父类的，传过来的是属性值
        PropertyActiveAttribute attr = attribute as PropertyActiveAttribute;
        //对应变量赋值
        field = attr.FieldPath;
        compareValue = attr.CompareValue;
        compare = attr.Compare;
        calculate = attr.Calculate;
        calculateValue = attr.CalculateValue;
        //获取当前属性所在的类，例如当前属性是a.b，那么就是获取a
        var parent = property.GetActualObjectParent();
        //获取用于条件判断的字段信息，值以及类型
        var ComparePropertyField = parent?.GetType()
            .GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var ComparePropertyValue = ComparePropertyField?.GetValue(parent);
        var ComparePropertyType = ComparePropertyValue?.GetType();
        //如果计算的类型不为None说明要计算
        object calculateRes = ComparePropertyValue;
        if (calculate != CalculateType.None)
        {
            //目前仅支持int或enum（其实写这个主要为了enum判断）
            if (CanChangeToInt(ComparePropertyValue) && CanChangeToInt(calculateValue))
            {
                switch (attr.Calculate)
                {
                    case CalculateType.Or:
                        {
                            calculateRes = (int)ComparePropertyValue | (int)calculateValue;
                        }
                        break;
                    case CalculateType.And:
                        {
                            calculateRes = (int)ComparePropertyValue & (int)calculateValue;
                        }
                        break;
                    case CalculateType.Xor:
                        {
                            calculateRes = (int)ComparePropertyValue ^ (int)calculateValue;
                        }
                        break;
                    default:
                        {
                            return EditorGUI.GetPropertyHeight(property);
                        }
                }
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property);
            }
        }
        if (calculateRes == null) return EditorGUI.GetPropertyHeight(property);
        //等于和不等于直接调用object的Equals
        if (compare == CompareType.Equl)
        {
            flag = calculateRes.Equals(compareValue);
        }
        else if (compare == CompareType.NotEqul)
        {
            flag = !calculateRes.Equals(compareValue);
        }
        else if (ComparePropertyType == compareValue.GetType() && IsNumberType(ComparePropertyType))
        {
            //其他比较需要使用Comparer来比较两个object
            switch (compare)
            {
                case CompareType.Less:
                    {
                        flag = Comparer.DefaultInvariant.Compare(calculateRes, compareValue) < 0;
                    }
                    break;
                case CompareType.Greater:
                    {
                        flag = Comparer.DefaultInvariant.Compare(calculateRes, compareValue) > 0;
                    }
                    break;
                case CompareType.LessAndEqul:
                    {
                        flag = Comparer.DefaultInvariant.Compare(calculateRes, compareValue) <= 0;
                    }
                    break;
                case CompareType.GreaterAndEqul:
                    {
                        flag = Comparer.DefaultInvariant.Compare(calculateRes, compareValue) >= 0;
                    }
                    break;
            }
        }

        return flag ? 0 : EditorGUI.GetPropertyHeight(property);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //不隐藏再进行一个绘制
        if (flag) return;
        EditorGUI.PropertyField(position, property, label, true);
    }

    //判断是否是数值类型
    public static bool IsNumberType(Type type)
    {
        //IsPrimitive就是系统自带的类，IsValueType就是值类型，再排除char剩下的就是int，float这些了
        return (type.IsPrimitive && type.IsValueType && type != typeof(char));
    }
    //。。。。
    public static bool CanChangeToInt(object value)
    {
        try
        {
            int i = (int)value;
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static bool CanChangeToDouble(object value)
    {
        try
        {
            double i = (double)value;
            return true;
        }
        catch
        {
            return false;
        }
    }
}

#endif
