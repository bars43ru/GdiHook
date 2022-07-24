using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace GdiHook
{
    public partial class App : Application
    {
        private ServiceProvider _ServiceProvider { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ServiceCollection serviceCollection = new();
            _ = serviceCollection
                .AddTransient<View.MainWindow>()
                .AddTransient<ViewModel.MainWindowViewModel>()
                .AddSingleton<Services.NktSpyMgr>()
                .AddSingleton<Services.HookСontrols>();

            _ServiceProvider = serviceCollection.BuildServiceProvider();

            View.MainWindow view = _ServiceProvider.GetRequiredService<View.MainWindow>();
            view.DataContext = _ServiceProvider.GetRequiredService<ViewModel.MainWindowViewModel>();
            view.Show();
        }
    }
}
