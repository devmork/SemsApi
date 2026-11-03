namespace SemsApi.Utilities;

/// <summary>
/// Converts the integer GradeLevel used by StudentTermEnrollment / SubjectOffering
/// into the human-readable label used across the SEMS UI.
/// Convention: 0=Nursery, 1=Kinder 1, 2=Kinder 2, 3=Grade 1, ..., 14=Grade 12.
/// </summary>
public static class GradeLevelHelper
{
    public static string ToDisplay(int gradeLevel) => gradeLevel switch
    {
        0 => "Nursery",
        1 => "Kinder 1",
        2 => "Kinder 2",
        >= 3 and <= 14 => $"Grade {gradeLevel - 2}",
        _ => $"Level {gradeLevel}"
    };
}