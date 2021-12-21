# DyanmicDataTest
A test showing a problem I'm having with [DynamicData](https://github.com/reactivemarbles/DynamicData) and [ReactiveUI](https://github.com/reactiveui/ReactiveUI).

For some reason, if I attempt to use a `SourceCache` in the UI (exposed via `ReadOnlyObservableCollection`) and also try to get filtered notifications for ReactiveUI validation, things don't work until the cache has had at least one Update changeset.

The application here creates 2 copies of a list of 10 items, each item consisting of a string "key" and a boolean to say if it was checked or not. It displays one of the lists in the UI as a series of checkboxes.

The UI also shows 2 sets of lists of notification messages.

The left set of messages is for the observables attached to the copy of the list that is being shown in the UI, as it has been bound to the `ReadOnlyObservableCollection` for this. The binding observable has been subscribed to, which I refer to as the bind observable. The other observable is meant to be used for ReactiveUI validation, where I am trying to find when any item in the list is checked, and is not subscribed to as I use the observable as a validation rule, which I refer to as the validation observable.

The right set of messages is for the observable attached to the copy of the list that is not being shown in the UI. It has no binding on it, and the observable is only subscribed to so I can get notifications on it.

The messages in both lists are generated after certain points in the observable chain using the `Do` operator.

The best way to see the problem is the following:
1. Check any box or boxes (notice how the button that should be enabled does not get enabled, nor is there an observable notification emitted by the validation observable for this action).
2. Optionally uncheck any of those boxes.
3. Click the "Check All" button (notice how the button is now enabled as expected, because the validation passes now due to the validation observable now emitting values).
4. Click the "Uncheck All" button (since the validation observable seems to now be emitting values, the button becomes disabled as validation no longer passes).
5. Check any box or boxes again (this time, unlike step 1, the button becomes enabled, as the validation observable is emitting values still).

Other things to note:
* If before step 3, either no boxes are checked and "Uncheck All" is clicked or all boxes are checked and "Check All" is clicked, the validation observable will still not emit values, as those buttons will not perform any updates on the list.
* From what I have noticed, clicking the checkbox in the UI causes a Refresh changeset to be emitted for the change. Because I cannot find a way to make the same happen on the list that isn't bound to the UI, I made the code perform an update there, which is not the same operation.
