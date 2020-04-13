using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Projet_Final_CS___Music_Player
{
	class ContextMenuCommands
	{
		private static RoutedUICommand _removeItem = new RoutedUICommand("Remove Item", "RemoveItem", typeof(ContextMenuCommands));
		public static ICommand RemoveItem { get; set; }

		static ContextMenuCommands()
		{
			// Register CommandBinding for all windows.
			CommandManager.RegisterClassCommandBinding(typeof(Window), new CommandBinding(RemoveItem, RemoveItem_Executed, RemoveItem_CanExecute));
		}

        #region RemoveItem
        internal static void RemoveItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(e.Parameter is Song)) return;

            e.Handled = true;
            Console.WriteLine("Remove accessed");
        }

        internal static void RemoveItem_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!(e.Parameter is Song)) return;

            e.CanExecute = true;
            var item = (e.Parameter as Song);
            // TODO : complete the execution code ...
        }
        #endregion
    }

    /*
         <ListView.ContextMenu>
                        <ContextMenu>
                            <MenuItem Header="Remove"
                                Command="{Binding RemoveItem}"
                                CommandParameter="{Binding RelativeSource={RelativeSource AncestorType=ContextMenu}, Path=PlacementTarget.SelectedItem}" />

                            <!--MenuItem Header="Play"
                                Command="{Binding PlayItem}"
                                CommandParameter="{Binding RelativeSource={RelativeSource AncestorType=ContextMenu}, Path=PlacementTarget.SelectedItem}" /-->
                        </ContextMenu>
         </ListView.ContextMenu>
    */
}
