using System;
using System.Windows;
using System.Windows.Threading;

namespace FitnessClub
{
    /// <summary>Точка входа приложения «Фитнес-клуб».</summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Store.Initialize();

            DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                "Произошла непредвиденная ошибка:\n" + e.Exception.Message,
                "Фитнес-клуб",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
