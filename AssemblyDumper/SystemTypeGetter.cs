﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AssemblyDumper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyDumper
{
	public static class SystemTypeGetter
	{
		public static AssemblyDefinition RuntimeAssembly { get; set; }
		public static AssemblyDefinition CollectionsAssembly { get; set; }

		public static readonly Dictionary<string, string> CppPrimitivesToCSharpPrimitives = new()
		{
			{ "bool", "Boolean" },
			{ "char", "Byte" },
			{ "double", "Double" },
			{ "float", "Single" },
			{ "int", "Int32" },
			{ "long long", "Int64" },
			{ "short", "Int16" },
			{ "SInt16", "Int16" },
			{ "SInt32", "Int32" },
			{ "SInt64", "Int64" },
			{ "SInt8", "SByte" },
			{ "string", "String" },
			{ "UInt16", "UInt16" },
			{ "UInt32", "UInt32" },
			{ "UInt64", "UInt64" },
			{ "UInt8", "Byte" },
			{ "unsigned int", "UInt32" },
			{ "unsigned long long", "UInt64" },
			{ "unsigned short", "UInt16" },
			{ "Type*", "Int32" } //TODO Verify
		};

		public static readonly string[] primitiveNamesCsharp = new string[]
		{
			"void",
			"bool",
			"byte",
			"sbyte",
			"short",
			"ushort",
			"int",
			"uint",
			"long",
			"ulong",
			"float",
			"double",
			"decimal",
		};

		public static CorLibTypeSignature Boolean { get; private set; }
		public static CorLibTypeSignature Int8 { get; private set; }
		public static CorLibTypeSignature UInt8 { get; private set; }
		public static CorLibTypeSignature Int16 { get; private set; }
		public static CorLibTypeSignature UInt16 { get; private set; }
		public static CorLibTypeSignature Int32 { get; private set; }
		public static CorLibTypeSignature UInt32 { get; private set; }
		public static CorLibTypeSignature Int64 { get; private set; }
		public static CorLibTypeSignature UInt64 { get; private set; }
		public static CorLibTypeSignature Single { get; private set; }
		public static CorLibTypeSignature Double { get; private set; }
		public static CorLibTypeSignature String { get; private set; }
		public static CorLibTypeSignature Void { get; private set; }
		public static CorLibTypeSignature Object { get; private set; }
		public static ITypeDefOrRef Dictionary { get; private set; }
		public static ITypeDefOrRef List { get; private set; }
		public static ITypeDefOrRef Type { get; private set; }
		public static ITypeDefOrRef BinaryReader { get; private set; }
		public static IMethodDefOrRef NotSupportedExceptionConstructor { get; private set; }

		public static void Initialize(ModuleDefinition module)
		{
			Boolean = module.CorLibTypeFactory.Boolean;
			Int8 = module.CorLibTypeFactory.SByte;
			UInt8 = module.CorLibTypeFactory.Byte;
			Int16 = module.CorLibTypeFactory.Int16;
			UInt16 = module.CorLibTypeFactory.UInt16;
			Int32 = module.CorLibTypeFactory.Int32;
			UInt32 = module.CorLibTypeFactory.UInt32;
			Int64 = module.CorLibTypeFactory.Int64;
			UInt64 = module.CorLibTypeFactory.UInt64;
			Single = module.CorLibTypeFactory.Single;
			Double = module.CorLibTypeFactory.Double;
			String = module.CorLibTypeFactory.String;
			Dictionary = module.ImportSystemType("System.Collections.Generic.Dictionary`2");
			List = module.ImportSystemType("System.Collections.Generic.List`1");
			Type = module.ImportSystemType("System.Type");
			Void = module.CorLibTypeFactory.Void;
			Object = module.CorLibTypeFactory.Object;
			BinaryReader = module.ImportSystemType("System.IO.BinaryReader");
			NotSupportedExceptionConstructor = module.ImportSystemDefaultConstructor("System.NotSupportedException");
		}

		public static IMethodDefOrRef ImportSystemDefaultConstructor(this ModuleDefinition module, string typeFullName)
		{
			TypeDefinition type = LookupSystemType(typeFullName) ?? throw new Exception($"{typeFullName} not found in the system assemblies");
			return SharedState.Importer.ImportMethod(type.GetDefaultConstructor());
		}

		public static TypeDefinition LookupSystemType(string typeFullName)
		{
			return RuntimeAssembly.ManifestModule.TopLevelTypes.SingleOrDefault(t => t.GetTypeFullName() == typeFullName)
				?? CollectionsAssembly.ManifestModule.TopLevelTypes.SingleOrDefault(t => t.GetTypeFullName() == typeFullName);
		}
		public static TypeDefinition LookupSystemType(Type type) => LookupSystemType(type.FullName);
		public static TypeDefinition LookupSystemType<T>() => LookupSystemType(typeof(T));

		public static MethodDefinition LookupSystemMethod(string typeFullName, Func<MethodDefinition, bool> filter)
		{
			TypeDefinition type = LookupSystemType(typeFullName) ?? throw new Exception($"{typeFullName} not found in the system assemblies");
			return type.Methods.Single(filter);
		}
		public static MethodDefinition LookupSystemMethod(Type type, Func<MethodDefinition, bool> filter) => LookupSystemMethod(type.FullName, filter);
		public static MethodDefinition LookupSystemMethod<T>(Func<MethodDefinition, bool> filter) => LookupSystemMethod(typeof(T), filter);

		public static ITypeDefOrRef ImportSystemType(this ModuleDefinition module, string typeFullName) => SharedState.Importer.ImportType(LookupSystemType(typeFullName));
		public static ITypeDefOrRef ImportSystemType(this ModuleDefinition module, System.Type type) => SharedState.Importer.ImportType(LookupSystemType(type));
		public static ITypeDefOrRef ImportSystemType<T>(this ModuleDefinition module) => SharedState.Importer.ImportType(LookupSystemType<T>());

		public static IMethodDefOrRef ImportSystemMethod(this ModuleDefinition module, string typeFullName, Func<MethodDefinition, bool> filter)
		{
			return SharedState.Importer.ImportMethod(LookupSystemMethod(typeFullName, filter));
		}
		public static IMethodDefOrRef ImportSystemMethod(this ModuleDefinition module, System.Type type, Func<MethodDefinition, bool> filter)
		{
			return SharedState.Importer.ImportMethod(LookupSystemMethod(type, filter));
		}
		public static IMethodDefOrRef ImportSystemMethod<T>(this ModuleDefinition module, Func<MethodDefinition, bool> filter)
		{
			return SharedState.Importer.ImportMethod(LookupSystemMethod<T>(filter));
		}

		public static CorLibTypeSignature GetCSharpPrimitiveTypeSignature(string cppPrimitiveName) => cppPrimitiveName switch
		{
			"Boolean" => Boolean,
			"SByte" => Int8,
			"Int16" => Int16,
			"Int32" => Int32,
			"Int64" => Int64,
			"Byte" => UInt8,
			"UInt16" => UInt16,
			"UInt32" => UInt32,
			"UInt64" => UInt64,
			"Single" => Single,
			"Double" => Double,
			"String" => String,
			_ => null,
		};

		/// <summary>
		/// Gets the type signature for a cpp primitive type
		/// </summary>
		/// <param name="cppPrimitiveName">The name of a cpp primitive, ie long long</param>
		/// <remarks>
		/// Note: The type tree dumps only contain cpp primitive names
		/// </remarks>
		/// <returns>The CorLibTypeSignature associated with that cpp name, or null if it can't be found</returns>
		public static CorLibTypeSignature GetCppPrimitiveTypeSignature(string cppPrimitiveName) =>
			CppPrimitivesToCSharpPrimitives.TryGetValue(cppPrimitiveName, out var csPrimitiveName)
				? GetCSharpPrimitiveTypeSignature(csPrimitiveName)
				: null;
	}
}