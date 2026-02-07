using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Web
{
    public static class ApiResponse
    {
        public static object Ok(object data, string? traceId)
            => new { data, traceId };
    }
}
