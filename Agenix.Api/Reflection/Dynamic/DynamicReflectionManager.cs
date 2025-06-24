#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using Agenix.Api.Util;
using NetDynamicMethod = System.Reflection.Emit.DynamicMethod;

namespace Agenix.Api.Reflection.Dynamic;

/// <summary>
///     Represents a Get method
/// </summary>
/// <param name="target">the target instance when calling an instance method</param>
/// <returns>the value return by the Get method</returns>
public delegate object FieldGetterDelegate(object target);

/// <summary>
///     Represents a Set method
/// </summary>
/// <param name="target">the target instance when calling an instance method</param>
/// <param name="value">the value to be set</param>
public delegate void FieldSetterDelegate(object target, object value);

/// <summary>
///     Represents an Indexer Get method
/// </summary>
/// <param name="target">the target instance when calling an instance method</param>
/// <param name="index"></param>
/// <returns>the value return by the Get method</returns>
public delegate object PropertyGetterDelegate(object target, params object[] index);

/// <summary>
///     Represents a Set method
/// </summary>
/// <param name="target">the target instance when calling an instance method</param>
/// <param name="value">the value to be set</param>
/// <param name="index"></param>
public delegate void PropertySetterDelegate(object target, object value, params object[] index);

/// <summary>
///     Represents a method
/// </summary>
/// <param name="target">the target instance when calling an instance method</param>
/// <param name="args">arguments to be passed to the method</param>
/// <returns>
///     the value return by the method.
///     <value>null</value>
///     when calling a void method
/// </returns>
public delegate object FunctionDelegate(object target, params object[] args);

/// <summary>
///     Represents a constructor
/// </summary>
/// <param name="args">arguments to be passed to the method</param>
/// <returns>the new object instance</returns>
public delegate object ConstructorDelegate(params object[] args);

/// <summary>
///     Allows easy access to existing and creation of new dynamic relection members.
/// </summary>
public sealed class DynamicReflectionManager
{
    private static readonly OpCode[] LdArgOpCodes = { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2 };

    private static readonly MethodInfo FnGetTypeFromHandle =
        new GetTypeFromHandleDelegate(Type.GetTypeFromHandle).Method;

    private static readonly MethodInfo FnConvertArgumentIfNecessary =
        new ChangeTypeDelegate(ConvertValueTypeArgumentIfNecessary).Method;

    private static readonly ConstructorInfo NewInvalidOperationException =
        typeof(InvalidOperationException).GetConstructor([typeof(string)]);

    /// <summary>
    ///     Create a new Get method delegate for the specified field using <see cref="System.Reflection.Emit.DynamicMethod" />
    /// </summary>
    /// <param name="fieldInfo">the field to create the delegate for</param>
    /// <returns>a delegate that can be used to read the field</returns>
    public static FieldGetterDelegate CreateFieldGetter(FieldInfo fieldInfo)
    {
        AssertUtils.ArgumentNotNull(fieldInfo, "You cannot create a delegate for a null value.");

        var skipVisibility = true; //!IsPublic(fieldInfo);
        Type[] argumentTypes = [typeof(object)];
        var dmGetter = CreateDynamicMethod("get_" + fieldInfo.Name, typeof(object), argumentTypes, fieldInfo,
            skipVisibility);
        var il = dmGetter.GetILGenerator();
        EmitFieldGetter(il, fieldInfo, false);
        return (FieldGetterDelegate)dmGetter.CreateDelegate(typeof(FieldGetterDelegate));
    }

    /// <summary>
    ///     Create a new Set method delegate for the specified field using <see cref="System.Reflection.Emit.DynamicMethod" />
    /// </summary>
    /// <param name="fieldInfo">the field to create the delegate for</param>
    /// <returns>a delegate that can be used to read the field.</returns>
    /// <remarks>
    ///     If the field's <see cref="FieldInfo.IsLiteral" /> returns true, the returned method
    ///     will throw an <see cref="InvalidOperationException" /> when called.
    /// </remarks>
    public static FieldSetterDelegate CreateFieldSetter(FieldInfo fieldInfo)
    {
        AssertUtils.ArgumentNotNull(fieldInfo, "You cannot create a delegate for a null value.");

        var skipVisibility = true; // !IsPublic(fieldInfo);
        var dmSetter = CreateDynamicMethod("set_" + fieldInfo.Name, null, new[] { typeof(object), typeof(object) },
            fieldInfo, skipVisibility);
        var il = dmSetter.GetILGenerator();
        EmitFieldSetter(il, fieldInfo, false);
        return (FieldSetterDelegate)dmSetter.CreateDelegate(typeof(FieldSetterDelegate));
    }

