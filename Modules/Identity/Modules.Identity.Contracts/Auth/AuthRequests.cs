using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Identity.Contracts.Auth
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
}
