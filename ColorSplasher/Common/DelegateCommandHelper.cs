#region Using
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
#endregion

namespace BIMOne.Common
{
    public class DelegateCommand<T> : DelegateCommandBase
    {
        #region Constructor/Destructor

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base(o => executeMethod((T) o), o => canExecuteMethod((T) o))
        {
        }

        #endregion
    }

    public abstract class DelegateCommandBase : ICommand
    {
        #region Private Fields

        private readonly Func<object, bool> _canExecuteMethod;
        private readonly Action<object> _executeMethod;
        private bool _isActive;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        ///     Creates a new instance of a <see cref="DelegateCommandBase" />, specifying both the execute action and the can
        ///     execute function.
        /// </summary>
        /// <param name="executeMethod">
        ///     The <see cref="Action" /> to execute when <see cref="ICommand.Execute" /> is invoked.
        /// </param>
        /// <param name="canExecuteMethod">
        ///     The <see cref="Func{Object,Bool}" /> to invoked when <see cref="ICommand.CanExecute" /> is invoked.
        /// </param>
        protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", "Delegate Command Delegates Cannot Be Null");

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the object is active; otherwise <see langword="false" />.
        /// </value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnIsActiveChanged();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Raises <see cref="ICommand.CanExecuteChanged" /> on the UI thread so every
        ///     command invoker can query <see cref="ICommand.CanExecute" /> to check if the
        ///     <see cref="CompositeCommand" /> can execute.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
        }


        /// <summary>
        ///     Raises <see cref="DelegateCommandBase.CanExecuteChanged" /> on the UI thread so every command invoker
        ///     can query to check if the command can execute.
        ///     <remarks>
        ///         Note that this will trigger the execution of <see cref="DelegateCommandBase.CanExecute" /> once for each
        ///         invoker.
        ///     </remarks>
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        ///     This raises the <see cref="DelegateCommandBase.IsActiveChanged" /> event.
        /// </summary>
        protected virtual void OnIsActiveChanged()
        {
            EventHandler isActiveChangedHandler = IsActiveChanged;
            if (isActiveChangedHandler != null) isActiveChangedHandler(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Executes the command with the provided parameter by invoking the <see cref="Action{Object}" /> supplied during
        ///     construction.
        /// </summary>
        /// <param name="parameter"></param>
        protected void Execute(object parameter)
        {
            _executeMethod(parameter);
        }

        /// <summary>
        ///     Determines if the command can execute with the provided parameter by invoing the <see cref="Func{Object,Bool}" />
        ///     supplied during construction.
        /// </summary>
        /// <param name="parameter">The parameter to use when determining if this command can execute.</param>
        /// <returns>
        ///     Returns <see langword="true" /> if the command can execute.  <see langword="False" /> otherwise.
        /// </returns>
        protected bool CanExecute(object parameter)
        {
            return _canExecuteMethod == null || _canExecuteMethod(parameter);
        }

        #endregion

        /// <summary>
        ///     Fired if the <see cref="IsActive" /> property changes.
        /// </summary>
        public virtual event EventHandler IsActiveChanged;

        #region ICommand Members

        void ICommand.Execute(object parameter)
        {
            Execute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute. You must keep a hard
        ///     reference to the handler to avoid garbage collection and unexpected results. See remarks for more information.
        /// </summary>
        /// <remarks>
        ///     When subscribing to the <see cref="ICommand.CanExecuteChanged" /> event using
        ///     code (not when binding using XAML) will need to keep a hard reference to the event handler. This is to prevent
        ///     garbage collection of the event handler because the command implements the Weak Event pattern so it does not have
        ///     a hard reference to this handler. An example implementation can be seen in the CompositeCommand and
        ///     CommandBehaviorBase
        ///     classes. In most scenarios, there is no reason to sign up to the CanExecuteChanged event directly, but if you do,
        ///     you
        ///     are responsible for maintaining the reference.
        /// </remarks>
        /// <example>
        ///     The following code holds a reference to the event handler. The myEventHandlerReference value should be stored
        ///     in an instance member to avoid it from being garbage collected.
        ///     <code>
        /// EventHandler myEventHandlerReference = new EventHandler(this.OnCanExecuteChanged);
        /// command.CanExecuteChanged += myEventHandlerReference;
        /// </code>
        /// </example>
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}