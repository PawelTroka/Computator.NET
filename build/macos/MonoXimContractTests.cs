using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.IO;

// Metadata-only rejection tests: no fake native execution or GUI success claim.
internal static class MonoXimContractTests
{
    private static string fixtureDirectory;
    private static int Main(string[] args)
    {
        try
        {
            fixtureDirectory = Path.Combine(Path.GetTempPath(), "computator-xim-metadata-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(fixtureDirectory);
            MonoXimContract.Validate(Fixture("valid"));
            int rejected = 0;
            foreach (string change in new[] { "module", "entry", "return", "parameter", "direction", "charset", "convention", "overload", "point-class", "point-size", "enum-width", "preserve-signature" })
            {
                try { MonoXimContract.Validate(Fixture(change)); }
                catch (PlatformNotSupportedException) { rejected++; continue; }
                throw new InvalidOperationException("Changed native ABI was accepted: " + change);
            }
            if (args.Length == 0)
                MonoXimContract.Validate(Assembly.Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
            else if (args.Length != 1 || args[0] != "--synthetic-only")
                throw new ArgumentException("Use no arguments on Mono, or --synthetic-only for metadata fixtures only.");
            Console.WriteLine("XIM metadata contracts: valid fixture accepted, " + rejected + " ABI changes rejected" +
                (args.Length == 0 ? ", actual installed WinForms metadata accepted." : "; no native/GUI execution claimed."));
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { if (fixtureDirectory != null && Directory.Exists(fixtureDirectory)) Directory.Delete(fixtureDirectory, true); }
    }

    private static Assembly Fixture(string change)
    {
        string name = "MonoXimFixture" + Guid.NewGuid().ToString("N");
        var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.RunAndSave, fixtureDirectory);
        var module = assembly.DefineDynamicModule("fixture", name + ".dll");
        var pointBuilder = module.DefineType("System.Windows.Forms.XPoint", TypeAttributes.Public | TypeAttributes.SequentialLayout |
            (change == "point-class" ? TypeAttributes.Sealed : TypeAttributes.Class), change == "point-class" ? typeof(ValueType) : typeof(object));
        pointBuilder.DefineField("X", typeof(short), FieldAttributes.Public);
        pointBuilder.DefineField("Y", typeof(short), FieldAttributes.Public);
        if (change == "point-size") pointBuilder.DefineField("Extra", typeof(int), FieldAttributes.Public);
        Type point = pointBuilder.CreateType();
        Type mask = module.DefineEnum("System.Windows.Forms.EventMask", TypeAttributes.Public, typeof(int)).CreateType();
        Type style = module.DefineEnum("System.Windows.Forms.XIMProperties", TypeAttributes.Public, change == "enum-width" ? typeof(long) : typeof(int)).CreateType();
        var keyboard = module.DefineType("System.Windows.Forms.X11Keyboard", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);
        Add(keyboard, change, "XGetIMValues", typeof(string), false, typeof(IntPtr), typeof(string), typeof(IntPtr).MakeByRefType(), typeof(IntPtr));
        Add(keyboard, change, "XGetICValues", typeof(string), false, typeof(IntPtr), typeof(string), mask.MakeByRefType(), typeof(IntPtr));
        Add(keyboard, change, "XSetICValues", typeof(void), true, typeof(IntPtr), typeof(string), typeof(IntPtr), typeof(IntPtr));
        if (change != "overload") Add(keyboard, change, "XCreateIC", typeof(IntPtr), true, typeof(IntPtr), typeof(string), style, typeof(string), typeof(IntPtr), typeof(IntPtr));
        Add(keyboard, change, "XCreateIC", typeof(IntPtr), true, typeof(IntPtr), typeof(string), style, typeof(string), typeof(IntPtr), typeof(string), typeof(IntPtr), typeof(IntPtr));
        Add(keyboard, change, "XVaCreateNestedList", typeof(IntPtr), true, typeof(int), typeof(IntPtr), point, typeof(IntPtr));
        Add(keyboard, change, "XVaCreateNestedList", typeof(IntPtr), true, typeof(int), typeof(IntPtr), point, typeof(IntPtr), typeof(IntPtr), typeof(IntPtr));
        Add(keyboard, change, "XVaCreateNestedList", typeof(IntPtr), true, typeof(int), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr), typeof(IntPtr));
        keyboard.CreateType();
        // Mono's in-memory Reflection.Emit view omits some parameter flags.
        // Installed assemblies contain serialized CLI metadata, which is the
        // contract the release launcher must validate.
        assembly.Save(name + ".dll");
        var saved = Assembly.LoadFile(Path.Combine(fixtureDirectory, name + ".dll"));
        if (saved.IsDynamic) throw new InvalidOperationException("The metadata fixture was not reloaded from its serialized assembly.");
        return saved;
    }

    private static void Add(TypeBuilder type, string change, string name, Type result, bool cdecl, params Type[] parameters)
    {
        bool first = name == "XGetIMValues";
        if (first && change == "parameter") parameters[0] = typeof(long);
        var method = type.DefinePInvokeMethod(name, first && change == "module" ? "other-X11" : "libX11",
            first && change == "entry" ? "ChangedEntry" : name,
            MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.PinvokeImpl, CallingConventions.Standard,
            first && change == "return" ? typeof(long) : result, parameters,
            first && change == "convention" ? CallingConvention.StdCall : cdecl ? CallingConvention.Cdecl : CallingConvention.Winapi,
            first && change == "charset" ? CharSet.Unicode : CharSet.Ansi);
        if (!(first && change == "preserve-signature")) method.SetImplementationFlags(method.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
        for (int index = 0; index < parameters.Length; index++)
            if (parameters[index].IsByRef && !(first && change == "direction")) method.DefineParameter(index + 1, ParameterAttributes.Out, "output");
    }
}
