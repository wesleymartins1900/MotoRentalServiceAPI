using System.Text.Json;
using MotoRentalService.Domain.Entities;
using MotoRentalService.CrossCutting.Logging;
using System.Data.Common;
using Microsoft.Extensions.Options;
using MotoRentalService.Domain.Contracts.Repositories;
using MotoRentalService.Infrastructure.Messaging;

namespace MotoRentalService.Application.Consumer
{
    public class MotoRegisteredConsumer(IMotoRepository motoRepository, ILoggerManager logger, IOptions<RabbitMQConfig> rabbitMQConfig) : RabbitMQConsumer(rabbitMQConfig, logger)
    {
        private readonly IMotoRepository _motoRepository = motoRepository;
        private readonly ILoggerManager _logger = logger;

        protected override async Task ProcessMessageAsync(string message)
        {
            _logger.LogInfo($"Processing message: {message}");

            try
            {
                var moto = JsonSerializer.Deserialize<Moto>(message);
                if (moto is not null && moto.Year is 2024)
                {
                    await _motoRepository.AddAsync(moto);
                    _logger.LogInfo($"Moto with ID: {moto.Id} and year 2024 registered successfully.");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error deserializing message: {message}\r\nException Details: {jsonEx}");
            }
            catch (DbException dbEx)
            {
                _logger.LogError($"Database error processing message: {message}\r\nException Details: {dbEx}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {message}\r\nException Details: {ex}");
            }
        }
    }
}