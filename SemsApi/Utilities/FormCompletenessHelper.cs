using Microsoft.EntityFrameworkCore;
using SemsApi.Data;

namespace SemsApi.Utilities
{
    public static class FormCompletenessHelper
    {
        /// <summary>
        /// A EvaluationForm is complete when every active selected category on that form
        /// has at least one form-scoped question.
        /// Returns an error message, or null when complete.
        /// </summary>
        public static async Task<string?> GetIncompleteFormErrorAsync(
            ApplicationDbContext context,
            int formId,
            CancellationToken ct = default)
        {
            var activeCategoryIds = await context.EvaluationFormCategories
                .AsNoTracking()
                .Where(fc => fc.FormId == formId && fc.IsActive)
                .Select(fc => fc.CategoryId)
                .ToListAsync(ct);

            if (activeCategoryIds.Count == 0)
            {
                return "This form has no categories selected. Add categories and questions first.";
            }

            var categoriesWithQuestions = await context.EvaluationFormQuestions
                .AsNoTracking()
                .Join(
                    context.EvaluationFormCategories.AsNoTracking(),
                    q => q.FormCategoryId,
                    fc => fc.FormCategoryId,
                    (q, fc) => new { q.FormCategoryId, fc.CategoryId, fc.FormId, fc.IsActive })
                .Where(x => x.FormId == formId && x.IsActive)
                .Select(x => x.CategoryId)
                .Distinct()
                .ToListAsync(ct);

            var missing = activeCategoryIds
                .Except(categoriesWithQuestions)
                .ToList();

            if (missing.Count > 0)
            {
                return $"Every selected category needs at least one question. Categories without questions (CategoryId): {string.Join(", ", missing)}.";
            }

            return null;
        }
    }
}
