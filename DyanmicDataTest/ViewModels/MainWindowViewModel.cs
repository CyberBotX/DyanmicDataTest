using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace DyanmicDataTest.ViewModels
{
	public class MainWindowViewModel : ReactiveValidationObject, IActivatableViewModel
	{
		public ViewModelActivator Activator { get; } = new();

		ReadOnlyObservableCollection<CheckedItemViewModel> checkedItems = null!;
		public ReadOnlyObservableCollection<CheckedItemViewModel> CheckedItems => this.checkedItems;

		readonly SourceCache<CheckedItemViewModel, string> items = new(k => k.Key);
		readonly SourceCache<CheckedItemViewModel, string> nonUIItems = new(k => k.Key);

		ReadOnlyObservableCollection<MessageViewModel> bindMessages = null!;
		public ReadOnlyObservableCollection<MessageViewModel> BindMessages => this.bindMessages;

		readonly SourceList<MessageViewModel> bindMessagesList = new();

		ReadOnlyObservableCollection<MessageViewModel> nonBindMessages = null!;
		public ReadOnlyObservableCollection<MessageViewModel> NonBindMessages => this.nonBindMessages;

		readonly SourceList<MessageViewModel> nonBindMessagesList = new();

		public ReactiveCommand<Unit, Unit> CheckAll { get; private set; } = null!;

		public ReactiveCommand<Unit, Unit> UncheckAll { get; private set; } = null!;

		public ReactiveCommand<Unit, Unit> DummyButton { get; private set; } = null!;

		public ReactiveCommand<Unit, IEnumerable<CheckedItemViewModel>> PopulateItems { get; private set; } = null!;

		static void AddMessages<T>(string header, SourceList<MessageViewModel> messageList, IEnumerable<T> items, Action? additionalAction = null)
		{
			var messagesToAdd = new List<MessageViewModel>
			{
				new()
				{
					Message = header,
					FontWeight = FontWeights.Bold
				}
			};
			foreach (var item in items)
				messagesToAdd.Add(new()
				{
					Message = $"-- {item}"
				});
			messageList.Edit(innerList => innerList.AddRange(messagesToAdd));
			additionalAction?.Invoke();
		}
		static void AddMessage<T>(string header, SourceList<MessageViewModel> messageList, T value) =>
			messageList.Edit(innerList => innerList.AddRange(new MessageViewModel[]
			{
				new()
				{
					Message = header,
					FontWeight = FontWeights.Bold
				},
				new()
				{
					Message = $"-- {value}"
				}
			}));

		public MainWindowViewModel() => this.WhenActivated(disposableRegistration =>
		{
			_ = this.bindMessagesList.Connect().AutoRefreshOnObservable(x => Observable.Return(x)).
				ObserveOn(RxApp.MainThreadScheduler).Bind(out this.bindMessages).Subscribe();
			_ = this.nonBindMessagesList.Connect().AutoRefreshOnObservable(x => Observable.Return(x)).
				ObserveOn(RxApp.MainThreadScheduler).Bind(out this.nonBindMessages).Subscribe();

			var items = this.items.Connect().Publish();
			_ = items.AutoRefresh().Do(changeSet => AddMessages("AutoRefresh (unfiltered):", this.bindMessagesList, changeSet, () =>
			{
				var refreshes = new List<CheckedItemViewModel>();
				foreach (var change in changeSet)
					if (change.Reason == ChangeReason.Refresh)
						refreshes.Add(change.Current);
				// The following is not *actually* the same operation, as it'll cause Update changes, not Refresh changes...
				if (refreshes.Count != 0)
					this.nonUIItems.Edit(innerList => innerList.AddOrUpdate(refreshes));
			})).ObserveOn(RxApp.MainThreadScheduler).Bind(out this.checkedItems).Subscribe();
			var anyChecked = items.
				AutoRefresh(i => i.Checked).Do(changeSet => AddMessages("AutoRefresh (filtered by i.Checked):", this.bindMessagesList, changeSet)).
				Filter(i => i.Checked).Do(changeSet => AddMessages("Filter (filtered by i.Checked):", this.bindMessagesList, changeSet)).
				ToCollection().Do(collection => AddMessages("ToCollection (after filtering):", this.bindMessagesList, collection)).
				Select(c => c.Count != 0).Do(value => AddMessage("Select (collection Count != 0):", this.bindMessagesList, value)).
				StartWith(false);
			_ = items.Connect().DisposeWith(disposableRegistration);

			_ = this.nonUIItems.Connect().
				AutoRefresh(i => i.Checked).Do(changeSet => AddMessages("AutoRefresh (filtered by i.Checked):", this.nonBindMessagesList, changeSet)).
				Filter(i => i.Checked).Do(changeSet => AddMessages("Filter (filtered by i.Checked):", this.nonBindMessagesList, changeSet)).
				ToCollection().Do(collection => AddMessages("ToCollection (after filtering):", this.nonBindMessagesList, collection)).
				Select(c => c.Count != 0).Do(value => AddMessage("Select (collection Count != 0):", this.nonBindMessagesList, value)).
				StartWith(false).Subscribe().DisposeWith(disposableRegistration);

			void SetChecked(bool @checked)
			{
				void Setter(ISourceUpdater<CheckedItemViewModel, string> innerList) =>
					innerList.AddOrUpdate(innerList.Items.Where(i => i.Checked != @checked).Select(i => new CheckedItemViewModel
					{
						Key = i.Key,
						Checked = @checked
					}));
				this.items.Edit(Setter);
				this.nonUIItems.Edit(Setter);
			}

			this.CheckAll = ReactiveCommand.Create(() => SetChecked(true)).DisposeWith(disposableRegistration);
			this.UncheckAll = ReactiveCommand.Create(() => SetChecked(false)).DisposeWith(disposableRegistration);

			this.DummyButton = ReactiveCommand.Create(() => { }, this.IsValid()).DisposeWith(disposableRegistration);

			this.PopulateItems = ReactiveCommand.Create(PopulateItemsImpl).DisposeWith(disposableRegistration);

			void PopulateCaches(IEnumerable<CheckedItemViewModel> items)
			{
				this.items.Edit(innerList =>
				{
					innerList.Clear();
					innerList.AddOrUpdate(items);
				});
				this.nonUIItems.Edit(innerList =>
				{
					innerList.Clear();
					innerList.AddOrUpdate(items);
				});
			}

			// I know doing this here is not recommended...
			_ = this.PopulateItems.Execute().Subscribe(PopulateCaches);

			_ = this.ValidationRule(viewModel => viewModel.CheckedItems, anyChecked, "At least one item must be checked!");
		});

		static IEnumerable<CheckedItemViewModel> PopulateItemsImpl() => Enumerable.Range(1, 10).Select(i => new CheckedItemViewModel
		{
			Key = $"Item {i}"
		});
	}
}
