/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Implements <see cref="T:System.Windows.Input.ICommand"/> with provided delegates.
    /// </summary>
    internal sealed class DelegateCommand : ICommand
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        /// <param name="execute">Delegate to be used for ICommand.Execute.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when execute is null.</exception>
        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class with
        /// delegates implementing Execute and CanExecute methods.
        /// </summary>
        /// <param name="execute">Delegate to be used for ICommand.Execute.</param>
        /// <param name="canExecute">Delegate to be used for
        /// ICommand.CanExecute.</param>
        public DelegateCommand(
            Action<object> execute,
            Func<object, bool> canExecute)
        {
            Debug.Assert(execute != null);

            _execute = execute;
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand Members
        /// <summary>
        /// Occurs when value returned by CanExecute could be changed.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Gets a value indicating if the command could be executed.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        /// <returns>true iff the command could be executed.</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Data used by the command.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion

        #region private fields
        /// <summary>
        /// The action implementing ICommand.Execute method.
        /// </summary>
        private Action<object> _execute;

        /// <summary>
        /// The predicate implementing ICommand.CanExecute method.
        /// </summary>
        private Func<object, bool> _canExecute;
        #endregion
    }
}
