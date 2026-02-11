using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.Identity.Contracts.Auth;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;
using System.Linq;

namespace Modules.Identity.Infrastructure.Auth;

public interface ITwoFactorAuthService
{
    Task<SetupTwoFactorResponse> SetupTwoFactorAsync(string userId, CancellationToken ct = default);
    Task<VerifyTwoFactorResponse> VerifyTwoFactorAsync(string userId, VerifyTwoFactorRequest request, CancellationToken ct = default);
    Task<bool> DisableTwoFactorAsync(string userId, DisableTwoFactorRequest request, CancellationToken ct = default);
    Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken ct = default);
    Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(string userId, CancellationToken ct = default);
    Task<RegenerateBackupCodesResponse> RegenerateBackupCodesAsync(string userId, CancellationToken ct = default);
    Task<bool> UseBackupCodeAsync(string userId, string code, string? ipAddress, string? userAgent, CancellationToken ct = default);
}

public sealed class TwoFactorAuthService : ITwoFactorAuthService
{
    private readonly AuthDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public TwoFactorAuthService(AuthDbContext db, PasswordService passwordService, IConfiguration configuration)
    {
        _db = db;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    public async Task<SetupTwoFactorResponse> SetupTwoFactorAsync(string userId, CancellationToken ct = default)
    {
        // Check if 2FA is already enabled
        var existingTfa = await _db.UserTfaTokens.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (existingTfa?.IsEnabled == true)
        {
            throw new InvalidOperationException("2FA is already enabled for this user.");
        }

        // Generate new secret key
        var secretKey = GenerateSecretKey();
        var issuer = _configuration["Jwt:Issuer"] ?? "Platform MVP";
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        var accountName = user?.Email ?? "Unknown";

        // Create or update TFA token
        if (existingTfa == null)
        {
            existingTfa = new UserTfaToken
            {
                UserId = userId,
                SecretKey = secretKey,
                Issuer = issuer,
                AccountName = accountName,
                IsEnabled = false,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.UserTfaTokens.Add(existingTfa);
        }
        else
        {
            existingTfa.SecretKey = secretKey;
            existingTfa.Issuer = issuer;
            existingTfa.AccountName = accountName;
            existingTfa.IsEnabled = false;
            existingTfa.CreatedAt = DateTimeOffset.UtcNow;
        }

        // Generate backup codes
        var backupCodes = GenerateBackupCodes();
        await SaveBackupCodesAsync(userId, backupCodes, ct);

        await _db.SaveChangesAsync(ct);

        // Generate QR code URI
        var qrCodeUri = GenerateQrCodeUri(secretKey, issuer, accountName);
        var manualEntryKey = FormatManualEntryKey(secretKey);

        return new SetupTwoFactorResponse(qrCodeUri, manualEntryKey, backupCodes, manualEntryKey);
    }

    public async Task<VerifyTwoFactorResponse> VerifyTwoFactorAsync(string userId, VerifyTwoFactorRequest request, CancellationToken ct = default)
    {
        var tfaToken = await _db.UserTfaTokens.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (tfaToken == null)
        {
            throw new InvalidOperationException("2FA setup not found. Please setup 2FA first.");
        }

        if (tfaToken.IsEnabled)
        {
            throw new InvalidOperationException("2FA is already enabled for this user.");
        }

        // Verify the TOTP code
        if (!VerifyTotpCode(tfaToken.SecretKey, request.Code))
        {
            tfaToken.FailedAttempts++;
            if (tfaToken.FailedAttempts >= 3)
            {
                tfaToken.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(5);
            }
            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Invalid verification code.");
        }

        // Enable 2FA
        tfaToken.IsEnabled = true;
        tfaToken.LastVerifiedAt = DateTimeOffset.UtcNow;
        tfaToken.FailedAttempts = 0;
        tfaToken.LockedUntil = null;

        await _db.SaveChangesAsync(ct);

        var remainingCodes = await GetRemainingBackupCodesAsync(userId, ct);
        return new VerifyTwoFactorResponse(true, remainingCodes);
    }

    public async Task<bool> DisableTwoFactorAsync(string userId, DisableTwoFactorRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user == null)
        {
            return false;
        }

        // Verify password
        if (!_passwordService.Verify(user.PasswordHash, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid password.");
        }

        var tfaToken = await _db.UserTfaTokens.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (tfaToken == null || !tfaToken.IsEnabled)
        {
            return false;
        }

        // Verify TOTP code
        if (!VerifyTotpCode(tfaToken.SecretKey, request.Code))
        {
            throw new UnauthorizedAccessException("Invalid verification code.");
        }

        // Disable 2FA and remove backup codes
        _db.UserTfaTokens.Remove(tfaToken);
        var backupCodes = await _db.UserBackupCodes.Where(x => x.UserId == userId).ToListAsync(ct);
        _db.UserBackupCodes.RemoveRange(backupCodes);

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken ct = default)
    {
        var tfaToken = await _db.UserTfaTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.IsEnabled, ct);
        if (tfaToken == null)
        {
            return false;
        }

        // Check if locked out
        if (tfaToken.LockedUntil.HasValue && tfaToken.LockedUntil.Value > DateTimeOffset.UtcNow)
        {
            return false;
        }

        var isValid = VerifyTotpCode(tfaToken.SecretKey, code);

        if (isValid)
        {
            tfaToken.LastVerifiedAt = DateTimeOffset.UtcNow;
            tfaToken.FailedAttempts = 0;
            tfaToken.LockedUntil = null;
        }
        else
        {
            tfaToken.FailedAttempts++;
            if (tfaToken.FailedAttempts >= 3)
            {
                tfaToken.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(5);
            }
        }

        await _db.SaveChangesAsync(ct);
        return isValid;
    }

    public async Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(string userId, CancellationToken ct = default)
    {
        var tfaToken = await _db.UserTfaTokens.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        var remainingCodes = await GetRemainingBackupCodesAsync(userId, ct);

        return new TwoFactorStatusResponse(
            tfaToken?.IsEnabled ?? false,
            remainingCodes.Length > 0,
            remainingCodes.Length,
            tfaToken?.LastVerifiedAt
        );
    }

    public async Task<RegenerateBackupCodesResponse> RegenerateBackupCodesAsync(string userId, CancellationToken ct = default)
    {
        // Remove existing backup codes
        var existingCodes = await _db.UserBackupCodes.Where(x => x.UserId == userId).ToListAsync(ct);
        _db.UserBackupCodes.RemoveRange(existingCodes);

        // Generate new backup codes
        var backupCodes = GenerateBackupCodes();
        await SaveBackupCodesAsync(userId, backupCodes, ct);

        await _db.SaveChangesAsync(ct);
        return new RegenerateBackupCodesResponse(backupCodes);
    }

    public async Task<bool> UseBackupCodeAsync(string userId, string code, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        var codeHash = _passwordService.Hash(code);
        var backupCode = await _db.UserBackupCodes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CodeHash == codeHash && !x.IsUsed, ct);

        if (backupCode == null)
        {
            return false;
        }

        // Mark as used
        backupCode.IsUsed = true;
        backupCode.UsedAt = DateTimeOffset.UtcNow;
        backupCode.UsedIpAddress = ipAddress;
        backupCode.UsedUserAgent = userAgent;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private string GenerateSecretKey()
    {
        var bytes = new byte[20]; // 160 bits for TOTP
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base32Encode(bytes);
    }

    private string[] GenerateBackupCodes()
    {
        var codes = new string[10];
        for (int i = 0; i < 10; i++)
        {
            codes[i] = GenerateBackupCode();
        }
        return codes;
    }

    private string GenerateBackupCode()
    {
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var code = BitConverter.ToUInt32(bytes, 0) % 1000000; // 6 digits
        return code.ToString("D6");
    }

    private async Task SaveBackupCodesAsync(string userId, string[] codes, CancellationToken ct)
    {
        foreach (var code in codes)
        {
            var hashedCode = _passwordService.Hash(code);
            _db.UserBackupCodes.Add(new UserBackupCode
            {
                UserId = userId,
                CodeHash = hashedCode
            });
        }
    }

    private async Task<string[]> GetRemainingBackupCodesAsync(string userId, CancellationToken ct)
    {
        var codes = await _db.UserBackupCodes
            .Where(x => x.UserId == userId && !x.IsUsed)
            .Select(x => x.Id) // We don't return the actual codes, just count
            .ToListAsync(ct);

        return codes.Select((_, index) => $"••••••").ToArray(); // Masked representation
    }

    private string GenerateQrCodeUri(string secretKey, string issuer, string accountName)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedAccountName = Uri.EscapeDataString(accountName);
        return $"otpauth://totp/{encodedIssuer}:{encodedAccountName}?secret={secretKey}&issuer={encodedIssuer}&digits=6";
    }