    /// <summary>
    ///     Create a new Get method delegate for the specified property using
    ///     <see cref="System.Reflection.Emit.DynamicMethod" />
    /// </summary>
    /// <param name="propertyInfo">the property to create the delegate for</param>
    /// <returns>a delegate that can be used to read the property.</returns>
    /// <remarks>
    ///     If the property's <see cref="PropertyInfo.CanRead" /> returns false, the returned method
    ///     will throw an <see cref="InvalidOperationException" /> when called.
    /// </remarks>
    public static PropertyGetterDelegate CreatePropertyGetter(PropertyInfo propertyInfo)
    {
        AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a delegate for a null value.");

        var getMethod = propertyInfo.GetGetMethod();
        var skipVisibility = true; // (null == getMethod || !IsPublic(getMethod)); // getter is public
        var dm = CreateDynamicMethod("get_" + propertyInfo.Name, typeof(object),
            new[] { typeof(object), typeof(object[]) }, propertyInfo, skipVisibility);
        var il = dm.GetILGenerator();
        EmitPropertyGetter(il, propertyInfo, false);
        return (PropertyGetterDelegate)dm.CreateDelegate(typeof(PropertyGetterDelegate));
    }

    /// <summary>
    ///     Create a new Set method delegate for the specified property using
    ///     <see cref="System.Reflection.Emit.DynamicMethod" />
    /// </summary>
    /// <param name="propertyInfo">the property to create the delegate for</param>
    /// <returns>a delegate that can be used to write the property.</returns>
    /// <remarks>
    ///     If the property's <see cref="PropertyInfo.CanWrite" /> returns false, the returned method
    ///     will throw an <see cref="InvalidOperationException" /> when called.
    /// </remarks>
    public static PropertySetterDelegate CreatePropertySetter(PropertyInfo propertyInfo)
    {
        AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a delegate for a null value.");

        var setMethod = propertyInfo.GetSetMethod();
        var skipVisibility = true; // (null == setMethod || !IsPublic(setMethod)); // setter is public
        var argumentTypes = new[] { typeof(object), typeof(object), typeof(object[]) };
        var dm = CreateDynamicMethod("set_" + propertyInfo.Name, null, argumentTypes, propertyInfo, skipVisibility);
        var il = dm.GetILGenerator();
        EmitPropertySetter(il, propertyInfo, false);
        return (PropertySetterDelegate)dm.CreateDelegate(typeof(PropertySetterDelegate));
    }

    /// <summary>
    ///     Create a new method delegate for the specified method using <see cref="System.Reflection.Emit.DynamicMethod" />
    /// </summary>
    /// <param name="methodInfo">the method to create the delegate for</param>
    /// <returns>a delegate that can be used to invoke the method.</returns>
    public static FunctionDelegate CreateMethod(MethodInfo methodInfo)
    {
        AssertUtils.ArgumentNotNull(methodInfo, "You cannot create a delegate for a null value.");

        var skipVisibility = true; // !IsPublic(methodInfo);
        var dm = CreateDynamicMethod(methodInfo.Name, typeof(object), new[] { typeof(object), typeof(object[]) },
            methodInfo, skipVisibility);
        var il = dm.GetILGenerator();
        EmitInvokeMethod(il, methodInfo, false);
        return (FunctionDelegate)dm.CreateDelegate(typeof(FunctionDelegate));
    }

