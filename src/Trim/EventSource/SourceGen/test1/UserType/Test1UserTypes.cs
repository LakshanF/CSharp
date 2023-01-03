using System.Diagnostics.Tracing;
namespace ES_Test1;
public class ClassTest
{
    public string S1 {get;set;}="ClassTest1";
    public int I1 {get;set;}=10;
}

[EventData]
public struct StructTest
{
    public StructTest(){}
    public string S2 {get;set;}="StructTest1";
    public int I2 {get;set;}=20;
}

public class ClassTest2
{
    public string S1 {get;set;}="ClassTest1";
    public int I1 {get;set;}=10;
}
