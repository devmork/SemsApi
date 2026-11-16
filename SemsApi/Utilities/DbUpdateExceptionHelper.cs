using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace SemsApi.Utilities
{
    public static class DbUpdateExceptionHelper
    {
        public static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
                return sqlEx.Number is 2601 or 2627;

            return false;
        }
    }
}
