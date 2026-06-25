using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Commands;

public sealed record LoginCommand(
    string Email,
    string Password,
    Platform Platform,
    Guid DeviceId,
    string? DeviceName
)
{
    public static implicit operator LoginCommand(LoginRequest loginRequest) 
        => new(loginRequest.Email, loginRequest.Password, loginRequest.Platform!.Value, loginRequest.DeviceId!.Value, loginRequest.DeviceName)
};