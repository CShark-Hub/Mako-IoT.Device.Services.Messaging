using MakoIoT.Device.Services.DependencyInjection;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.Messaging.Extensions
{
    public delegate void MessageBusConfigurator(MessageBusOptions options);

    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddMessageBus(this IDeviceBuilder builder)
        {
            return builder.AddMessageBus(o => { });
        }

        private static void Builder_DeviceStopped(object sender, System.EventArgs e)
        {
            var mb = (IMessageBus)DI.Resolve(typeof(IMessageBus));
            mb.Stop();
        }

        private static void Builder_DeviceStarting(object sender, System.EventArgs e)
        {
            var mb = (IMessageBus)DI.Resolve(typeof(IMessageBus));
            mb.Start();
        }

        public static IDeviceBuilder AddMessageBus(this IDeviceBuilder builder, MessageBusConfigurator configurator)
        {
            DI.RegisterSingleton(typeof(IMessageBus), typeof(MessageBus));
            var options = new MessageBusOptions();
            configurator(options);
            DI.RegisterInstance(typeof(MessageBusOptions), options);

            builder.DeviceStarting += Builder_DeviceStarting;
            builder.DeviceStopped += Builder_DeviceStopped;


            return builder;
        }
    }
}
