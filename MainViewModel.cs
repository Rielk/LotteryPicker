using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LotteryPicker;
internal class MainViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private (int Start, int End) Range = (1, 101);

	private readonly int?[] lotteryNumbers = [null, null, null, null, null];
	private readonly int?[] rollingNumbers = [null, null, null, null, null];
	public int? Number1 { get { return GetNumber(0); } }
	public int? Number2 { get { return GetNumber(1); } }
	public int? Number3 { get { return GetNumber(2); } }
	public int? Number4 { get { return GetNumber(3); } }
	public int? Number5 { get { return GetNumber(4); } }

	private bool isRolling = false;
	public bool IsRolling { get => isRolling; set => SetProperty(ref isRolling, value, onChanged: StartRandomDisplay); }

	private int currentlyRolling = 0;

	private static readonly IList<string> propertyNames = new ReadOnlyCollection<string>([
		nameof(Number1),
		nameof(Number2),
		nameof(Number3),
		nameof(Number4),
		nameof(Number5),
		]);
	private const int FullRollDelay = 1000;
	private const int QuickSpinDelay = 50;

	public int? GetNumber(int index)
	{
		return lotteryNumbers[index] ?? rollingNumbers[index];
	}

	public async void RollNumbers()
	{
		IsRolling = true;
		ClearNumbers();

		var r = new Random();
		for (int i = 0; i < lotteryNumbers.Length; i++)
		{
			var delay = Task.Delay(FullRollDelay);
			int? pick = null;
			while (pick == null || lotteryNumbers.Contains(pick))
				pick = r.Next(Range.Start, Range.End);
			await delay;
			lotteryNumbers[i] = pick;
			currentlyRolling++;
			OnPropertyChanged(propertyNames[i]);
		}

		IsRolling = false;
	}

	private async void StartRandomDisplay()
	{
		var r = new Random();
		while (IsRolling)
		{
			var delay = Task.Delay(QuickSpinDelay);
			for (int i = currentlyRolling; i < rollingNumbers.Length; i++)
			{
				rollingNumbers[i] = r.Next(Range.Start, Range.End);
				OnPropertyChanged(propertyNames[i]);
			}
			await delay;
		}
	}

	private void ClearNumbers()
	{
		for (int i = 0; i < lotteryNumbers.Length; i++)
			lotteryNumbers[i] = null;
		currentlyRolling = 0;
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
