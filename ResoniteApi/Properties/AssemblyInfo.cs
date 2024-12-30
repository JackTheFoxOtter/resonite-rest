using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

using Elements.Core;

[assembly: AssemblyTitle("ResoniteAPI")]
[assembly: AssemblyProduct("ResoniteAPI")]
[assembly: AssemblyDescription("A simple REST-API plugin for Resonite.")]
[assembly: AssemblyCopyright("Made by JackTheFoxOtter")]
[assembly: AssemblyVersion("0.0.1")]
[assembly: AssemblyFileVersion("0.0.1")]

[assembly: ComVisible(false)]

//Mark as DataModelAssembly for the Plugin loading system to load this assembly
[assembly: DataModelAssembly(DataModelAssemblyType.UserspaceCore)]