using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Computator.NET.Core.Bootstrapping;
using NUnit.Framework;
using Unity;
using Unity.Lifetime;

namespace Computator.NET.Core.Tests.BootstrappingTests
{
    [TestFixture]
    public class UnityMigrationTests
    {
        [Test]
        public void InterfaceAndConcreteShareTheRegisteredSingleton()
        {
            using (var container = new UnityContainer())
            {
                container.RegisterTypeLegacy<IEditor, Editor>(new ContainerControlledLifetimeManager());
                NUnit.Framework.Assert.That(container.Resolve<IEditor>(), Is.SameAs(container.Resolve<Editor>()));
                NUnit.Framework.Assert.That(container.Resolve<IEditor>(), Is.SameAs(container.Resolve<IEditor>()));
            }
        }

        [Test]
        public void NamedEditorsAndFallbackDoNotExchangeRoles()
        {
            var script = new Editor();
            var custom = new Editor();
            var fallback = new Editor();
            var resolver = new FuzzyMatchingParameterOverrideWithFallback<Editor>(
                new Dictionary<string, Editor> { { "script", script }, { "customFunction", custom } }, fallback);
            using (var container = new UnityContainer())
            {
                var result = container.Resolve<ThreeEditors>(resolver.Overrides);
                NUnit.Framework.Assert.That(result.Script, Is.SameAs(script));
                NUnit.Framework.Assert.That(result.Custom, Is.SameAs(custom));
                NUnit.Framework.Assert.That(result.Other, Is.SameAs(fallback));
            }
        }

        [Test]
        public void SharedOverridesAreSafeAcrossConcurrentResolutions()
        {
            var script = new Editor();
            var custom = new Editor();
            var resolver = new FuzzyMatchingParameterOverrideWithFallback<Editor>(
                new Dictionary<string, Editor> { { "SCRIPT", script }, { "customFunction", custom } }, new Editor());
            using (var container = new UnityContainer())
            {
                Parallel.For(0, 100, _ =>
                {
                    var result = container.Resolve<ThreeEditors>(resolver.Overrides);
                    NUnit.Framework.Assert.That(result.Script, Is.SameAs(script));
                    NUnit.Framework.Assert.That(result.Custom, Is.SameAs(custom));
                });
            }
        }

        [Test]
        public void ConstructorOverridesDoNotReplaceInjectedProperties()
        {
            var propertyValue = new Editor();
            using (var container = new UnityContainer())
            {
                container.RegisterInstance<IEditor>(propertyValue);
                var resolver = new FuzzyMatchingParameterOverrideWithFallback<Editor>(
                    new Dictionary<string, Editor> { { "script", new Editor() } }, new Editor());
                var result = container.Resolve<PropertyConsumer>(resolver.Overrides);
                NUnit.Framework.Assert.That(result.Script, Is.SameAs(propertyValue));
            }
        }

        [Test]
        public void NullParameterMapIsRejected()
        {
            NUnit.Framework.Assert.Throws<ArgumentNullException>(() =>
                new FuzzyMatchingParameterOverrideWithFallback<Editor>(null));
        }

        public interface IEditor { }
        public class Editor : IEditor { }
        public class ThreeEditors
        {
            public IEditor Script { get; }
            public IEditor Custom { get; }
            public IEditor Other { get; }
            public ThreeEditors(IEditor scriptingEditor, IEditor customFunctionsEditor, IEditor otherEditor)
            {
                Script = scriptingEditor;
                Custom = customFunctionsEditor;
                Other = otherEditor;
            }
        }
        public class PropertyConsumer
        {
            [Dependency]
            public IEditor Script { get; set; }
        }
    }
}
