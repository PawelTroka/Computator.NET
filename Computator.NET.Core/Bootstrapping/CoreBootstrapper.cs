using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services;
using Computator.NET.Core.Services.ErrorHandling;
using Unity;
using Unity.Lifetime;
using Unity.Registration;

namespace Computator.NET.Core.Bootstrapping
{
    public static class ContainerExtensions
    {
        public static void RegisterTypeLegacy<TFrom, TTo>(this IUnityContainer container, params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            container.RegisterTypeLegacy<TFrom, TTo>(null, injectionMembers);
        }

        public static void RegisterTypeLegacy<TFrom, TTo>(this IUnityContainer container, LifetimeManager lifetimeManager, params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            container.RegisterType<TTo>(lifetimeManager);
            container.RegisterType<TFrom, TTo>();
        }
    }
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
            Container.RegisterTypeLegacy<IModeDeterminer, ModeDeterminer>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<ITslCompiler, TslCompiler>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IScriptEvaluator, ScriptEvaluator>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IExpressionsEvaluator, ExpressionsEvaluator>(new ContainerControlledLifetimeManager());

            Container.RegisterTypeLegacy<IFunctionsDetailsFileSource, FunctionsDetailsFileSource>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IAutocompleteReflectionSource, AutocompleteReflectionSource>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IAutocompleteProvider, AutocompleteProvider>(new ContainerControlledLifetimeManager());

            Container.RegisterTypeLegacy<IProcessRunnerService, ProcessRunnerService>(new ContainerControlledLifetimeManager());
        }

        private void RegisterHandlers()
        {
            //singleton handlers
            Container.RegisterTypeLegacy<IErrorHandler, SimpleErrorHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IExceptionsHandler, ExceptionsHandler>(new ContainerControlledLifetimeManager());
        }

        private void RegisterSharedObjects()
        {
            //shared singletons
            Container.RegisterTypeLegacy<ISharedViewState, SharedViewState>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<ICommandLineHandler, CommandLineHandler>(new ContainerControlledLifetimeManager());
        }
    }
}