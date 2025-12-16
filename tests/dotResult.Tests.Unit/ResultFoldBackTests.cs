using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultFoldBackTests
{
    [Fact]
    public void FoldBack_returns_folded_state_for_ok()
    {
        var initialState = 10;
        var result = Ok<string, string>("four");
        Func<string, int, int> folder = (value, state) => value.Length + state;

        var folded = result.FoldBack(folder, initialState);

        Assert.Equal(14, folded);
    }

    [Fact]
    public void FoldBack_skips_folder_for_error()
    {
        var initialState = 7;
        var result = Error<string, string>("boom");
        var called = false;
        int Folder(string value, int state)
        {
            called = true;
            return value.Length + state;
        }

        var folded = result.FoldBack(Folder, initialState);

        Assert.Equal(initialState, folded);
        Assert.False(called);
    }

    [Property]
    public Property FoldBack_on_ok_matches_folder_application() =>
        Prop.ForAll<NonEmptyString, int, Func<string, int, int>>((value, state, folder) =>
            Ok<string, string>(value.Get).FoldBack(folder, state) == folder(value.Get, state));

    [Property]
    public Property FoldBack_on_error_returns_initial_state() =>
        Prop.ForAll<NonEmptyString, int, Func<string, int, int>>((msg, state, folder) =>
            Error<string, string>(msg.Get).FoldBack(folder, state) == state);
}
