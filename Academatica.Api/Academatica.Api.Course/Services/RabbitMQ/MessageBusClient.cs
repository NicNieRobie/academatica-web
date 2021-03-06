using Academatica.Api.Users.DTOs;
using Academatica.Api.Users.Services.RabbitMQ;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace Academatica.Api.Course.Services.RabbitMQ
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]),
                UserName = "rabbitmq",
                Password = "rabbitmq"
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
                Console.WriteLine("--> Connected to the Message Bus.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public void PublishExpChange(ExpChangePublishDto expChangePublishDto)
        {
            var message = JsonSerializer.Serialize(expChangePublishDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connectionis closed, not sending");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "trigger",
                            routingKey: "",
                            basicProperties: null,
                            body: body);
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }
    }
}
