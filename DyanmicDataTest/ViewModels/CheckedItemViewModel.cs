using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DyanmicDataTest.ViewModels
{
	public class CheckedItemViewModel : ReactiveObject
	{
		[Reactive]
		public string Key { get; set; } = null!;

		[Reactive]
		public bool Checked { get; set; }

		public override string ToString() => $"Key = {this.Key} | Checked = {this.Checked}";
	}
}
