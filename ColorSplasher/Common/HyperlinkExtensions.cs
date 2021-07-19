
#region Using Directives

using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

#endregion

namespace ColorSplasher.Common
{
    public static class HyperlinkExtensions
    {
        #region Static Fields

        /// <summary>
        ///     Dependency Property used to extend the HyberLink element
        ///     By using this Extension property the HyberLink element will open the URL without
        ///     adding additional code to handle it.
        /// </summary>
        public static readonly DependencyProperty IsExternalProperty =
            DependencyProperty.RegisterAttached("IsExternal", typeof (bool), typeof (HyperlinkExtensions),
                new UIPropertyMetadata(false, OnIsExternalChanged));

        #endregion

        #region Methods

        public static bool GetIsExternal(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsExternalProperty);
        }

        public static void SetIsExternal(DependencyObject obj, bool value)
        {
            obj.SetValue(IsExternalProperty, value);
        }

        private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var hyperlink = sender as Hyperlink;

            if ((bool) args.NewValue)
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            else
                hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
        }

        private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion
    }
}