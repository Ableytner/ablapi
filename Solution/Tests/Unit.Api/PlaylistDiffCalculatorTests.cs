using AblApi.Core.AppSpotify;

namespace Tests.Unit.Api;

public class PlaylistDiffCalculatorTests
{
    [Fact]
    public void Calculate_BothEmpty_ReturnsEmpty()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate([], []);

        Assert.Empty(toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_IdenticalLists_ReturnsEmpty()
    {
        string[] input = ["a", "b", "c", "d", "e"];

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(input, input);

        Assert.Empty(toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_SavedEmpty_AllAdded()
    {
        string[] updated = ["a", "b", "c"];

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate([], updated);

        Assert.Empty(toRemove);
        Assert.Equal(updated, toAdd);
    }

    [Fact]
    public void Calculate_UpdatedEmpty_AllRemoved()
    {
        string[] saved = ["a", "b", "c"];

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(saved, []);

        Assert.Equal(saved, toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_SingleTrackUnchanged_ReturnsEmpty()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(["a"], ["a"]);

        Assert.Empty(toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_SingleTrackChanged_RemovesAndAdds()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(["a"], ["b"]);

        Assert.Equal(["a"], toRemove);
        Assert.Equal(["b"], toAdd);
    }

    [Fact]
    public void Calculate_AddSingleAtEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c"],
            ["a", "b", "c", "d"]);

        Assert.Empty(toRemove);
        Assert.Equal(["d"], toAdd);
    }

    [Fact]
    public void Calculate_AddMultipleAtEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c"],
            ["a", "b", "c", "d", "e"]);

        Assert.Empty(toRemove);
        Assert.Equal(["d", "e"], toAdd);
    }

    [Fact]
    public void Calculate_RemoveSingleFromEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d"],
            ["a", "b", "c"]);

        Assert.Equal(["d"], toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_RemoveMultipleFromEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d", "e"],
            ["a", "b", "c"]);

        Assert.Equal(["d", "e"], toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_RemoveSingleFromMiddle()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d", "e"],
            ["a", "b", "d", "e"]);

        Assert.Equal(["c"], toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_RemoveSingleFromBeginning()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d"],
            ["b", "c", "d"]);

        Assert.Equal(["a"], toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_RemoveMultipleFromMiddle()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d", "e", "f"],
            ["a", "b", "e", "f"]);

        Assert.Equal(["c", "d"], toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_RemoveFromMiddleAndAppendAtEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d"],
            ["a", "c", "d", "e"]);

        Assert.Equal(["b"], toRemove);
        Assert.Equal(["e"], toAdd);
    }

    [Fact]
    public void Calculate_RemoveFromBeginningAndAppendAtEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c"],
            ["b", "c", "d"]);

        Assert.Equal(["a"], toRemove);
        Assert.Equal(["d"], toAdd);
    }

    [Fact]
    public void Calculate_MultipleRemovesAndAppendAtEnd()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d", "e"],
            ["a", "d", "e", "x", "y"]);

        Assert.Equal(["b", "c"], toRemove);
        Assert.Equal(["x", "y"], toAdd);
    }

    [Fact]
    public void Calculate_CompleteReplacement()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c"],
            ["x", "y", "z"]);

        Assert.Equal(["a", "b", "c"], toRemove);
        Assert.Equal(["x", "y", "z"], toAdd);
    }

    [Fact]
    public void Calculate_LargeList_SingleRemovalInMiddle()
    {
        var saved = Enumerable.Range(0, 1000).Select(i => $"t{i}").ToArray();
        var updated = saved.Where(id => id != "t500").ToArray();

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(saved, updated);

        Assert.Equal(["t500"], toRemove);
        Assert.Empty(toAdd);
    }

    [Fact]
    public void Calculate_LargeList_AppendAtEnd()
    {
        var saved = Enumerable.Range(0, 1000).Select(i => $"t{i}").ToArray();
        var updated = saved.Concat(Enumerable.Range(1000, 50).Select(i => $"t{i}")).ToArray();

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(saved, updated);

        Assert.Empty(toRemove);
        Assert.Equal(50, toAdd.Count);
        Assert.Equal("t1000", toAdd[0]);
        Assert.Equal("t1049", toAdd[^1]);
    }

    [Fact]
    public void Calculate_LargeList_RemoveAndAppend()
    {
        var saved = Enumerable.Range(0, 1000).Select(i => $"t{i}").ToArray();
        var updated = saved
            .Where(id => id != "t250" && id != "t750")
            .Concat(["t1000", "t1001"])
            .ToArray();

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(saved, updated);

        Assert.Equal(["t250", "t750"], toRemove);
        Assert.Equal(["t1000", "t1001"], toAdd);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(42)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(12345)]
    public void Calculate_MatchesSetDifference_ForAppendOnlyScenarios(int seed)
    {
        var rng = new Random(seed);

        // A pool of "old" tracks, each with a stable timestamp, sorted oldest->newest
        var oldCount = rng.Next(0, 80);
        var oldTracks = Enumerable.Range(0, oldCount)
            .Select(i => $"old{i}")
            .OrderBy(_ => rng.Next()) // assign random timestamps, then sort
            .ToArray();

        // saved = all old tracks (the previous cycle's all_playlist content)
        var saved = oldTracks;

        // updated = a subset of old tracks (some removed) + new tracks appended at the end
        var keep = oldTracks.Where(_ => rng.NextDouble() < 0.7).ToList();
        var newCount = rng.Next(0, 30);
        var newTracks = Enumerable.Range(0, newCount).Select(i => $"new{i}").ToArray();

        var updated = keep.Concat(newTracks).ToArray();

        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(saved, updated);

        var savedSet = new HashSet<string>(saved);
        var updatedSet = new HashSet<string>(updated);

        var expectedRemove = saved.Where(id => !updatedSet.Contains(id)).ToArray();
        var expectedAdd = updated.Where(id => !savedSet.Contains(id)).ToArray();

        Assert.Equal(expectedRemove, toRemove);
        Assert.Equal(expectedAdd, toAdd);
    }

    [Fact]
    public void Calculate_PropertyBased_EmptySavedAcrossSeeds()
    {
        for (var seed = 0; seed < 20; seed++)
        {
            var rng = new Random(seed);
            var count = rng.Next(0, 40);
            var updated = Enumerable.Range(0, count).Select(i => $"t{i}").ToArray();

            var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate([], updated);

            Assert.Empty(toRemove);
            Assert.Equal(updated, toAdd);
        }
    }

    [Fact]
    public void Calculate_InsertInMiddle_DoesFullSuffixResync()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c", "d"],
            ["a", "x", "b", "c", "d"]);

        var result = ApplyDiff(["a", "b", "c", "d"], toRemove, toAdd);
        Assert.Equal(["a", "x", "b", "c", "d"], result);
    }

    [Fact]
    public void Calculate_InsertAtFront_DoesFullResync()
    {
        var (toRemove, toAdd) = PlaylistDiffCalculator.Calculate(
            ["a", "b", "c"],
            ["x", "a", "b", "c"]);

        var result = ApplyDiff(["a", "b", "c"], toRemove, toAdd);
        Assert.Equal(["x", "a", "b", "c"], result);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static string[] ApplyDiff(string[] current, List<string> toRemove, List<string> toAdd)
    {
        var removeSet = new HashSet<string>(toRemove);
        var result = current.Where(id => !removeSet.Contains(id)).ToList();
        result.AddRange(toAdd);
        return result.ToArray();
    }
}