    private string FormatManualEntryKey(string secretKey)
    {
        // Format as XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX
        var chunks = secretKey.Chunk(4).Select(chunk => new string(chunk));
        return string.Join("-", chunks);
    }

    private bool VerifyTotpCode(string secretKey, string code)
    {
        if (!int.TryParse(code, out var codeValue) || codeValue < 0 || codeValue > 999999)
        {
            return false;
        }

        var secretBytes = Base32Decode(secretKey);
        var timeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30; // 30-second time steps

        // Check current time step and adjacent ones (allow 1 step clock skew)
        for (int offset = -1; offset <= 1; offset++)
        {
            var counter = timeStep + offset;
            var computedCode = GenerateTotp(secretBytes, counter);
            if (computedCode == codeValue)
            {
                return true;
            }
        }

        return false;
    }

    private int GenerateTotp(byte[] secret, long counter)
    {
        using var hmac = new HMACSHA1(secret);
        var counterBytes = BitConverter.GetBytes(counter).Reverse().ToArray();
        var hash = hmac.ComputeHash(counterBytes);

        var offset = hash[hash.Length - 1] & 0x0F;
        var binaryCode = ((hash[offset] & 0x7F) << 24) |
                        ((hash[offset + 1] & 0xFF) << 16) |
                        ((hash[offset + 2] & 0xFF) << 8) |
                        (hash[offset + 3] & 0xFF);

        return binaryCode % 1000000;
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        var bits = 0;
        var bitsCount = 0;

        foreach (var b in data)
        {
            bits = (bits << 8) | b;
            bitsCount += 8;

            while (bitsCount >= 5)
            {
                result.Append(alphabet[(bits >> (bitsCount - 5)) & 0x1F]);
                bitsCount -= 5;
            }
        }

        if (bitsCount > 0)
        {
            result.Append(alphabet[(bits << (5 - bitsCount)) & 0x1F]);
        }

        // Add padding
        var padding = (8 - result.Length % 8) % 8;
        for (int i = 0; i < padding; i++)
        {
            result.Append('=');
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string base32)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32 = base32.TrimEnd('=').ToUpper();
        var result = new List<byte>();
        var bits = 0;
        var bitsCount = 0;

        foreach (var c in base32)
        {
            var value = alphabet.IndexOf(c);
            if (value < 0) continue;

            bits = (bits << 5) | value;
            bitsCount += 5;

            if (bitsCount >= 8)
            {
                result.Add((byte)(bits >> (bitsCount - 8)));
                bitsCount -= 8;
            }
        }

        return result.ToArray();
    }
}
