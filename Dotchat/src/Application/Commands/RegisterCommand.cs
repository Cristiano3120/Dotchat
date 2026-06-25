using DotchatShared.src.DTOs.AuthRequests;
using DotchatShared.src.Enums;

namespace DotchatServer.src.Application.Commands;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string Username,
    string DisplayName,
    Platform Platform,
    DateTimeOffset Birthday,
    Guid DeviceId,
    string? DeviceName
)
{
    public static implicit operator RegisterCommand(RegisterRequest request)
        => new(request.Email, request.Password, request.Username, request.DisplayName, request.Platform!.Value, request.Birthday!.Value, request.DeviceId!.Value, request.DeviceName); 
};