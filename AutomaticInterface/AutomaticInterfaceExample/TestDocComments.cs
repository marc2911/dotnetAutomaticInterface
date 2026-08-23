using System;
using DotnetAutomaticInterface;

namespace AutomaticInterfaceExample;

public class Class1;

public class Class2;

/// <summary>
///     Debug utility to verify in IDE that doc comments are correctly rendered for the interface!
/// </summary>
/// <param name="clsParam">param</param>
[GenerateAutomaticInterface]
public class TestDocComments((Class1, Class2) clsParam) : ITestDocComments
{
    private readonly (Class1, Class2) _clsParam = clsParam;

    /// <summary>
    ///     Some Prop comment ...
    /// </summary>
    public bool SomeProp { get; set; }

    /// <summary>
    ///     Some Comment..
    /// </summary>
    /// <param name="param">This is the param</param>
    public void Method((Class1, Class2) param) { }

    /// <summary>
    ///     Some Comment 2..
    /// </summary>
    /// <param name="timespan">param</param>
    public void Method2((DateTime? from, DateTime? to) timespan) { }

    /// <summary>
    ///     Some in comment...
    /// </summary>
    /// <param name="b"></param>
    public void MethodIn(in bool b) { }

    /// <summary>
    ///     Some params comment...
    /// </summary>
    /// <param name="a"></param>
    public void MethodParams(params int[] a) { }

    /// <summary>
    ///     Some Comment 3...
    ///     <see
    ///         cref="SomeProp" />
    ///     <see
    ///         cref="Method(ValueTuple{Class1, Class2})" />
    ///     <see
    ///         cref="Method2(ValueTuple{DateTime?, DateTime?})" />
    /// </summary>
    /// <param name="param">param</param>
    /// <returns>Result</returns>
    public bool Method3(Class1 param)
    {
        return true;
    }
}

class SomeOtherClass
{
    public static void Method(ITestDocComments param)
    {
        param.Method((new Class1(), new Class2()));
        param.Method2((new DateTime(), new DateTime()));
        param.Method3(new Class1());
        var boolean = true;
        param.MethodIn(in boolean);
        param.MethodParams(1, 2);
        param.SomeProp = true;
    }
}
