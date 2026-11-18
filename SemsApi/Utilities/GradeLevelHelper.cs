using System.Text.RegularExpressions;

namespace SemsApi.Utilities;

/// <summary>
/// Converts the integer GradeLevel used by StudentTermEnrollment / SubjectOffering
/// into the human-readable label used across the SEMS UI.
/// Convention: 0=Nursery, 1=Kinder 1, 2=Kinder 2, 3=Grade 1, ..., 14=Grade 12.
/// </summary>
public static class GradeLevelHelper
{
    public const int MinGradeLevel = 0;
    public const int MaxGradeLevel = 14;

    private static readonly Regex GradePattern =
        new(@"grade\s*(\d{1,2})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex KinderPattern =
        new(@"kinder\w*\s*(\d)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string ToDisplay(int gradeLevel) => gradeLevel switch
    {
        0 => "Nursery",
        1 => "Kinder 1",
        2 => "Kinder 2",
        >= 3 and <= 14 => $"Grade {gradeLevel - 2}",
        _ => $"Level {gradeLevel}"
    };

    /// <summary>
    /// Best-effort conversion of the legacy free-text <c>Student.GradeLevel</c>
    /// ("Grade 7", "Kinder 1", "Nursery", or a raw internal value) into the
    /// numeric convention above. Returns null when no grade can be inferred.
    /// </summary>
    public static int? ParseDisplay(string? gradeLevel)
    {
        if (string.IsNullOrWhiteSpace(gradeLevel))
            return null;

        var text = gradeLevel.Trim();

        if (text.Contains("nursery", StringComparison.OrdinalIgnoreCase))
            return 0;

        var gradeMatch = GradePattern.Match(text);
        if (gradeMatch.Success && int.TryParse(gradeMatch.Groups[1].Value, out var grade))
            return Clamp(grade + 2);

        var kinderMatch = KinderPattern.Match(text);
        if (kinderMatch.Success)
        {
            var number = kinderMatch.Groups[1].Value;
            return number == "1" ? 1 : 2;
        }

        return int.TryParse(text, out var numeric) ? Clamp(numeric) : null;
    }

    private static int? Clamp(int value) =>
        value >= MinGradeLevel && value <= MaxGradeLevel ? value : null;
}
