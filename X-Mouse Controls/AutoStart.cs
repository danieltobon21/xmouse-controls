// TobonMouse - utilidad para el comportamiento x-mouse (el foco sigue al raton) en Windows.
// Basada en X-Mouse Controls de Joel Purra (GPL-3.0); ver LICENSE y README.md.
//
// Modificaciones 2026 (Daniel Tobon): arranque con Windows (HKCU\...\Run) y lectura/escritura
// del valor de la clave de inicio.

namespace XMouseControls
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Win32;

    /// <summary>
    /// Alta y baja de TobonMouse en el arranque de Windows. Se usa la clave Run del usuario
    /// (HKCU), que no necesita permisos de administrador: los ajustes de x-mouse tambien son
    /// por usuario, asi que no tiene sentido elevarlo.
    /// </summary>
    public static class AutoStart
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>Nombre del valor dentro de la clave Run.</summary>
        public const string ValueName = "TobonMouse";

        /// <summary>Argumento con el que se arranca en la bandeja, sin abrir la ventana.</summary>
        public const string TrayArgument = "-tray";

        /// <summary>Ruta del ejecutable en curso.</summary>
        public static string ExecutablePath =>
            Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;

        /// <summary>Indica si TobonMouse esta registrado para arrancar con Windows.</summary>
        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    return key?.GetValue(ValueName) != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Registra o quita TobonMouse del arranque de Windows.</summary>
        public static void Set(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (key == null)
                {
                    throw new InvalidOperationException("No se pudo abrir la clave de arranque del usuario.");
                }

                if (enabled)
                {
                    string path = ExecutablePath;

                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        throw new FileNotFoundException("No se encontro el ejecutable de TobonMouse.", path);
                    }

                    key.SetValue(ValueName, "\"" + path + "\" " + TrayArgument);
                }
                else
                {
                    key.DeleteValue(ValueName, false);
                }
            }
        }
    }
}
