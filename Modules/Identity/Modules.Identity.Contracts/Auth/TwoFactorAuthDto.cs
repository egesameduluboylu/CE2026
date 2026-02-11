namespace Modules.Identity.Contracts.Auth;

/// <summary>
/// Request to setup 2FA for a user
/// </summary>
public sealed record SetupTwoFactorRequest;

/// <summary>
/// Response with 2FA setup information (QR code, secret key, backup codes)
/// </summary>
public sealed record SetupTwoFactorResponse(
    string QrCodeUri,
    string SecretKey,
    string[] BackupCodes,
    string ManualEntryKey
);

/// <summary>
/// Request to verify and enable 2FA
/// </summary>
public sealed record VerifyTwoFactorRequest(
    string Code,
    string[] BackupCodes
);

/// <summary>
/// Response after successful 2FA verification
/// </summary>
public sealed record VerifyTwoFactorResponse(
    bool IsEnabled,
    string[] RemainingBackupCodes
);

/// <summary>
/// Request to disable 2FA
/// </summary>
public sealed record DisableTwoFactorRequest(
    string Password,
    string Code
);

/// <summary>
/// Request to verify 2FA during login
/// </summary>
public sealed record TwoFactorVerificationRequest(
    string Code
);

/// <summary>
/// Response with 2FA status
/// </summary>
public sealed record TwoFactorStatusResponse(
    bool IsEnabled,
    bool HasBackupCodes,
    int RemainingBackupCodes,
    DateTimeOffset? LastVerifiedAt
);

/// <summary>
/// Request to regenerate backup codes
/// </summary>
public sealed record RegenerateBackupCodesRequest(
    string Password
);

/// <summary>
/// Response with new backup codes
/// </summary>
public sealed record RegenerateBackupCodesResponse(
    string[] BackupCodes
);
