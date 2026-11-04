using Microsoft.EntityFrameworkCore;
using SemsApi.Data;

namespace SemsApi.Utilities
{
    public static class FormCompletenessHelper
    {
        /// <summary>
        /// A form is complete when its EvaluationType has at least one category,
        /// and every category has at least one question.
        /// Returns an error message, or null when complete.
        /// </summary>
        public static async Task<string?> GetIncompleteFormErrorAsync(
            ApplicationDbContext context,
            int evaluationTypeId,
            CancellationToken ct = default)
        {
            var categoryNos = await context.TeacherEvaluationCategories
                .AsNoTracking()
                .Where(c => c.EvaluationTypeId == evaluationTypeId)
                .Select(c => c.CatNo)
                .ToListAsync(ct);

            if (categoryNos.Count == 0)
            {
                return "This evaluation type has no categories yet. Create its categories and questions first.";
            }

            var categoriesWithQuestions = await context.TeacherEvaluationResults
                .AsNoTracking()
                .Where(q => categoryNos.Contains(q.CatNo))
                .Select(q => q.CatNo)
                .Distinct()
                .ToListAsync(ct);

            var missing = categoryNos
                .Except(categoriesWithQuestions)
                .ToList();

            if (missing.Count > 0)
            {
                return $"Every category needs at least one question. Categories without questions (CatNo): {string.Join(", ", missing)}.";
            }

            return null;
        }
    }
}