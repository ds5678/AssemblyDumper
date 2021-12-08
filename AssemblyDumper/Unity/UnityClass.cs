﻿using System;
using System.Collections.Generic;

namespace AssemblyDumper.Unity
{
	public class UnityClass
	{
		/// <summary>
		/// The name of the class not including the namespace
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The namespace of the class if it exists
		/// </summary>
		public string Namespace { get; set; }
		/// <summary>
		/// The full name of the class including the namespace but not an assembly specification
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// The module containing the class
		/// </summary>
		public string Module { get; set; }
		/// <summary>
		/// The unique number used to identify the class
		/// </summary>
		public int TypeID { get; set; }
		/// <summary>
		/// The name of the base class if it exists. Namespace not included
		/// </summary>
		public string Base { get; set; }
		/// <summary>
		/// The names of the classes that directly derive from this. Namespaces not included
		/// </summary>
		public List<string> Derived { get; set; }
		/// <summary>
		/// The count of all classes that descend from this class
		/// </summary>
		public uint DescendantCount { get; set; }
		/// <summary>
		/// The size in bytes of one object. Doesn't include alignments. May be wildly inaccurate, especially for classes with variable size.
		/// </summary>
		public int Size { get; set; }
		/// <summary>
		/// The zero based index of this class in the list of acquired classes. Doesn't mean much.
		/// </summary>
		public uint TypeIndex { get; set; }
		/// <summary>
		/// Is the class abstract?
		/// </summary>
		public bool IsAbstract { get; set; }
		/// <summary>
		/// Is the class sealed?
		/// </summary>
		public bool IsSealed { get; set; }
		/// <summary>
		/// Does the class only appear in the editor?
		/// </summary>
		public bool IsEditorOnly { get; set; }
		/// <summary>
		/// Is the class stripped?
		/// </summary>
		public bool IsStripped { get; set; }
		public UnityNode EditorRootNode { get; set; }
		public UnityNode ReleaseRootNode { get; set; }

		/// <summary>
		/// The constructor used in json deserialization
		/// </summary>
		public UnityClass() { }

		/// <summary>
		/// The constructor used to make dependent class definitions
		/// </summary>
		public UnityClass(UnityNode releaseRootNode, UnityNode editorRootNode)
		{
			if (releaseRootNode == null && editorRootNode == null)
				throw new ArgumentException("Both root nodes cannot be negative");

			ReleaseRootNode = releaseRootNode;
			EditorRootNode = editorRootNode;
			var mainRootNode = releaseRootNode ?? editorRootNode;

			Name = mainRootNode.TypeName;
			FullName = Name;
			TypeID = -1;
			Derived = new List<string>();
			DescendantCount = 0;
			Size = mainRootNode.ByteSize;
			IsAbstract = false;
			IsSealed = true;
			IsEditorOnly = releaseRootNode == null;
			IsStripped = false;
		}
	}
}