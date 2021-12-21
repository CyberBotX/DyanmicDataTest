using System.Reactive.Disposables;
using DyanmicDataTest.ViewModels;
using ReactiveUI;

namespace DyanmicDataTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			this.InitializeComponent();

			_ = this.WhenActivated(disposableRegistration =>
			{
				this.ViewModel = new MainWindowViewModel();

				_ = this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext).DisposeWith(disposableRegistration);

				_ = this.OneWayBind(this.ViewModel, vm => vm.CheckedItems, view => view.checkboxes.ItemsSource).DisposeWith(disposableRegistration);

				_ = this.BindCommand(this.ViewModel, vm => vm.CheckAll, view => view.checkAll).DisposeWith(disposableRegistration);
				_ = this.BindCommand(this.ViewModel, vm => vm.UncheckAll, view => view.uncheckAll).DisposeWith(disposableRegistration);

				_ = this.OneWayBind(this.ViewModel, vm => vm.BindMessages, view => view.boundMessages.ItemsSource).
					DisposeWith(disposableRegistration);
				_ = this.OneWayBind(this.ViewModel, vm => vm.NonBindMessages, view => view.notBoundMessages.ItemsSource).
					DisposeWith(disposableRegistration);
			});
		}
	}
}
