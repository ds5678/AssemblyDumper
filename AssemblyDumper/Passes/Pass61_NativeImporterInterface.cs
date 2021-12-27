﻿using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AssemblyDumper.Utils;
using System;
using AssetRipper.Core.Classes.Meta.Importers;

namespace AssemblyDumper.Passes
{
	public static class Pass61_NativeImporterInterface
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;
		const string PropertyName = "MainObjectFileID";
		const string FieldName = "m_" + PropertyName;

		public static void DoPass()
		{
			Console.WriteLine("Pass 61: Implement Native Format Importer Interface");
			ITypeDefOrRef nativeImporterInterface = SharedState.Importer.ImportCommonType<INativeFormatImporter>();
			if (SharedState.TypeDictionary.TryGetValue("NativeFormatImporter", out TypeDefinition type))
			{
				type.Interfaces.Add(new InterfaceImplementation(nativeImporterInterface));
				type.TryGetFieldByName(FieldName, out FieldDefinition field);
				type.ImplementFullProperty(PropertyName, InterfacePropertyImplementationAttributes, SystemTypeGetter.Int64, field);
			}
			else
			{
				throw new Exception("NativeFormatImporter not found");
			}
		}
	}
}
