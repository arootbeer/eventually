using System;
using Microsoft.Extensions.Logging;

namespace Eventually.Utilities.Logging
{
    public static class ApplicationLogging
    {
		private static ILoggerFactory _factory = null;

        public static ILoggerFactory LoggerFactory
        {
            get => _factory ?? throw new NullReferenceException($"The {nameof(LoggerFactory)} has not been initialized.");
            set => _factory = value;
        }

        public static ILogger CreateLogger<TLogging>() => LoggerFactory.CreateLogger<TLogging>();

        public static ILogger CreateLogger(this object input) => LoggerFactory.CreateLogger(input.GetType());

        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
    }
}