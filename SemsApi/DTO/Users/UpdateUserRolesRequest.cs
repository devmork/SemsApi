namespace SemsApi.DTO.Users
{
    public class UpdateUserRolesRequest
    {
        /// <summary>The complete set of roles the user should have (Admin, QA, Student). Existing roles are replaced.</summary>
        public List<string> Roles { get; set; } = [];
    }
}
