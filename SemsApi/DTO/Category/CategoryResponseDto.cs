namespace SemsApi.DTO
{
    /// <summary>
    /// Response shape for category CRUD operations.
    /// Includes Recno so clients can identify the record for update/delete.
    /// </summary>
    public class CategoryResponseDto
    {
        public int Recno { get; set; }
        public string? EvalType { get; set; }
        public int? CatNo { get; set; }
        public string? CatRn { get; set; }
        public string? CatName { get; set; }
        public decimal? CatRate { get; set; }
    }
}