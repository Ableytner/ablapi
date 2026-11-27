namespace AblApi.Core.AppSpotify;

public static class PlaylistDiffCalculator
{
    public static (List<string> ToRemove, List<string> ToAdd) Calculate(
        IReadOnlyList<string> savedTrackIds,
        IReadOnlyList<string> updatedTrackIds)
    {
        var toRemove = new List<string>();

        var offset = 0;
        var c = 0;

        while (c < savedTrackIds.Count)
        {
            if (c + offset > updatedTrackIds.Count - 1)
            {
                // we have exhausted all updated tracks
                toRemove.AddRange(savedTrackIds.Skip(c));
                break;
            }

            if (savedTrackIds[c] != updatedTrackIds[c + offset])
            {
                // remove the mismatching track
                toRemove.Add(savedTrackIds[c]);
                offset -= 1;
            }

            c++;
        }

        // add remaining new tracks
        var toAdd = updatedTrackIds.Skip(c + offset).ToList();

        return (toRemove, toAdd);
    }
}
