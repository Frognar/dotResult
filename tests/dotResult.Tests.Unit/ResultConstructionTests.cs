namespace dotResult.Tests.Unit;

using FsCheck;
using FsCheck.Xunit;
using Result = DotResult.Result;

public class ResultConstructionTests
{
    [Property]
    public void Ok_roundtrips(int value)
    {
        var ok = Result.Ok<int, string>(value);
        Assert.Equal(Result.Ok<int, string>(value), ok);
    }
}
