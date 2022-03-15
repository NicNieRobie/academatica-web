using Academatica.Api.Users.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services.RabbitMQ
{
    public interface IMessageBusClient
    {
        void PublishExpChange(ExpChangePublishDto expChangePublishDto);
    }
}
