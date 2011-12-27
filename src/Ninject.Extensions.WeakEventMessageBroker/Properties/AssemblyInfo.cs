using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly : AssemblyTitle( "Ninject.Extensions.WeakEventBroker" )]
[assembly : AssemblyDescriptionAttribute("Message broker extension for Ninject using weak events")]
[assembly : AssemblyConfiguration( "" )]
[assembly : AssemblyTrademark( "" )]
[assembly : AssemblyCulture( "" )]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly : Guid( "ff77a1fe-e81d-46f5-b2a7-4a846a4188ad" )]

#if !NO_PARTIAL_TRUST
[assembly: AllowPartiallyTrustedCallers]
#endif