using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace MakoIoT.Device.Services.Messaging.Extensions
{
    public delegate void MessageBusConfigurator(MessageBusOptions options);

    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddMessageBus(this IDeviceBuilder builder)
        {
            return builder.AddMessageBus(o => { });
        }

        private static void Builder_DeviceStopped(IDevice sender)
        {
            var mb = (IMessageBus)sender.ServiceProvider.GetService(typeof(IMessageBus));
            mb.Stop();
        }

        private static void Builder_DeviceStarting(IDevice sender)
        {
            var mb = (IMessageBus)sender.ServiceProvider.GetService(typeof(IMessageBus));
            mb.Start();
        }

        public static IDeviceBuilder AddMessageBus(this IDeviceBuilder builder, MessageBusConfigurator configurator)
        {
            builder.Services.AddSingleton(typeof(IMessageBus), typeof(MessageBus));
            var options = new MessageBusOptions();
            configurator(options);
            builder.Services.AddSingleton(typeof(MessageBusOptions), options);

            builder.DeviceStarting += Builder_DeviceStarting;
            builder.DeviceStopped += Builder_DeviceStopped;


            return builder;
        }
    }
}
