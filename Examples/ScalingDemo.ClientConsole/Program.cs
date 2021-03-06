﻿using System;
using Gaev.Rpc;
using Gaev.Rpc.Rebus;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Logging;
using Rebus.RabbitMq;
using Rebus.Routing.TypeBased;
using ScalingDemo.Shared;

namespace ScalingDemo.ClientConsole
{
    class Program
    {
        public static string NodeId = Guid.NewGuid().ToString();
        static void Main(string[] args)
        {
            using (var activator = new BuiltinHandlerActivator())
            {
                StartBus(activator);

                IRequestor requestor = new RebusRequestor(activator);

                Console.WriteLine("Client {0}. Type any word then press Enter to test client-server interop. Type 'exit' then press Enter to quit", NodeId);
                while (true)
                {
                    var cmd = Console.ReadLine();
                    if (cmd == "exit") return;

                    var res = (PongMessage)requestor.Ask(new PingMessage { Sender = NodeId, Payload = cmd }).Result;
                    Console.WriteLine("Server {0} received", res.Sender);
                }
            }
        }

        static IBus StartBus(BuiltinHandlerActivator activator)
        {
            return Configure.With(activator)
                .Logging(e => e.ColoredConsole(LogLevel.Warn))
                .Transport(t => t.UseRabbitMq("amqp://localhost", "ScalingDemo-Requestor-" + NodeId))
                .Routing(e => e.TypeBased())
                .Start();
        }
    }
}
