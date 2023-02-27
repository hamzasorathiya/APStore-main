using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Autofac;
using Nop.Plugin.Misc.Picker.Services;
using Nop.Web.Framework.Mvc;
using Nop.Plugin.Misc.Picker.Data;
using Nop.Plugin.Misc.Picker.Models;
using Nop.Data;
using Nop.Core.Data;
using Autofac.Core;

namespace Nop.Plugin.Misc.Picker
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<PickerService>().As<IPickerService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<PickerObjectContext>(builder, "nop_object_context_Picker");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<OrderTrack>>()
                .As<IRepository<OrderTrack>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_Picker"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PickerShippingInformation>>()
                .As<IRepository<PickerShippingInformation>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_Picker"))
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
