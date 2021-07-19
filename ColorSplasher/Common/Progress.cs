#region Using 
using System;
using System.Threading;
using System.Windows.Threading;
using BIMOne.ViewModels;
using BIMOneAddinManager.Views;
using System.Linq;
#endregion
namespace BIMOne.Common
{
    public class Progress
    {
        #region Private Fields

        private Dispatcher _dispatcher;
        private ProgressViewModel _progressViewModel;
        private ProgressWindow _progressWindow;
        private bool _started;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        ///     Class constructor method
        /// </summary>
        public Progress()
        {
            _started = false;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     ProcessCanceled Event invocator
        /// test
        /// </summary>
        protected virtual void OnProcessCanceled()
        {
            EventHandler<EventArgs> handler = ProcessCanceled;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Event handler fired when Cancel button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessCanceled(object sender, EventArgs e)
        {
            OnProcessCanceled();
        }

        /// <summary>
        ///     Start the progress and show its Window
        /// </summary>
        /// <param name="maxValue"></param>
        public void Start(int maxValue)
        {
            _started = false;
            var thread = new Thread(ThreadWorker);
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = false;

            thread.Start(maxValue);
            while (!_started)
            {
                Thread.Sleep(500);
            }
        }

        /// <summary>
        ///     Internal Thread used to create and show the progress Window
        ///     This prevent the Window from hung and force it to run is a separated thread
        /// </summary>
        /// <param name="param"></param>
        private void ThreadWorker(object param)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _progressViewModel = new ProgressViewModel();
            _progressWindow = new ProgressWindow();
            _progressViewModel.ProcessCanceled += OnProcessCanceled;
            _progressWindow.DataContext = _progressViewModel;

            _progressViewModel.Start((int) param);
            _progressWindow.Show();
            _started = true;
            Dispatcher.Run();
        }

        /// <summary>
        ///     Increment the progress value
        /// </summary>
        /// <param name="value"></param>
        public void Increment(int value)
        {
            _dispatcher.Invoke(new Action (() => _progressViewModel.Increment(value)));
        }

        /// <summary>
        ///     Set the status string
        /// </summary>
        /// <param name="status"></param>
        public void SetStatus(string status)
        {
            _dispatcher.Invoke(new Action (() => _progressViewModel.SetStatus(status)));
        }


        /// <summary>
        ///     End the progress and close the Window
        /// </summary>
        public void End()
        {
            _dispatcher.Invoke(new Action(() =>
            {
                _progressViewModel.End();
                _progressWindow.Close();
            }));
        }

        #endregion  

        /// <summary>
        ///     Event to redirect the Cancel notification from the Window
        /// </summary>
        public event EventHandler<EventArgs> ProcessCanceled;
    }
}