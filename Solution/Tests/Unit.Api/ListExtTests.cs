using AblApi.Common.Extensions;

namespace Tests.Unit.Api;

public class ListExtTests
{
    [Fact]
    public void SortByVersionDescending_ShouldSortListCorrectly()
    {
        // Arrange
        var inputList = GetInitialVersionsList();
        var expectedList = GetSortedVersionsList();

        // Act
        var sortedList = inputList.SortByVersionDescending();

        // Assert
        Assert.Equal(expectedList, sortedList);
    }

    [Fact]
    public void SortByVersionDescending_Generic_ShouldSortListCorrectly()
    {
        // Arrange
        var inputList = GetInitialVersionsList().Select(v => new VersionTestDto { Version = v }).ToList();
        var expectedList = GetSortedVersionsList().Select(v => new VersionTestDto { Version = v }).ToList();

        // Act
        var sortedList = inputList.SortByVersionDescending(x => x.Version);

        // Assert
        Assert.Equal(expectedList.Select(v => v.Version), sortedList.Select(v => v.Version));
    }

    private List<string> GetInitialVersionsList()
    {
        return new List<string>
        {
            "1.0.0",
            "1.0.0-beta-1",
            "2.0.0-beta",
            "1.0.0-rc",
            "1.0.0-alpha",
            "1.1.0",
            "2.0.0",
            "2.0.0-rc",
            "1.0.2",
            "1.0.1",
            "1.0.0-beta-2",
            "1.0.11",
            "2.0.0-alpha"
        };
    }

    private List<string> GetSortedVersionsList()
    {
        return new List<string>
        {
            "2.0.0",
            "2.0.0-rc",
            "2.0.0-beta",
            "2.0.0-alpha",
            "1.1.0",
            "1.0.11",
            "1.0.2",
            "1.0.1",
            "1.0.0",
            "1.0.0-rc",
            "1.0.0-beta-2",
            "1.0.0-beta-1",
            "1.0.0-alpha"
        };
    }
}

internal class VersionTestDto
{
    public string Version { get; set; }
}
