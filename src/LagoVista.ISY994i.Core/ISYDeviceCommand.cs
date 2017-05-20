using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LagoVista.ISY994i.Core
{
    public class ISYDeviceCommand : ICommand
    {
        readonly Action<object> execute;
        readonly Predicate<object> canExecute;

        public ISYDeviceCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public ISYDeviceCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
