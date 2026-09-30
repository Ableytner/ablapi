using AblApi.Core.AppWillhaben;

namespace Tests.Unit.Api;

public class TreeAttributesTests
{
    [Fact]
    public void ParseValues_NullOrEmptyInput_ReturnsEmptyDictionary()
    {
        // Arrange
        var inputs = new string?[] { null, "", "   ", "[]" };

        // Act & Assert
        foreach (var input in inputs)
        {
            var result = TreeAttributes.ParseValues(input);
            Assert.Empty(result);
        }
    }

    [Fact]
    public void ParseValues_BracketedInput_ParsesAllAttributes()
    {
        // Arrange
        var raw = "[21;22,2535;2536,3199;3215]";

        // Act
        var result = TreeAttributes.ParseValues(raw);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(Zustand.Neu.ToString(), result[nameof(Zustand)]);
        Assert.Equal(Übergabe.Selbstabholung.ToString(), result[nameof(Übergabe)]);
        Assert.Equal(Farbe.Weiß.ToString(), result[nameof(Farbe)]);
    }

    [Fact]
    public void ParseValues_UnknownIds_SkipsInvalidEntries()
    {
        // Arrange
        var raw = "9999;1234,21;9999,21;22,3199;3220";

        // Act
        var result = TreeAttributes.ParseValues(raw);

        // Assert
        Assert.Single(result);
        Assert.Equal(Zustand.Neu.ToString(), result[nameof(Zustand)]);
    }

    [Fact]
    public void ParseValues_MalformedEntries_SkipsInvalidEntries()
    {
        // Arrange
        var raw = "21;22,invalid,2535;2536,3199;abc";

        // Act
        var result = TreeAttributes.ParseValues(raw);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(Zustand.Neu.ToString(), result[nameof(Zustand)]);
        Assert.Equal(Übergabe.Selbstabholung.ToString(), result[nameof(Übergabe)]);
    }

    [Fact]
    public void ParseValue_NullOrMissingAttribute_ReturnsNull()
    {
        // Arrange
        string? nullRaw = null;
        var missingRaw = "21;22,3199;3201";

        // Act & Assert
        Assert.Null(TreeAttributes.ParseValue(nullRaw, nameof(Zustand)));
        Assert.Null(TreeAttributes.ParseValue(missingRaw, nameof(Übergabe)));
    }

    [Fact]
    public void ParseValue_MultipleAttributes_ExtractsCorrectValue()
    {
        // Arrange
        var raw = "21;24,2535;2537,3199;3208";

        // Act
        var zustand = TreeAttributes.ParseValue(raw, nameof(Zustand));
        var übergabe = TreeAttributes.ParseValue(raw, nameof(Übergabe));
        var farbe = TreeAttributes.ParseValue(raw, nameof(Farbe));

        // Assert
        Assert.Equal(Zustand.Defekt.ToString(), zustand);
        Assert.Equal(Übergabe.Versand.ToString(), übergabe);
        Assert.Equal(Farbe.Rot.ToString(), farbe);
    }

    [Fact]
    public void ParseValue_AllZustandValues_ReturnsCorrectNames()
    {
        // Arrange
        var cases = new[]
        {
            (22, Zustand.Neu.ToString()),
            (23, Zustand.Gebraucht.ToString()),
            (24, Zustand.Defekt.ToString()),
            (2539, Zustand.Ausstellungsstück.ToString()),
            (2546, Zustand.Neuwertig.ToString()),
            (5013256, Zustand.Generalüberholt.ToString()),
        };

        // Act & Assert
        foreach (var (id, expected) in cases)
        {
            var result = TreeAttributes.ParseValue($"21;{id}", nameof(Zustand));
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void ParseNames_NullOrEmptyInput_ReturnsEmptyDictionary()
    {
        // Arrange
        var inputs = new string?[] { null, "", "   " };

        // Act & Assert
        foreach (var input in inputs)
        {
            var result = TreeAttributes.ParseNames(input);
            Assert.Empty(result);
        }
    }

    [Fact]
    public void ParseNames_SingleAndMultipleAttributes_ParsesCorrectly()
    {
        // Arrange
        var single = "Farbe;Weiß";
        var multiple = "Zustand;Neu,Übergabe;Versand,Farbe;Schwarz";

        // Act
        var singleResult = TreeAttributes.ParseNames(single);
        var multipleResult = TreeAttributes.ParseNames(multiple);

        // Assert
        Assert.Single(singleResult);
        Assert.Equal(3215, singleResult[3199]);

        Assert.Equal(3, multipleResult.Count);
        Assert.Equal(22, multipleResult[21]);
        Assert.Equal(2537, multipleResult[2535]);
        Assert.Equal(3201, multipleResult[3199]);
    }

    [Fact]
    public void ParseNames_UnknownNames_SkipsInvalidEntries()
    {
        // Arrange
        var raw = "NonExistent;Value,Zustand;Neu,Farbe;Unbekannt";

        // Act
        var result = TreeAttributes.ParseNames(raw);

        // Assert
        Assert.Single(result);
        Assert.Equal(22, result[21]);
    }

    [Fact]
    public void ParseNames_DuplicateAttributes_LastValueWins()
    {
        // Arrange
        var raw = "Zustand;Neu,Zustand;Gebraucht,Zustand;Generalüberholt";

        // Act
        var result = TreeAttributes.ParseNames(raw);

        // Assert
        Assert.Single(result);
        Assert.Equal(5013256, result[21]);
    }

    [Fact]
    public void ParseNames_WithSpaces_ParsesCorrectly()
    {
        // Arrange
        var raw = " Zustand ; Neu , Übergabe ; Selbstabholung , Farbe ; Blau ";

        // Act
        var result = TreeAttributes.ParseNames(raw);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(22, result[21]);
        Assert.Equal(2536, result[2535]);
        Assert.Equal(3203, result[3199]);
    }

    [Fact]
    public void ParseValues_ConvertsIdRepresentationToNames()
    {
        // Arrange
        var raw = "21;22,2535;2536,3199;3215";

        // Act
        var nameMap = TreeAttributes.ParseValues(raw);

        // Assert
        Assert.Equal(3, nameMap.Count);
        Assert.Equal(Zustand.Neu.ToString(), nameMap[nameof(Zustand)]);
        Assert.Equal(Übergabe.Selbstabholung.ToString(), nameMap[nameof(Übergabe)]);
        Assert.Equal(Farbe.Weiß.ToString(), nameMap[nameof(Farbe)]);
    }

    [Fact]
    public void ParseNames_ConvertsNameRepresentationToIds()
    {
        // Arrange
        var raw = "Zustand;Neu,Übergabe;Versand,Farbe;Rot";

        // Act
        var idMap = TreeAttributes.ParseNames(raw);

        // Assert
        Assert.Equal(3, idMap.Count);
        Assert.Equal(22, idMap[21]);
        Assert.Equal(2537, idMap[2535]);
        Assert.Equal(3208, idMap[3199]);
    }
}
