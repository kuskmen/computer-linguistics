using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace computer_linguistics
{
    public static class Configure
    {
        public static ILogger Logger(Parameters parameters)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning);

                if (parameters.Verbose)
                {
                    builder
                        .AddFilter("computer_linguistics.Program", LogLevel.Debug);
                }
                else
                {
                    builder
                        .AddFilter("computer_linguistics.Program", LogLevel.Information);
                }

                builder.AddConsole();
            });
            return loggerFactory.CreateLogger(nameof(Program));
        }

        public static CrawlerOptions Options(Parameters parameters, ILogger logger)
        {
            var uri = (Uri)null;
            try
            {
                uri = new Uri(parameters.Url, UriKind.Absolute);
                if (uri == null || !uri.IsWellFormedOriginalString())
                {
                    logger.LogError("Url was not in correct format.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Argument: Url was not in valid format. Supplied value: {parameters.Url}");
                return CrawlerOptions.Invalid;
            }

            var regex = (Regex)null;
            try
            {
                regex = new Regex(parameters.RegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Argument: Pattern was not in valid format. Supplied value: {parameters.RegEx}");
                return CrawlerOptions.Invalid;
            }

            return new CrawlerOptions
            {
                StartingPoint = uri,
                SearchPattern = regex,
            };
        }
    }
}
