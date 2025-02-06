using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LotteryPicker;
internal class MainViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private readonly int?[] lotteryNumbers = [null, null, null, null, null];
	public int? Number1 { get { return GetNumber(0); } }
	public int? Number2 { get { return GetNumber(1); } }
	public int? Number3 { get { return GetNumber(2); } }
	public int? Number4 { get { return GetNumber(3); } }
	public int? Number5 { get { return GetNumber(4); } }

	private bool isRolling = false;
	public bool IsRolling { get => isRolling; set => SetProperty(ref isRolling, value); }


	private static readonly IList<string> propertyNames = new ReadOnlyCollection<string>([
		nameof(Number1),
		nameof(Number2),
		nameof(Number3),
		nameof(Number4),
		nameof(Number5),
		]);

	public int? GetNumber(int index)
	{
		return lotteryNumbers[index];
	}

	public async void RollNumbers()
	{
		ClearNumbers();
		IsRolling = true;

		var r = new Random();
		const int bottom = 0, top = 100;
		for (int i = 0; i < lotteryNumbers.Length; i++)
		{
			var delay = Task.Delay(1000);
			int? pick = null;
			while (pick == null || lotteryNumbers.Contains(pick))
				pick = r.Next(bottom, top);
			await delay;
			lotteryNumbers[i] = pick;
			OnPropertyChanged(propertyNames[i]);
		}

		IsRolling = false;
	}

	private void ClearNumbers()
	{
		for (int i = 0; i < lotteryNumbers.Length; i++)
			lotteryNumbers[i] = null;
		foreach (string name in propertyNames)
			OnPropertyChanged(name);
	}


	#region INotifyPropertyChanged
	protected bool SetProperty<T>(ref T backingStore, T value,
		[CallerMemberName] string propertyName = "",
		Action? onChanging = null,
		Action? onChanged = null)
	{
		if (EqualityComparer<T>.Default.Equals(backingStore, value))
			return false;

		onChanging?.Invoke();
		backingStore = value;
		onChanged?.Invoke();
		OnPropertyChanged(propertyName);
		return true;
	}
	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChangedEventHandler? changed = PropertyChanged;
		if (changed == null)
			return;

		changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}
