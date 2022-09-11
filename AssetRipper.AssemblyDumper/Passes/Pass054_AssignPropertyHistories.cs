﻿using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Passes
{
	internal static class Pass054_AssignPropertyHistories
	{
		public static void DoPass()
		{
			foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
			{
				foreach (GeneratedClassInstance instance in group.Instances)
				{
					if (instance.History is null)
					{
						continue;
					}

					IReadOnlyDictionary<string, DataMemberHistory> members = instance.History.GetAllMembers(instance.VersionRange.Start, SharedState.Instance.HistoryFile);
					foreach (ClassProperty classProperty in instance.Properties)
					{
						SetHistory(classProperty, members);
					}
				}
				foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
				{
					interfaceProperty.History = interfaceProperty.DetermineHistoryFromImplementations();
				}
			}
		}

		private static void SetHistory(ClassProperty classProperty, IReadOnlyDictionary<string, DataMemberHistory> dictionary)
		{
			if (classProperty.OriginalFieldName is not null && dictionary.TryGetValue(classProperty.OriginalFieldName, out DataMemberHistory? history))
			{
			}
			else if (classProperty.BackingField is not null)
			{
				if (dictionary.TryGetValue(classProperty.BackingField.Name!, out history))
				{
				}
				else
				{
					history = dictionary.FirstOrDefault(pair => HistoryIsApplicable(pair.Value, classProperty.BackingField)).Value;
				}
			}
			else
			{
				history = null;
			}
			classProperty.History = history;
		}

		private static bool HistoryIsApplicable(DataMemberHistory history, FieldDefinition field)
		{
			string historyNameNormalized = Pass002_RenameSubnodes.GetValidFieldName(history.Name).ToLowerInvariant();
			string fieldName = field.Name!.ToString().ToLowerInvariant();
			if (historyNameNormalized == fieldName)
			{
				return true;
			}
			foreach (string? nativeName in history.NativeName.Values)
			{
				if (nativeName is null)
				{
				}
				else if (fieldName == nativeName.ToLowerInvariant()
					|| fieldName == Pass002_RenameSubnodes.GetValidFieldName(nativeName).ToLowerInvariant())
				{
					return true;
				}
			}
			return false;
		}

		private static DataMemberHistory? DetermineHistoryFromImplementations(this InterfaceProperty interfaceProperty)
		{
			DataMemberHistory? history = null;
			foreach (ClassProperty classProperty in interfaceProperty.Implementations)
			{
				if (history is null)
				{
					history = classProperty.History;
				}
				else if (classProperty.History is null)
				{
				}
				else if (history != classProperty.History)
				{
					return null;
				}
			}
			return history;
		}
	}
}
