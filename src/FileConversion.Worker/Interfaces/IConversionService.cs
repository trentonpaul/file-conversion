using FileConversion.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileConversion.Worker.Interfaces
{
    public interface IConversionService
    {
        Task<string> ConvertAsync(ConversionJob job);
    }
}
