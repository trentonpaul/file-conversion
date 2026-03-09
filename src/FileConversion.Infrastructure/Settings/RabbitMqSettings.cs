using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Infrastructure.Settings
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = String.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string QueueName { get; set; } = String.Empty;
    }
}
