using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace BuildingBlocks.Web
{
    public static class ApiProblems
    {
        public static ObjectResult Unauthorized(string detail = "Unauthorized")
            => Create(StatusCodes.Status401Unauthorized, "Unauthorized", detail);

        public static ObjectResult Conflict(string detail = "Conflict")
            => Create(StatusCodes.Status409Conflict, "Conflict", detail);

        public static ObjectResult Locked(string detail, DateTimeOffset? lockedUntil = null)
        {
            var problem = new ProblemDetails
            {
                Status = 423,
                Title = "Locked",
                Detail = detail,
                Type = "https://httpstatuses.com/423"
            };

            if (lockedUntil.HasValue)
                problem.Extensions["lockedUntil"] = lockedUntil.Value;

            return new ObjectResult(problem) { StatusCode = 423 };
        }

        public static ObjectResult TooManyRequests(string detail = "Too Many Requests", int? retryAfterSeconds = null)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = detail,
                Type = "https://httpstatuses.com/429"
            };

            if (retryAfterSeconds.HasValue)
                problem.Extensions["retryAfterSeconds"] = retryAfterSeconds.Value;

            return new ObjectResult(problem) { StatusCode = StatusCodes.Status429TooManyRequests };
        }

        public static ObjectResult Validation(string detail = "Validation error", IDictionary<string, string[]>? errors = null)
        {
            var v = new ValidationProblemDetails(errors ?? new Dictionary<string, string[]>())
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Error",
                Detail = detail,
                Type = "https://httpstatuses.com/422"
            };
            return new ObjectResult(v) { StatusCode = StatusCodes.Status422UnprocessableEntity };
        }

        public static ObjectResult Create(int statusCode, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{statusCode}"
            };
            return new ObjectResult(problem) { StatusCode = statusCode };
        }
    }  
}