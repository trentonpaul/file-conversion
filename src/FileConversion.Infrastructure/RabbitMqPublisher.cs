using FileConversion.Shared.Interfaces;
using FileConversion.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Infrastructure
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        public Task PublishAsync(ConversionJob job)
        {
            throw new NotImplementedException();
        }
    }
}
