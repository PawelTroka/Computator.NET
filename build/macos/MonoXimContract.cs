using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

// Shared by the installed preflight and native diagnostic. No WinForms reference
// is required: inspect the actual assembly selected by this Mono runtime before
// enabling any fixed-arity dispatcher or initializing its GUI driver.
internal static class MonoXimContract
{
    public static void Validate(Assembly assembly)
    {
        Type keyboard = assembly.GetType("System.Windows.Forms.X11Keyboard", true);
        Type point = assembly.GetType("System.Windows.Forms.XPoint", true);
        Type mask = assembly.GetType("System.Windows.Forms.EventMask", true);
        Type style = assembly.GetType("System.Windows.Forms.XIMProperties", true);
        Require(IntPtr.Size == 8 && point.IsClass && point.IsLayoutSequential && Marshal.SizeOf(point) == 4,
            "XPoint must be the reviewed four-byte sequential class passed by pointer.");
        var fields = point.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Require(fields.Length == 2 && fields.All(f => f.FieldType == typeof(short)) &&
            Marshal.OffsetOf(point, "X").ToInt32() == 0 && Marshal.OffsetOf(point, "Y").ToInt32() == 2,
            "XPoint field layout changed.");
        Require(mask.IsEnum && style.IsEnum && Enum.GetUnderlyingType(mask) == typeof(int) && Enum.GetUnderlyingType(style) == typeof(int),
            "The native EventMask or XIMProperties enum width changed.");
        Check(keyboard, "XGetIMValues", typeof(string), false, new[] {
            new[] { typeof(IntPtr), typeof(string), typeof(IntPtr).MakeByRefType(), typeof(IntPtr) } });
        Check(keyboard, "XGetICValues", typeof(string), false, new[] {
            new[] { typeof(IntPtr), typeof(string), mask.MakeByRefType(), typeof(IntPtr) } });
        Check(keyboard, "XSetICValues", typeof(void), true, new[] {
            new[] { typeof(IntPtr), typeof(string), typeof(IntPtr), typeof(IntPtr) } });
        Check(keyboard, "XCreateIC", typeof(IntPtr), true, new[] {
            new[] { typeof(IntPtr), typeof(string), style, typeof(string), typeof(IntPtr), typeof(IntPtr) },
            new[] { typeof(IntPtr), typeof(string), style, typeof(string), typeof(IntPtr), typeof(string), typeof(IntPtr), typeof(IntPtr) } });
        Check(keyboard, "XVaCreateNestedList", typeof(IntPtr), true, new[] {
            new[] { typeof(int), typeof(IntPtr), point, typeof(IntPtr) },
            new[] { typeof(int), typeof(IntPtr), point, typeof(IntPtr), typeof(IntPtr), typeof(IntPtr) },
            new[] { typeof(int), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr) } });
    }

    private static void Check(Type keyboard, string name, Type result, bool explicitCdecl, Type[][] signatures)
    {
        var methods = keyboard.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == name).ToArray();
        Require(methods.Length == signatures.Length, name + " overload count changed.");
        foreach (var signature in signatures)
        {
            var candidates = methods.Where(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(signature)).ToArray();
            Require(candidates.Length == 1, name + " fixed parameter order or types changed.");
            var method = candidates[0];
            var import = (DllImportAttribute)Attribute.GetCustomAttribute(method, typeof(DllImportAttribute));
            Require(method.ReturnType == result && import != null && import.Value == "libX11" && import.EntryPoint == name && import.PreserveSig,
                name + " native module, entry point, return type or PreserveSig changed.");
            Require(import.CallingConvention == CallingConvention.Cdecl || (!explicitCdecl && import.CallingConvention == CallingConvention.Winapi),
                name + " calling convention changed.");
            Require(import.CharSet == CharSet.None || import.CharSet == CharSet.Ansi, name + " string encoding changed.");
            var returnMarshal = (MarshalAsAttribute)Attribute.GetCustomAttribute(method.ReturnParameter, typeof(MarshalAsAttribute));
            Require(returnMarshal == null || (result == typeof(string) && returnMarshal.Value == UnmanagedType.LPStr),
                name + " explicit return marshalling changed.");
            foreach (var parameter in method.GetParameters())
            {
                if (parameter.ParameterType.IsByRef) Require(parameter.IsOut, name + " output direction changed.");
                var marshal = (MarshalAsAttribute)Attribute.GetCustomAttribute(parameter, typeof(MarshalAsAttribute));
                Require(marshal == null || (parameter.ParameterType == typeof(string) && marshal.Value == UnmanagedType.LPStr),
                    name + " explicit argument marshalling changed.");
            }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new PlatformNotSupportedException("Installed Mono WinForms XIM ABI is not compatible with this package: " + message);
    }
}
