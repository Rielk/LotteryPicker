using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LotteryPicker;
internal class MainViewModel : INotifyPropertyChanged
{
	private ReadOnlyCollection<int?> numbers;
	public ReadOnlyCollection<int?> Numbers
	{
		get => numbers;
		private set => SetProperty(ref numbers, value);
	}

	private int? bonusNumber;
	public int? BonusNumber
	{
		get => bonusNumber;
		private set => SetProperty(ref bonusNumber, value);
	}

	private int Bottom { get; }
	private int Top { get; }
	private int Count { get; }


	private bool isRolling = false;
	public bool IsRolling
	{
		get => isRolling;
		private set => SetProperty(ref isRolling, value);
	}

	private bool isSpinningNumbers = false;
	public bool IsSpinningNumbers
	{
		get => isSpinningNumbers;
		private set => SetProperty(ref isSpinningNumbers, value);
	}

	private bool isSpinningBonus = false;
	public bool IsSpinningBonus
	{
		get => isSpinningBonus;
		private set => SetProperty(ref isSpinningBonus, value);
	}

	private bool quickRoll = false;
	public bool QuickRoll { get => quickRoll; set => SetProperty(ref quickRoll, value); }

	private bool showHistory = false;
	public bool ShowHistory { get => showHistory; set => SetProperty(ref showHistory, value); }

	public ObservableCollection<HistoryItem> History { get; } = [];

	//Hardcoded defaults
	private const int Bottom_Default = 1;
	private const int Top_Default = 60;
	private const int Count_Default = 5;
	private const int FullRollDelay = 1000;
	private const int QuickSpinDelay = 50;

	//Empty Costructor for Resource DataBinding
	public MainViewModel() : this(Bottom_Default, Top_Default, Count_Default)
	{ }

	public MainViewModel(int bottom, int top, int count)
	{
		if (top <= bottom)
			throw new ArgumentException("Upper Limit of LotteryNumbers must be greater than the Lower Limit");
		if (top - bottom < count + 1)
			throw new ArgumentException($"Insuffient values between Limits to provide {count} unique numbers");

		Bottom = bottom;
		Top = top;
		Count = count;
		List<int?> list = [];
		for (int i = 0; i < count; i++)
			list.Add(null);
		numbers = new(list);
	}

	public async void RollNumbers()
	{
		if (IsRolling)
			return;
		IsRolling = true;

		if (QuickRoll)
		{
			//Just generate numbers
			Random r = new();
			RegenerateNumbers(r);
			RegenerateBonusNumber(r);
		}
		else
		{
			//Queue the full animation.
			BonusNumber = null;
			StartSpinningNumbers(QuickSpinDelay);
			await Task.Delay(FullRollDelay);
			await StopSpinningNumbers();
			StartSpinningBonus(QuickSpinDelay);
			await Task.Delay(FullRollDelay);
			await StopSpinningBonus();
		}

		lock (this)
		{
			//Check should be unnecessary and should always be true.
			if (Numbers.All(n => n.HasValue) && BonusNumber.HasValue)
				History.Insert(0, new(Numbers.Select(n => n ?? -1), BonusNumber.Value));
		}
		while (History.Count > 10)
			History.RemoveAt(History.Count - 1);

		IsRolling = false;
	}


	private Task SpinNumbersTask = Task.CompletedTask;
	private bool StartSpinningNumbers(int delay)
	{
		if (IsSpinningNumbers)
			return false;
		IsSpinningNumbers = true;

		Random r = new();
		SpinNumbersTask = Task.Run(async () =>
		{
			while (IsSpinningNumbers)
			{
				RegenerateNumbers(r);
				await Task.Delay(delay);
			}
		});
		return true;
	}
	private async Task StopSpinningNumbers()
	{
		IsSpinningNumbers = false;
		await SpinNumbersTask;
	}

	private Task SpinBonusTask = Task.CompletedTask;
	private bool StartSpinningBonus(int delay)
	{
		if (IsSpinningBonus)
			return false;
		IsSpinningBonus = true;

		Random r = new();
		SpinBonusTask = Task.Run(async () =>
			{
				while (IsSpinningBonus)
				{
					RegenerateBonusNumber(r);
					await Task.Delay(delay);
				}
			});
		return true;
	}
	private async Task StopSpinningBonus()
	{
		IsSpinningBonus = false;
		await SpinBonusTask;
	}

	private void RegenerateNumbers(Random r)
	{
		lock (this)
		{
			IEnumerable<int?> numbers = [];
			for (int i = 0; i < Count; i++)
				numbers = numbers.Append(GenerateNumber(r, numbers));
			Numbers = new([.. numbers.Order()]);
		}
	}

	[MemberNotNull(nameof(BonusNumber))]
	private void RegenerateBonusNumber(Random r)
	{
		lock (this)
		{
			//Check to make sure Numbers is generated first.
			if (Numbers.Any(n => n == null))
				RegenerateNumbers(r);
			BonusNumber = GenerateNumber(r, Numbers);
		}
	}

	private int GenerateNumber(Random r, IEnumerable<int?> exludeNumbers)
	{
		//A little brute forcey and could be cleverer for ranges with size ~= count. But adequate for this.
		int? next = null;
		while (next == null || exludeNumbers.Contains(next.Value))
			next = r.Next(Bottom, Top);
		return next.Value;
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

	public event PropertyChangedEventHandler? PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChangedEventHandler? changed = PropertyChanged;
		if (changed == null)
			return;

		changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}
