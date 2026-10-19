using OpenWallet.Shared.DTOs;

namespace OpenWallet.UI.Services;

/// <summary>The answer to an app sign-in: the tokens, or why there are none.</summary>
public sealed record AppLoginResult(AppTokensDto? Tokens, LoginResultDto Result);
