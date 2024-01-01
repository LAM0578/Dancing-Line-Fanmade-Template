#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
public static class UnityEditorHelper
{
    public static T GetActualObject<T>(this SerializedProperty property)
    {
        try
        {
            if (property == null)
                return default(T);
            var serializedObject = property.serializedObject;
            if (serializedObject == null)
            {
                return default(T);
            }

            var targetObject = serializedObject.targetObject;

            //if (property.depth > 0)
            //{
            var slicedName = property.propertyPath.Split('.').ToList();
            List<int> arrayCounts = new List<int>();
            for (int index = 0; index < slicedName.Count; index++)
            {
                arrayCounts.Add(-1);
                var currName = slicedName[index];
                if (currName.EndsWith("]"))
                {
                    var arraySlice = currName.Split('[', ']');
                    if (arraySlice.Length >= 2)
                    {
                        arrayCounts[index - 2] = Convert.ToInt32(arraySlice[1]);
                        slicedName[index] = string.Empty;
                        slicedName[index - 1] = string.Empty;
                    }
                }
            }

            while (string.IsNullOrEmpty(slicedName.Last()))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }

            return DescendHierarchy<T>(targetObject, slicedName, arrayCounts, 0);
            //   }

            //   return default(T);
        }
        catch
        {
            return default(T);
        }
    }

    public static object GetActualObjectParent(this SerializedProperty property)
    {
        try
        {
            if (property == null)
                return default;
            //获取当前序列化的Object
            var serializedObject = property.serializedObject;
            if (serializedObject == null)
            {
                return default;
            }
            //获取targetObject，这里的targetObject就是
            //我不好描述直接举个例子：a.b.c.d.e.f,比如serializedObject就是f，那么targetObject就是a
            var targetObject = serializedObject.targetObject;
            //还是上面的例子propertyPath其实就是a.b.c.d.e.f
            //但是如果其中某一个是Array的话假设是b那么就会变成a.b.Array.data[x].c.d.e.f
            //其中x为index
            var slicedName = property.propertyPath.Split('.').ToList();
            List<int> arrayCounts = new List<int>();
            //根据"."分好后还需要获取其中的数组及其index保存在一个表中
            for (int index = 0; index < slicedName.Count; index++)
            {
                arrayCounts.Add(-1);
                var currName = slicedName[index];
                if (currName.EndsWith("]"))
                {
                    var arraySlice = currName.Split('[', ']');
                    if (arraySlice.Length >= 2)
                    {
                        arrayCounts[index - 2] = Convert.ToInt32(arraySlice[1]);
                        slicedName[index] = string.Empty;
                        slicedName[index - 1] = string.Empty;
                    }
                }
            }
            //清除数组导致的空
            while (string.IsNullOrEmpty(slicedName.Last()))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }
            //如果和属性名称相同则清除
            if (slicedName.Last().Equals(property.name))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }
            //如果空了那么返回targetObject为当前的父对象
            if (slicedName.Count == 0) return targetObject;
            //继续清除数组，防止父对象也是数组
            while (string.IsNullOrEmpty(slicedName.Last()))
            {
                int i = slicedName.Count - 1;
                slicedName.RemoveAt(i);
                arrayCounts.RemoveAt(i);
            }
            //如果空了那么返回targetObject为当前的父对象
            if (slicedName.Count == 0) return targetObject;
            //获取父物体
            return DescendHierarchy<object>(targetObject, slicedName, arrayCounts, 0);
        }
        catch // (Exception ex)
        {
            //Debug.LogException(ex);
            return default;
        }
    }
    //自己看
    static T DescendHierarchy<T>(object targetObject, List<string> splitName, List<int> splitCounts, int depth)
    {
        if (depth >= splitName.Count)
            return default(T);

        var currName = splitName[depth];

        if (string.IsNullOrEmpty(currName))
            return DescendHierarchy<T>(targetObject, splitName, splitCounts, depth + 1);

        int arrayIndex = splitCounts[depth];

        var newField = targetObject.GetType().GetField(currName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (newField == null)
        {
            Type baseType = targetObject.GetType().BaseType;
            while (baseType != null && newField == null)
            {
                newField = baseType.GetField(currName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                baseType = baseType.BaseType;
            }
        }

        var newObj = newField.GetValue(targetObject);
        if (depth == splitName.Count - 1)
        {
            T actualObject = default(T);
            if (arrayIndex >= 0)
            {
                if (newObj.GetType().IsArray && ((System.Array)newObj).Length > arrayIndex)
                    actualObject = (T)((System.Array)newObj).GetValue(arrayIndex);

                var newObjList = newObj as IList;
                if (newObjList != null && newObjList.Count > arrayIndex)
                {
                    actualObject = (T)newObjList[arrayIndex];
                }
            }
            else
            {
                actualObject = (T)newObj;
            }

            return actualObject;
        }
        else if (arrayIndex >= 0)
        {
            if (newObj is IList)
            {
                IList list = (IList)newObj;
                newObj = list[arrayIndex];
            }
            else if (newObj is System.Array)
            {
                System.Array a = (System.Array)newObj;
                newObj = a.GetValue(arrayIndex);
            }
        }

        return DescendHierarchy<T>(newObj, splitName, splitCounts, depth + 1);
    }
}
#endif
