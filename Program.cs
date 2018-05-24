using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmSerializer
{
    internal class Program
    {
        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var bus = Bus.Factory.CreateUsingInMemory(cfg =>
            {
                cfg.UseXmlSerializer();
                cfg.UseInMemoryScheduler();

                cfg.ReceiveEndpoint("queue", c =>
                {
                    c.Consumer<Consumer>();
                });
            });

            await bus.StartAsync();

            await bus.Publish<IMyMessage>(new { Description = "hi!" });
        }
    }

    public interface IMyMessage
    {
        string Description { get; }
    }

    public class Consumer : IConsumer<IMyMessage>
    {
        public async Task Consume(ConsumeContext<IMyMessage> context)
        {
            if (context.Headers.TryGetHeader("MT-Redelivery-Count", out var value))
            {
                if (context.Headers.TryGetHeader("#text", out var val))
                {
                    throw new Exception("this is a weird header");
                }

                return;
            }

            if (value == null)
            {
                await context.Redeliver(TimeSpan.FromSeconds(1));
            }
        }
    }
}