using System;
using System.Threading.Tasks;
using CommandLine;

namespace computer_linguistics
{
    // Да се напише приложение, което приема адрес 
    // към която и да е уеб страница, зарежда изходния ѝ код
    // и търси текст по зададен от потребител шаблон на регулярен израз, 
    // намира чрез регулярни изрази други страници и 
    // ги добавя за по-късно посещение в стек и с тях прави същото. 
    // Намерените текстове да се съхраняват в база данни, както и URL 
    // адресите на вече посетените и обработени уеб страници. Бонус точки,
    // ако се направи асинхронно и многонишково, както и ако се публикува 
    // в git с документация за проекта.
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var crawler = (Crawler)null;
            var crawlerOptions = (CrawlerOptions)null;

            Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed<Parameters>(parameters =>
                {
                    var logger = Configure.Logger(parameters);
                    crawlerOptions = Configure.Options(parameters, logger);

                    if (logger == null || crawlerOptions == null || !crawlerOptions.IsValid)
                    {
                        throw new TypeInitializationException(nameof(Crawler), new Exception("Logger or Crawler Options did not initialize successfully."));
                    }

                    crawler = new Crawler(logger, crawlerOptions);
                });

            await crawler?.Start(crawlerOptions?.StartingPoint.AbsoluteUri);
        }

    }
}
