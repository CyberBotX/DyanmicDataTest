using System.Windows;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DyanmicDataTest.ViewModels
{
	public class MessageViewModel : ReactiveObject
	{
		[Reactive]
		public string Message { get; set; } = null!;

		[Reactive]
		public FontWeight FontWeight { get; set; } = FontWeights.Normal;
	}
}
