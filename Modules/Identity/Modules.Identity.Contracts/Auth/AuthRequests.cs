
namespace Modules.Identity.Contracts.Auth
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    
    // Request validation attributes
    public class LoginRequestValidator
    {
        public static bool IsValid(LoginRequest request)
        {
            return !string.IsNullOrWhiteSpace(request.Email) && 
                   !string.IsNullOrWhiteSpace(request.Password) &&
                   request.Email.Contains("@");
        }
    }
}
