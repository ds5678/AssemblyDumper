﻿using AssetRipper.AssemblyCreationTools.Methods;
using AssetRipper.AssemblyCreationTools.Types;
using AssetRipper.Assets.Interfaces;
using AssetRipper.Primitives;

namespace AssetRipper.AssemblyDumper.Passes
{
	internal static class Pass300_HasNameInterface
	{
		private const string Utf8PropertyName = "Name";
		private const string StringPropertyName = nameof(IHasNameString.NameString);

		private const string Utf8StringName = Pass002_RenameSubnodes.Utf8StringName;

		public static void DoPass()
		{
			TypeSignature utf8StringSignature = SharedState.Instance.Importer.ImportType<Utf8String>().ToTypeSignature();
			TypeDefinition hasNameInterface = MakeHasNameInterface(utf8StringSignature);
			foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
			{
				DoPassOnGroup(group, hasNameInterface, utf8StringSignature);
			}
		}

		private static TypeDefinition MakeHasNameInterface(TypeSignature utf8StringSignature)
		{
			TypeDefinition @interface = InterfaceCreator.CreateEmptyInterface(SharedState.Instance.Module, SharedState.InterfacesNamespace, "IHasName");
			@interface.AddFullProperty(Utf8PropertyName, InterfaceUtils.InterfacePropertyDeclaration, utf8StringSignature);
			@interface.AddInterfaceImplementation<IHasNameString>(SharedState.Instance.Importer);
			return @interface;
		}

		private static void DoPassOnGroup(ClassGroupBase group, TypeDefinition hasNameInterface, TypeSignature utf8StringSignature)
		{
			if (group.Types.All(t => t.TryGetNameField(true, out var _)))
			{
				TypeDefinition groupInterface = group.Interface;
				groupInterface.AddInterfaceImplementation(hasNameInterface);
				if (groupInterface.Properties.Any(p => p.Name == Utf8PropertyName))
				{
					throw new Exception("Interface already has a name property");
				}

				foreach (TypeDefinition type in group.Types)
				{
					if (type.TryGetNameField(false, out FieldDefinition? field))
					{
						type.ImplementNameProperties(field, utf8StringSignature);
					}
				}
			}
			else
			{
				foreach (TypeDefinition type in group.Types)
				{
					if (type.TryGetNameField(false, out FieldDefinition? field))
					{
						type.AddInterfaceImplementation(hasNameInterface);
						type.ImplementNameProperties(field, utf8StringSignature);
					}
				}
			}
		}

		private static void ImplementNameProperties(this TypeDefinition type, FieldDefinition field, TypeSignature utf8StringSignature)
		{
			if (!type.Properties.Any(p => p.Name == Utf8PropertyName))
			{
				type.ImplementFullProperty(Utf8PropertyName, InterfaceUtils.InterfacePropertyImplementation, utf8StringSignature, field);
			}
			if (!type.Properties.Any(p => p.Name == StringPropertyName))
			{
				type.ImplementStringProperty(StringPropertyName, InterfaceUtils.InterfacePropertyImplementation, field);
			}
		}

		private static PropertyDefinition ImplementStringProperty(this TypeDefinition type, string propertyName, MethodAttributes methodAttributes, FieldDefinition field)
		{
			PropertyDefinition property = type.AddFullProperty(propertyName, methodAttributes, SharedState.Instance.Importer.String);

			//Get method
			{
				IMethodDefOrRef getRef = SharedState.Instance.Importer.ImportMethod<Utf8String>(m => m.Name == $"get_{nameof(Utf8String.String)}");

				CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				processor.Add(CilOpCodes.Call, getRef);
				processor.Add(CilOpCodes.Ret);
			}

			//Set method
			{
				IMethodDefOrRef equalityMethod = SharedState.Instance.Importer.ImportMethod<Utf8String>(m =>
				{
					return m.Name == "op_Inequality" && m.Parameters[1].ParameterType is CorLibTypeSignature { ElementType: ElementType.String };
				});
				IMethodDefOrRef constructor = SharedState.Instance.Importer.ImportMethod<Utf8String>(m =>
				{
					return m.IsConstructor && m.Parameters.Count is 1 && m.Parameters[0].ParameterType is CorLibTypeSignature { ElementType: ElementType.String };
				});

				CilInstructionCollection processor = property.SetMethod!.CilMethodBody!.Instructions;

				CilInstructionLabel returnLabel = new();

				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				processor.Add(CilOpCodes.Ldarg_1);
				processor.Add(CilOpCodes.Call, equalityMethod);
				processor.Add(CilOpCodes.Brfalse_S, returnLabel);

				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldarg_1);
				processor.Add(CilOpCodes.Newobj, constructor);
				processor.Add(CilOpCodes.Stfld, field);

				returnLabel.Instruction = processor.Add(CilOpCodes.Nop);
				processor.Add(CilOpCodes.Ret);
			}

			return property;
		}

		private static bool TryGetNameField(this TypeDefinition type, bool checkBaseTypes, [NotNullWhen(true)] out FieldDefinition? field)
		{
			field = type.TryGetFieldByName("m_Name", checkBaseTypes);
			return field?.Signature?.FieldType.Name == Utf8StringName;
		}
	}
}
