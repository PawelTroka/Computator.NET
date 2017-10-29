using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services;
using Computator.NET.Core.Services.ErrorHandling;
using Unity;
using Unity.Lifetime;

namespace Computator.NET.Core.Bootstrapping
{
    public class CoreBootstrapper
    {
        public IUnityContainer Container { get; }

        public CoreBootstrapper() : this(new UnityContainer())
        {
        }

        public CoreBootstrapper(IUnityContainer container)
        {
            Container = container;
            RegisterSharedObjects();
            RegisterHandlers();
            RegisterModel();
        }

        public virtual T Create<T>()
        {
            return Container.Resolve<T>();
        }

        public void RegisterInstance<TInterface>(TInterface instance)
        {
            Container.RegisterInstance(typeof(TInterface), (string)null, (object)instance, (LifetimeManager)new ContainerControlledLifetimeManager());
        }

        private void RegisterModel()
        {
            //models and business objects
            Container.RegisterType<IModeDeterminer, ModeDeterminer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITslCompiler, TslCompiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptEvaluator, ScriptEvaluator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IExpressionsEvaluator, ExpressionsEvaluator>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IFunctionsDetailsFileSource, FunctionsDetailsFileSource>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAutocompleteReflectionSource, AutocompleteReflectionSource>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAutocompleteProvider, AutocompleteProvider>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IProcessRunnerService, ProcessRunnerService>(new ContainerControlledLifetimeManager());
        }

        private void RegisterHandlers()
        {
            //singleton handlers
            Container.RegisterType<IErrorHandler, SimpleErrorHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IExceptionsHandler, ExceptionsHandler>(new ContainerControlledLifetimeManager());
        }

        private void RegisterSharedObjects()
        {
            //shared singletons
            Container.RegisterType<ISharedViewState, SharedViewState>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICommandLineHandler, CommandLineHandler>(new ContainerControlledLifetimeManager());
        }
    }
}