    /// <summary>
    ///     Creates a new delegate for the specified constructor.
    /// </summary>
    /// <param name="constructorInfo">the constructor to create the delegate for</param>
    /// <returns>delegate that can be used to invoke the constructor.</returns>
    public static ConstructorDelegate CreateConstructor(ConstructorInfo constructorInfo)
    {
        AssertUtils.ArgumentNotNull(constructorInfo, "You cannot create a dynamic constructor for a null value.");

        var skipVisibility = true; //!IsPublic(constructorInfo);
        DynamicMethod dmGetter;
        var argumentTypes = new[] { typeof(object[]) };
        dmGetter = CreateDynamicMethod(constructorInfo.Name, typeof(object), argumentTypes, constructorInfo,
            skipVisibility);
        var il = dmGetter.GetILGenerator();
        EmitInvokeConstructor(il, constructorInfo, false);
        var ctor = (ConstructorDelegate)dmGetter.CreateDelegate(typeof(ConstructorDelegate));
        return ctor;
    }

    /// <summary>
    ///     Creates a <see cref="System.Reflection.Emit.DynamicMethod" /> instance with the highest possible code access
    ///     security.
    /// </summary>
    /// <remarks>
    ///     If allowed by security policy, associates the method with the <paramref name="member" />s declaring type.
    ///     Otherwise associates the dynamic method with <see cref="DynamicReflectionManager" />.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static NetDynamicMethod CreateDynamicMethod(string methodName, Type returnType, Type[] argumentTypes,
        MemberInfo member, bool skipVisibility)
    {
        NetDynamicMethod dmGetter = null;
        methodName = "_dynamic_" + member.DeclaringType.FullName + "." + methodName;
        try
        {
#if !NETSTANDARD
            new PermissionSet(PermissionState.Unrestricted).Demand();
#endif
            dmGetter = CreateDynamicMethodInternal(methodName, returnType, argumentTypes, member, skipVisibility);
        }
        catch (SecurityException)
        {
            dmGetter = CreateDynamicMethodInternal(methodName, returnType, argumentTypes, MethodBase.GetCurrentMethod(),
                false);
        }

        return dmGetter;
    }

    private static NetDynamicMethod CreateDynamicMethodInternal(string methodName, Type returnType,
        Type[] argumentTypes, MemberInfo member, bool skipVisibility)
    {
        NetDynamicMethod dm;
        dm = new NetDynamicMethod(methodName, returnType, argumentTypes, member.Module, skipVisibility);
        return dm;
    }

    private static void SetupOutputArgument(ILGenerator il, int paramsArrayPosition, ParameterInfo argInfo,
        IDictionary outArgs)
    {
        if (!IsOutputOrRefArgument(argInfo))
        {
            return;
        }

        var argType = argInfo.ParameterType.GetElementType();

        var lb = il.DeclareLocal(argType);
        if (!argInfo.IsOut)
        {
            PushParamsArgumentValue(il, paramsArrayPosition, argType, argInfo.Position);
            il.Emit(OpCodes.Stloc, lb);
        }

        outArgs[argInfo.Position] = lb;
    }

    private static bool IsOutputOrRefArgument(ParameterInfo argInfo)
    {
        return argInfo.IsOut || argInfo.ParameterType.Name.EndsWith("&");
    }

    private static void ProcessOutputArgument(ILGenerator il, int paramsArrayPosition, ParameterInfo argInfo,
        IDictionary outArgs)
    {
        if (!IsOutputOrRefArgument(argInfo))
        {
            return;
        }

        var argType = argInfo.ParameterType.GetElementType();

        il.Emit(LdArgOpCodes[paramsArrayPosition]);
        il.Emit(OpCodes.Ldc_I4, argInfo.Position);
        il.Emit(OpCodes.Ldloc, (LocalBuilder)outArgs[argInfo.Position]);
        if (argType.IsValueType)
        {
            il.Emit(OpCodes.Box, argType);
        }

        il.Emit(OpCodes.Stelem_Ref);
    }

    private static void SetupMethodArgument(ILGenerator il, int paramsArrayPosition, ParameterInfo argInfo,
        IDictionary outArgs)
    {
        if (IsOutputOrRefArgument(argInfo))
        {
            il.Emit(OpCodes.Ldloca_S, (LocalBuilder)outArgs[argInfo.Position]);
        }
        else
        {
            PushParamsArgumentValue(il, paramsArrayPosition, argInfo.ParameterType, argInfo.Position);
        }
    }

    private static void PushParamsArgumentValue(ILGenerator il, int paramsArrayPosition, Type argumentType,
        int argumentPosition)
    {
        il.Emit(LdArgOpCodes[paramsArrayPosition]);
        il.Emit(OpCodes.Ldc_I4, argumentPosition);
        il.Emit(OpCodes.Ldelem_Ref);
        if (argumentType.IsValueType)
        {
            // call ConvertArgumentIfNecessary() to convert e.g. int32 to double if necessary
            il.Emit(OpCodes.Ldtoken, argumentType);
            EmitCall(il, FnGetTypeFromHandle);
            il.Emit(OpCodes.Ldc_I4, argumentPosition);
            EmitCall(il, FnConvertArgumentIfNecessary);
            EmitUnbox(il, argumentType);
        }
        else
        {
            il.Emit(OpCodes.Castclass, argumentType);
        }
    }

    private static void EmitUnbox(ILGenerator il, Type argumentType)
    {
        il.Emit(OpCodes.Unbox_Any, argumentType);
    }

    /// <summary>
    ///     Generates code to process return value if necessary.
    /// </summary>
    /// <param name="il">IL generator to use.</param>
    /// <param name="returnValueType">Type of the return value.</param>
    private static void EmitMethodReturn(ILGenerator il, Type returnValueType)
    {
        if (returnValueType == typeof(void))
        {
            il.Emit(OpCodes.Ldnull);
        }
        else if (returnValueType.IsValueType)
        {
            il.Emit(OpCodes.Box, returnValueType);
        }

        il.Emit(OpCodes.Ret);
    }

    /// <summary>
    ///     Converts <paramref name="value" /> to an instance of <paramref name="targetType" /> if necessary to
    ///     e.g. avoid e.g. double/int cast exceptions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method mimics the behavior of the compiler that
    ///         automatically performs casts like int to double in "Math.Sqrt(4)".<br />
    ///         See about implicit, widening type conversions on
    ///         <a href="http://social.msdn.microsoft.com/Search/en-US/?query=type conversion tables">
    ///             MSDN - Type Conversion
    ///             Tables
    ///         </a>
    ///     </para>
    ///     <para>
    ///         Note: <paramref name="targetType" /> is expected to be a value type!
    ///     </para>
    /// </remarks>
    public static object ConvertValueTypeArgumentIfNecessary(object value, Type targetType, int argIndex)
    {
        if (value == null)
        {
            if (ReflectionHelper.IsNullableType(targetType))
            {
                return null;
            }

            throw new InvalidCastException(
                $"Cannot convert NULL at position {argIndex} to argument type {targetType.FullName}");
        }

        var valueType = value.GetType();

        if (ReflectionHelper.IsNullableType(targetType))
        {
            targetType = Nullable.GetUnderlyingType(targetType);
        }

        // no conversion necessary?
        if (valueType == targetType)
        {
            return value;
        }

        if (!valueType.IsValueType)
        // we're facing a reftype/valuetype mix that never can convert
        {
            throw new InvalidCastException(
                $"Cannot convert value '{value}' of type {valueType.FullName} at position {argIndex} to argument type {targetType.FullName}");
        }

        // we're dealing only with ValueType's now - try to convert them
        try
        {
            // TODO: allow widening conversions only
            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException(
                $"Cannot convert value '{value}' of type {valueType.FullName} at position {argIndex} to argument type {targetType.FullName}",
                ex);
        }
    }

    private static void EmitTarget(ILGenerator il, Type targetType, bool isInstanceMethod)
    {
        il.Emit(isInstanceMethod ? OpCodes.Ldarg_1 : OpCodes.Ldarg_0);
        if (targetType.IsValueType)
        {
            var local = il.DeclareLocal(targetType);
            EmitUnbox(il, targetType);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloca_S, 0);
        }
        else
        {
            il.Emit(OpCodes.Castclass, targetType);
        }
    }

    private static void EmitCall(ILGenerator il, MethodInfo method)
    {
        il.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
    }

    private static void EmitConstant(ILGenerator il, object value)
    {
        if (value is string stringValue)
        {
            il.Emit(OpCodes.Ldstr, stringValue);
            return;
        }

        if (value is bool booleanValue)
        {
            if (booleanValue)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }

            return;
        }

        if (value is char charValue)
        {
            il.Emit(OpCodes.Ldc_I4, charValue);
            il.Emit(OpCodes.Conv_I2);
            return;
        }

        if (value is byte byteValue)
        {
            il.Emit(OpCodes.Ldc_I4_S, byteValue);
            il.Emit(OpCodes.Conv_I1);
        }
        else if (value is short int16Value)
        {
            il.Emit(OpCodes.Ldc_I4, int16Value);
            il.Emit(OpCodes.Conv_I2);
        }
        else if (value is int intValue)
        {
            il.Emit(OpCodes.Ldc_I4, intValue);
        }
        else if (value is long longValue)
        {
            il.Emit(OpCodes.Ldc_I8, longValue);
        }
        else if (value is ushort ushortValue)
        {
            il.Emit(OpCodes.Ldc_I4, ushortValue);
            il.Emit(OpCodes.Conv_U2);
        }
        else if (value is uint unsignedInt)
        {
            il.Emit(OpCodes.Ldc_I4, unsignedInt);
            il.Emit(OpCodes.Conv_U4);
        }
        else if (value is ulong unsignedLongValue)
        {
            il.Emit(OpCodes.Ldc_I8, unsignedLongValue);
            il.Emit(OpCodes.Conv_U8);
        }
        else if (value is float s)
        {
            il.Emit(OpCodes.Ldc_R4, s);
        }
        else if (value is double d)
        {
            il.Emit(OpCodes.Ldc_R8, d);
        }
    }

    /// <summary>
    ///     Generates code that throws <see cref="InvalidOperationException" />.
    /// </summary>
    /// <param name="il">IL generator to use.</param>
    /// <param name="message">Error message to use.</param>
    private static void EmitThrowInvalidOperationException(ILGenerator il, string message)
    {
        il.Emit(OpCodes.Ldstr, message);
        il.Emit(OpCodes.Newobj, NewInvalidOperationException);
        il.Emit(OpCodes.Throw);
    }

    private delegate Type GetTypeFromHandleDelegate(RuntimeTypeHandle handle);

    private delegate object ChangeTypeDelegate(object value, Type targetType, int argIndex);

    #region Shared Code Generation

    private static void EmitFieldGetter(ILGenerator il, FieldInfo fieldInfo, bool isInstanceMethod)
    {
        if (fieldInfo.IsLiteral)
        {
            var value = fieldInfo.GetValue(null);
            EmitConstant(il, value);
        }
        else if (fieldInfo.IsStatic)
        {
            //                object v = fieldInfo.GetValue(null); // ensure type is initialized...
            il.Emit(OpCodes.Ldsfld, fieldInfo);
        }
        else
        {
            EmitTarget(il, fieldInfo.DeclaringType, isInstanceMethod);
            il.Emit(OpCodes.Ldfld, fieldInfo);
        }

        if (fieldInfo.FieldType.IsValueType)
        {
            il.Emit(OpCodes.Box, fieldInfo.FieldType);
        }

        il.Emit(OpCodes.Ret);
    }

    internal static void EmitFieldSetter(ILGenerator il, FieldInfo fieldInfo, bool isInstanceMethod)
    {
        if (!fieldInfo.IsLiteral
            && !fieldInfo.IsInitOnly
            && !(fieldInfo.DeclaringType.IsValueType && !fieldInfo.IsStatic))
        {
            if (!fieldInfo.IsStatic)
            {
                EmitTarget(il, fieldInfo.DeclaringType, isInstanceMethod);
            }

            il.Emit(OpCodes.Ldarg_1);
            if (fieldInfo.FieldType.IsValueType)
            {
                EmitUnbox(il, fieldInfo.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
            }

            if (fieldInfo.IsStatic)
            {
                il.Emit(OpCodes.Stsfld, fieldInfo);
            }
            else
            {
                il.Emit(OpCodes.Stfld, fieldInfo);
            }

            il.Emit(OpCodes.Ret);
        }
        else
        {
            EmitThrowInvalidOperationException(il,
                string.Format("Cannot write to read-only field '{0}.{1}'", fieldInfo.DeclaringType.FullName,
                    fieldInfo.Name));
        }
    }

    internal static void EmitPropertyGetter(ILGenerator il, PropertyInfo propertyInfo, bool isInstanceMethod)
    {
        if (propertyInfo.CanRead)
        {
            var getMethod = propertyInfo.GetGetMethod(true);
            EmitInvokeMethod(il, getMethod, isInstanceMethod);
        }
        else
        {
            EmitThrowInvalidOperationException(il,
                string.Format("Cannot read from write-only property '{0}.{1}'", propertyInfo.DeclaringType.FullName,
                    propertyInfo.Name));
        }
    }

    internal static void EmitPropertySetter(ILGenerator il, PropertyInfo propertyInfo, bool isInstanceMethod)
    {
        var method = propertyInfo.GetSetMethod(true);

        if (propertyInfo.CanWrite
            && !(propertyInfo.DeclaringType.IsValueType && !method.IsStatic))
        {
            // Note: last arg is property value!
            // property set method signature:
            // void set_MyOtherProperty( [indexArg1, indexArg2, ...], string value)

            const int paramsArrayPosition = 2;
            IDictionary outArgs = new Hashtable();
            var args = propertyInfo.GetIndexParameters(); // get indexParameters here!
            foreach (var p in args)
            {
                SetupOutputArgument(il, paramsArrayPosition, p, outArgs);
            }

            // load target
            if (!method.IsStatic)
            {
                EmitTarget(il, method.DeclaringType, isInstanceMethod);
            }

            // load indexer arguments
            for (var i = 0; i < args.Length; i++)
            {
                SetupMethodArgument(il, paramsArrayPosition, args[i], outArgs);
            }

            // load value
            il.Emit(OpCodes.Ldarg_1);
            if (propertyInfo.PropertyType.IsValueType)
            {
                EmitUnbox(il, propertyInfo.PropertyType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            }

            // call setter
            EmitCall(il, method);

            foreach (var p in args)
            {
                ProcessOutputArgument(il, paramsArrayPosition, p, outArgs);
            }

            il.Emit(OpCodes.Ret);
        }
        else
        {
            EmitThrowInvalidOperationException(il,
                $"Cannot write to read-only property '{propertyInfo.DeclaringType.FullName}.{propertyInfo.Name}'");
        }
    }

    /// <summary>
    ///     Delegates a Method(object target, params object[] args) call to the actual underlying method.
    /// </summary>
    internal static void EmitInvokeMethod(ILGenerator il, MethodInfo method, bool isInstanceMethod)
    {
        var paramsArrayPosition = isInstanceMethod ? 2 : 1;
        var args = method.GetParameters();
        IDictionary outArgs = new Hashtable();
        foreach (var p in args)
        {
            SetupOutputArgument(il, paramsArrayPosition, p, outArgs);
        }

        if (!method.IsStatic)
        {
            EmitTarget(il, method.DeclaringType, isInstanceMethod);
        }

        foreach (var p in args)
        {
            SetupMethodArgument(il, paramsArrayPosition, p, outArgs);
        }

        EmitCall(il, method);

        foreach (var t in args)
        {
            ProcessOutputArgument(il, paramsArrayPosition, t, outArgs);
        }

        EmitMethodReturn(il, method.ReturnType);
    }

    internal static void EmitInvokeConstructor(ILGenerator il, ConstructorInfo constructor, bool isInstanceMethod)
    {
        var paramsArrayPosition = isInstanceMethod ? 1 : 0;
        var args = constructor.GetParameters();

        IDictionary outArgs = new Hashtable();
        foreach (var p in args)
        {
            SetupOutputArgument(il, paramsArrayPosition, p, outArgs);
        }

        foreach (var p in args)
        {
            SetupMethodArgument(il, paramsArrayPosition, p, null);
        }

        il.Emit(OpCodes.Newobj, constructor);

        foreach (var p in args)
        {
            ProcessOutputArgument(il, paramsArrayPosition, p, outArgs);
        }

        EmitMethodReturn(il, constructor.DeclaringType);
    }

    #endregion
}
