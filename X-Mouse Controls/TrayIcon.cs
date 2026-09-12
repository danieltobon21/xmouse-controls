// TobonMouse - utilidad para el comportamiento x-mouse (el foco sigue al raton) en Windows.
// Basada en X-Mouse Controls de Joel Purra (GPL-3.0); ver LICENSE y README.md.
//
// Modificaciones 2026 (Daniel Tobon): icono de bandeja con menu (abrir / salir), doble clic
// para mostrar la ventana y globo informativo la primera vez que se oculta.

namespace XMouseControls
{
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    /// <summary>
    /// Icono en el area de notificacion. Mantiene la aplicacion viva cuando la ventana se
    /// oculta, de modo que el ajuste aplicado siga activo sin tener una ventana abierta.
    /// </summary>
    internal sealed class TrayIcon : IDisposable
    {
        private readonly Window window;
        private readonly NotifyIcon notifyIcon;
        private bool hintShown;

        public TrayIcon(Window window)
        {
            this.window = window ?? throw new ArgumentNullException(nameof(window));

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Abrir TobonMouse", null, (sender, args) => this.ShowWindow());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Salir", null, (sender, args) => this.ExitApplication());

            this.notifyIcon = new NotifyIcon
            {
                Icon = LoadApplicationIcon(),
                Text = "TobonMouse",
                ContextMenuStrip = menu,
                Visible = true,
            };
            this.notifyIcon.DoubleClick += (sender, args) => this.ShowWindow();
        }

        /// <summary>Muestra la ventana principal y la trae al frente.</summary>
        public void ShowWindow()
        {
            this.window.Show();

            if (this.window.WindowState == WindowState.Minimized)
            {
                this.window.WindowState = WindowState.Normal;
            }

            this.window.Activate();
        }

        /// <summary>Oculta la ventana (sigue en la bandeja). Avisa una sola vez.</summary>
        public void HideWindow()
        {
            this.window.Hide();

            if (this.hintShown)
            {
                return;
            }

            this.hintShown = true;

            try
            {
                this.notifyIcon.BalloonTipTitle = "TobonMouse sigue activo";
                this.notifyIcon.BalloonTipText = "La aplicacion queda en la bandeja. Doble clic para volver a abrirla; clic derecho para salir.";
                this.notifyIcon.ShowBalloonTip(5000);
            }
            catch (Exception)
            {
                // Un globo fallido no debe romper nada.
            }
        }

        public void Dispose()
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Visible = false;
                this.notifyIcon.Dispose();
            }
        }

        private void ExitApplication()
        {
            this.Dispose();
            System.Windows.Application.Current?.Shutdown();
        }

        private static Icon LoadApplicationIcon()
        {
            try
            {
                string path = AutoStart.ExecutablePath;

                if (!string.IsNullOrEmpty(path))
                {
                    Icon extracted = Icon.ExtractAssociatedIcon(path);

                    if (extracted != null)
                    {
                        return extracted;
                    }
                }
            }
            catch (Exception)
            {
                // Se cae al icono del sistema.
            }

            return SystemIcons.Application;
        }
    }
}
