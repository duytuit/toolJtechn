namespace JtechnApi.Shares
{
    public enum RepositoryOutcome { Success, Created, Empty, Invalid, Missing, Conflict }

    // Repository results contain data and business outcomes, never MVC action results.
    public sealed record RepositoryResult(RepositoryOutcome Outcome, object Value = null, string Action = null, object RouteValues = null)
    {
        public static RepositoryResult Ok(object value) => new(RepositoryOutcome.Success, value);
        public static RepositoryResult BadRequest(object value) => new(RepositoryOutcome.Invalid, value);
        public static RepositoryResult NotFound(object value = null) => new(RepositoryOutcome.Missing, value);
        public static RepositoryResult Conflict(object value) => new(RepositoryOutcome.Conflict, value);
        public static RepositoryResult NoContent() => new(RepositoryOutcome.Empty);
        public static RepositoryResult CreatedAtAction(string action, object routeValues, object value) => new(RepositoryOutcome.Created, value, action, routeValues);
    }
}
