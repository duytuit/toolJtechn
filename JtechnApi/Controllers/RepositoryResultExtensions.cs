using JtechnApi.Shares;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Controllers
{
    public static class RepositoryResultExtensions
    {
        public static IActionResult ToActionResult(this RepositoryResult result, ControllerBase controller) => result.Outcome switch
        {
            RepositoryOutcome.Success => controller.Ok(result.Value),
            RepositoryOutcome.Created => controller.CreatedAtAction(result.Action, result.RouteValues, result.Value),
            RepositoryOutcome.Empty => controller.NoContent(),
            RepositoryOutcome.Invalid => controller.BadRequest(result.Value),
            RepositoryOutcome.Missing => result.Value == null ? controller.NotFound() : controller.NotFound(result.Value),
            RepositoryOutcome.Conflict => controller.Conflict(result.Value),
            _ => throw new InvalidOperationException("Unknown repository outcome.")
        };
    }
}
