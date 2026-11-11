using Microsoft.EntityFrameworkCore;

namespace PatientManagement.Data.Repositories;

// Recognises a unique-index refusal from SQL Server: error 2601 (unique index) and 2627 (unique constraint).
internal static class UniqueViolation
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
}
