using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotResult.Tests.Unit;

public class ResultFoldTests
{
    [Fact]
    public void Fold_returns_folded_state_for_ok()
    {
        var initialState = 10;
        var result = Ok(5);
        Func<int, int, int> folder = (state, value) => state + value;

        var folded = result.Fold(folder, initialState);

        Assert.Equal(15, folded);
    }

    [Fact]
    public void Fold_skips_folder_for_error()
    {
        var initialState = 7;
        var result = Error("boom");
        var called = false;
        int Folder(int state, int value)
        {
            called = true;
            return state + value;
        }

        var folded = result.Fold(Folder, initialState);

        Assert.Equal(initialState, folded);
        Assert.False(called);
    }

    [Property]
    public Property Fold_on_ok_matches_folder_application() =>
        Prop.ForAll<int, int, Func<int, int, int>>((state, value, folder) =>
            Ok(value).Fold(folder, state) == folder(state, value));

    [Property]
    public Property Fold_on_error_returns_initial_state() =>
        Prop.ForAll<NonEmptyString, int, Func<int, int, int>>((msg, state, folder) =>
            Error<int, string>(msg.Get).Fold(folder, state) == state);
}
