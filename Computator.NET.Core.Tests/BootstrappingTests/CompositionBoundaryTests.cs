using System.Collections.Generic;
using Computator.NET.Core.Bootstrapping;
using NUnit.Framework;
using Unity;
using Unity.Lifetime;

namespace Computator.NET.Core.Tests.BootstrappingTests
{
    [TestFixture]
    public class CompositionBoundaryTests
    {
        [Test]
        public void NestedFactoryAliasesAndDirectConsumersShareTheRoleBoundEditor()
        {
            using (var container = new UnityContainer())
            {
                var editor = new Editor();
                container.RegisterTypeLegacy<IWindow, Window>(new ContainerControlledLifetimeManager());
                container.RegisterTypeLegacy<IPanel, Panel>(new ContainerControlledLifetimeManager());
                var roles = new FuzzyMatchingParameterOverrideWithFallback<Editor>(new Dictionary<string, Editor> { { "customFunction", editor } });
                container.UseCompositionOverrides(roles.Overrides);
                var window = container.ResolveComposed<IWindow>();
                NUnit.Framework.Assert.That(window.Panel.Editor, Is.SameAs(editor));
                NUnit.Framework.Assert.That(container.ResolveComposed<Panel>().Editor, Is.SameAs(editor));
                NUnit.Framework.Assert.That(container.ResolveComposed<Consumer>().Editor, Is.SameAs(editor));
                NUnit.Framework.Assert.That(container.ResolveComposed<Window>(), Is.SameAs(window));
            }
        }
        [Test]
        public void SeparateCompositionRootsDoNotShareEditorBindings()
        {
            using (var first = new UnityContainer())
            using (var second = new UnityContainer())
            {
                first.UseCompositionOverrides(new FuzzyMatchingParameterOverrideWithFallback<Editor>(
                    new Dictionary<string, Editor> { { "customFunction", new Editor() } }).Overrides);
                second.UseCompositionOverrides(new FuzzyMatchingParameterOverrideWithFallback<Editor>(
                    new Dictionary<string, Editor> { { "customFunction", new Editor() } }).Overrides);
                NUnit.Framework.Assert.That(first.ResolveComposed<Consumer>().Editor, Is.Not.SameAs(second.ResolveComposed<Consumer>().Editor));
            }
        }
        public class Editor { }
        public interface IWindow { IPanel Panel { get; } }
        public interface IPanel { Editor Editor { get; } }
        public class Window : IWindow
        {
            public Window(IPanel panel) { Panel = panel; }
            public IPanel Panel { get; }
        }
        public class Panel : IPanel
        {
            public Panel(Editor customFunctionsEditor) { Editor = customFunctionsEditor; }
            public Editor Editor { get; }
        }
        public class Consumer
        {
            public Consumer(Editor customFunctionsEditor) { Editor = customFunctionsEditor; }
            public Editor Editor { get; }
        }
    }
}
