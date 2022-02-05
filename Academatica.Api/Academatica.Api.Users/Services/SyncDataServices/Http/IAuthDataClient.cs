using Academatica.Api.Users.DTOs;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services.SyncDataServices.Http
{
    public interface IAuthDataClient
    {
        Task SendEmailConfirmation(SendConfirmationEmailRequestDto requestDto);
    }
}
