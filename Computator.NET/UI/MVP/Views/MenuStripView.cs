using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Computator.NET.UI.Commands;

namespace Computator.NET.UI.MVP.Views
{
    public partial class MenuStripView : UserControl, IToolbarView
    {
        public MenuStripView()
        {
            InitializeComponent();
        }

        public void SetCommands(IEnumerable<IToolbarCommand> commands)
        {
            menuStrip2.Items.Clear();
            foreach (var command in commands)
            {
                if (command == null)
                {
                    menuStrip2.Items.Add(new ToolStripSeparator());
                    continue;
                }

                ToolStripItem button;

                button = CommandToButton(command);
                menuStrip2.Items.Add(button);
                AddChildren(button, command.ChildrenCommands);
            }
        }

        private static ToolStripComboBox CommandToComboBox(dynamic command)
        {

            var button = new ToolStripComboBox()
            {
             
                ToolTipText = command.ToolTip?.Replace(@"&", ""), //hack for accelerator keys
                Text = command.Text,
                Image = command.Icon,
                Enabled = command.IsEnabled,
                ImageScaling = ToolStripItemImageScaling.None,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            if(command.DisplayProperty!=null)
            button.ComboBox.DisplayMember = command.DisplayProperty;
            button.Items.Clear();
            button.Items.AddRange(command.Items);
            button.SelectedItem = command.SelectedItem;
            var c = command; // create a closure around the command
            /*command.PropertyChanged += (object o, EventArgs s) =>
            {
               // button.ToolTipText = c.ToolTip;

                button.Text = c.ToolTip;
                button.Image = c.Icon;
                button.Enabled = c.IsEnabled;
            };*/


                button.SelectedIndexChanged += (sender, args) =>
                {
                    c.SelectedItem = (dynamic)button.SelectedItem;
                    c.Execute();
                };
            return button;
        }

        private static ToolStripMenuItem CommandToButton(IToolbarCommand command)
        {

            var button = new ToolStripMenuItem()
            {
                Checked = command.Checked,
                CheckOnClick = command.CheckOnClick,
                ToolTipText = command.ToolTip.Replace(@"&", ""), //hack for accelerator keys
                Text = command.Text,
                Image = command.Icon,
                Enabled = command.IsEnabled,
                ImageScaling = ToolStripItemImageScaling.None,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ShowShortcutKeys = true,
                ShortcutKeyDisplayString = command.ShortcutKeyString
            };
            var c = command; // create a closure around the command
            command.PropertyChanged += (o, s) =>
            {
                button.CheckOnClick = c.CheckOnClick;
                button.Checked = c.Checked;
                button.ToolTipText = c.ToolTip;

                button.Text = c.ToolTip;
                button.Image = c.Icon;
                button.Enabled = c.IsEnabled;
            };

            if (!(command is DummyCommand))
                button.Click += (sender, args) => c.Execute();
            return button;
        }

        private void AddChildren(ToolStripItem button, IEnumerable<IToolbarCommand> childrenCommands)
        {
            if (childrenCommands == null)
                return;
            foreach (var childrenCommand in childrenCommands)
            {
                if (childrenCommand == null)
                {
                    (button as ToolStripMenuItem)?.DropDownItems.Add(new ToolStripSeparator());
                    continue;
                }

                ToolStripItem newButton;

                if (childrenCommand.GetType().IsSubClassOfGeneric(typeof(DropDownCommand<>)))
                {
                    newButton = CommandToComboBox(childrenCommand);
                    (button as ToolStripMenuItem)?.DropDownItems.Add(newButton);

                }
                else
                {
                    newButton = CommandToButton(childrenCommand);
                    (button as ToolStripMenuItem)?.DropDownItems.Add(newButton);
                }

                AddChildren(newButton, childrenCommand.ChildrenCommands);
            }
        }
    }

    public static class ReflexionExtension
    {
        public static bool IsSubClassOfGeneric(this Type child, Type parent)
        {
            if (child == parent)
                return false;

            if (child.IsSubclassOf(parent))
                return true;

            var parameters = parent.GetGenericArguments();
            var isParameterLessGeneric = !(parameters != null && parameters.Length > 0 &&
                ((parameters[0].Attributes & TypeAttributes.BeforeFieldInit) == TypeAttributes.BeforeFieldInit));

            while (child != null && child != typeof(object))
            {
                var cur = GetFullTypeDefinition(child);
                if (parent == cur || (isParameterLessGeneric && cur.GetInterfaces().Select(i => GetFullTypeDefinition(i)).Contains(GetFullTypeDefinition(parent))))
                    return true;
                else if (!isParameterLessGeneric)
                    if (GetFullTypeDefinition(parent) == cur && !cur.IsInterface)
                    {
                        if (VerifyGenericArguments(GetFullTypeDefinition(parent), cur))
                            if (VerifyGenericArguments(parent, child))
                                return true;
                    }
                    else
                        foreach (var item in child.GetInterfaces().Where(i => GetFullTypeDefinition(parent) == GetFullTypeDefinition(i)))
                            if (VerifyGenericArguments(parent, item))
                                return true;

                child = child.BaseType;
            }

            return false;
        }

        private static Type GetFullTypeDefinition(Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        private static bool VerifyGenericArguments(Type parent, Type child)
        {
            Type[] childArguments = child.GetGenericArguments();
            Type[] parentArguments = parent.GetGenericArguments();
            if (childArguments.Length == parentArguments.Length)
                for (int i = 0; i < childArguments.Length; i++)
                    if (childArguments[i].Assembly != parentArguments[i].Assembly || childArguments[i].Name != parentArguments[i].Name || childArguments[i].Namespace != parentArguments[i].Namespace)
                        if (!childArguments[i].IsSubclassOf(parentArguments[i]))
                            return false;

            return true;
        }
    }
}
