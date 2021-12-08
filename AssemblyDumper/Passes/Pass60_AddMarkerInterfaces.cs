﻿using Mono.Cecil;
using System;

namespace AssemblyDumper.Passes
{
	public static class Pass60_AddMarkerInterfaces
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 60: Add Marker Interfaces");
			TryImplementInterface<AssetRipper.Core.Classes.IEditorExtension>("EditorExtension");
			TryImplementInterface<AssetRipper.Core.Classes.INamedObject>("NamedObject");
			TryImplementInterface<AssetRipper.Core.Classes.IGameManager>("GameManager");
			TryImplementInterface<AssetRipper.Core.Classes.IGlobalGameManager>("GlobalGameManager");
			TryImplementInterface<AssetRipper.Core.Classes.ILevelGameManager>("LevelGameManager");
			TryImplementInterface<AssetRipper.Core.Classes.ILightmapParameters>("LightmapParameters");
			TryImplementInterface<AssetRipper.Core.Classes.EditorSettings.IEditorSettings>("EditorSettings");
			TryImplementInterface<AssetRipper.Core.Classes.Meta.Importers.Asset.IAssetImporter>("AssetImporter");
			TryImplementInterface<AssetRipper.Core.Classes.Meta.Importers.IDefaultImporter>("DefaultImporter");
		}

		private static bool TryImplementInterface<T>(string typeName)
		{
			if (SharedState.TypeDictionary.TryGetValue(typeName, out TypeDefinition type))
			{
				type.AddInterface<T>();
				return true;
			}
			return false;
		}

		private static void AddInterface<T>(this TypeDefinition type)
		{
			TypeReference @interface = SharedState.Module.ImportCommonType<T>();
			type.Interfaces.Add(new InterfaceImplementation(@interface));
		}
	}
}
