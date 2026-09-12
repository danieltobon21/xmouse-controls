// <copyright file="MainWindow.xaml.cs" company="Joel Purra">
// X-Mouse Controls by Joel Purra
// Copyright © 2007, 2008, 2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018, 2019, 2020, 2021, 2022, 2023.
// All rights reserved. Released under GNU General Public License version 3.0 (GPL-3.0).
//
// - https://joelpurra.com/projects/X-Mouse_Controls/
// - https://github.com/joelpurra/xmouse-controls
// - https://joelpurra.com/
// - https://www.gnu.org/licenses/
// </copyright>
//
// Modificado para TobonMouse (2026, Daniel Tobon): casilla de arranque con Windows, cierre que
// oculta a la bandeja en lugar de salir, textos en espanol y el retardo ya no se pone a cero al
// escribir un valor no numerico en la casilla del retardo.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element must begin with upper-case letter
#pragma warning disable SA1600 // Elements must be documented
namespace XMouseControls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Interop;
    using System.Windows.Navigation;
    using SystemParametersInfo;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WindowTrackingValues windowTrackingValues = new WindowTrackingValues();
        private bool pauseRefresh = false;
        private bool pauseAutoStartHandler = false;
        private TrayIcon trayIcon;

        public MainWindow()
        {
            this.InitializeComponent();

            this.DataContext = this.windowTrackingValues;

            this.LoadAutoStartState();
        }

        /// <summary>Guarda el icono de bandeja para poder mostrar/ocultar la ventana.</summary>
        public void AttachTrayIcon(TrayIcon icon)
        {
            this.trayIcon = icon;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(this.WndProc);
        }

        private void LoadAutoStartState()
        {
            try
            {
                this.pauseAutoStartHandler = true;
                this.startWithWindowsCheckbox.IsChecked = AutoStart.IsEnabled();
            }
            catch (Exception)
            {
                // Si no se puede leer el registro, la casilla queda como esta.
            }
            finally
            {
                this.pauseAutoStartHandler = false;
            }
        }

        private void GetValues()
        {
            // Defaults to be overwritten when reading system settings.
            bool windowTrackingIsEnabled = false;
            bool windowRaisingIsEnabled = false;
            uint activeWindowTrkTimeout = (uint)WindowTrackingValues.DefaultDelay;

            try
            {
                windowTrackingIsEnabled = Helpers.GetActiveWindowTracking();
            }
            catch
            {
                MessageBox.Show("No se pudo leer el seguimiento de ventanas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            try
            {
                windowRaisingIsEnabled = Helpers.GetActiveWindowRaising();
            }
            catch
            {
                MessageBox.Show("No se pudo leer el traer-al-frente (raising).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            try
            {
                activeWindowTrkTimeout = Helpers.GetActiveWindowDelay();
            }
            catch
            {
                MessageBox.Show("No se pudo leer el retardo del seguimiento de ventanas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            this.windowTrackingValues.IsTrackingEnabled = windowTrackingIsEnabled;
            this.windowTrackingValues.IsRaisingEnabled = windowRaisingIsEnabled;
            this.windowTrackingValues.Delay = activeWindowTrkTimeout;
        }

        private void SetValues()
        {
            try
            {
                Helpers.SetActiveWindowTracking(this.windowTrackingValues.IsTrackingEnabled);
            }
            catch
            {
                MessageBox.Show("No se pudo aplicar el seguimiento de ventanas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            try
            {
                Helpers.SetActiveWindowRaising(this.windowTrackingValues.IsRaisingEnabled);
            }
            catch
            {
                MessageBox.Show("No se pudo aplicar el traer-al-frente (raising).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            try
            {
                Helpers.SetActiveWindowDelay((uint)this.windowTrackingValues.Delay);
            }
            catch
            {
                MessageBox.Show("No se pudo aplicar el retardo del seguimiento de ventanas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyValues()
        {
            this.pauseRefresh = true;
            this.SetValues();
            this.pauseRefresh = false;
        }

        private void RefreshValues()
        {
            if (this.pauseRefresh != true)
            {
                this.GetValues();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Not looking very nice, but it's a workaround for standalone applications.
            // https://laurenlavoie.com/avalon/159
            Uri uri = ((Hyperlink)sender).NavigateUri;
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshValues();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Cerrar no sale de la aplicacion: se queda en la bandeja para que el ajuste siga
            // activo. Para salir de verdad, clic derecho en el icono -> Salir.
            if (this.trayIcon == null)
            {
                return;
            }

            e.Cancel = true;
            this.trayIcon.HideWindow();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            this.ApplyValues();
        }

        private void startWithWindowsCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (this.pauseAutoStartHandler)
            {
                return;
            }

            bool enable = this.startWithWindowsCheckbox.IsChecked == true;

            try
            {
                AutoStart.Set(enable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cambiar el arranque con Windows: " + ex.Message,
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                this.LoadAutoStartState();
            }
        }

        private void delayTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            uint delay;

            // Si lo escrito no es un numero, se ignora: antes esto ponia el retardo a 0.
            if (!uint.TryParse(this.delayTextbox.Text, out delay))
            {
                return;
            }

            this.windowTrackingValues.Delay = delay;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case (int)WM.SETTINGCHANGE:
                    this.RefreshValues();
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
