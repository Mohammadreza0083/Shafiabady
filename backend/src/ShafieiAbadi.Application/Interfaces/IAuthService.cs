using ShafieiAbadi.Application.Common;
using ShafieiAbadi.Application.DTOs;

namespace ShafieiAbadi.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
}
