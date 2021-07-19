
#region Using Directives
using System;
using System.ComponentModel;
using System.Windows.Input;
using ColorSplasher.Annotations;
using BIMOne.Common;
#endregion

namespace BIMOne.ViewModels
{
    internal class ProgressViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private int _maxValue;
        private string _status;
        private int _value;
        #endregion

        #region Constructor/Destructor

        /// <summary>
        ///     Class Constructor
        /// </summary>
        public ProgressViewModel()
        {

            Command = new DelegateCommand<object>(OnSubmit, CanSubmit);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Used to Bind Buttons elements
        /// </summary>
        public ICommand Command { get; private set; }

        /// <summary>
        ///     Bind to Progress.MaxValue
        /// </summary>
        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                if (value == _maxValue) return;
                _maxValue = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        ///     Bind to Progress.Value
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Bind to Status Label
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     ProcessCanceled event invocator
        /// </summary>
        protected virtual void OnProcessCanceled()
        {
            EventHandler<EventArgs> handler = ProcessCanceled;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Handle the Click event of the bind buttons
        /// </summary>
        /// <param name="arg"></param>
        private void OnSubmit(object arg)
        {
            switch (arg.ToString())
            {
                case "Cancel":
                    OnProcessCanceled();
                    break;
            }
        }

        /// <summary>
        ///     Not used - This is part of ICommand implementation can be used to control
        ///     the buttons click behavior (Can fire the click event or not)
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private bool CanSubmit(object arg)
        {
            return true;
        }

        /// <summary>
        ///     Start the progress and reset it
        /// </summary>
        /// <param name="maxValue"></param>
        public void Start(int maxValue)
        {
            Value = 0;
            Status = "";
            MaxValue = maxValue;
        }

        /// <summary>
        ///     Increment the progress value
        /// </summary>
        /// <param name="value"></param>
        public void Increment(int value)
        {
            Value += value;
        }

        /// <summary>
        ///     Set the status text
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(string status)
        {
            Status = status;
        }

        /// <summary>
        ///     End the progress by setting its value to MaxValue
        /// </summary>
        public void End()
        {
            Value = MaxValue;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        ///     Event to notify the owner that the Cancel button was clicked
        /// </summary>
        public event EventHandler<EventArgs> ProcessCanceled;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}