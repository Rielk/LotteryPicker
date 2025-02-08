using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LotteryPicker;
internal class MainViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;


	private ReadOnlyObservableCollection<int?> numbers;
	public ReadOnlyObservableCollection<int?> Numbers => numbers;
	private ObservableCollection<int?> NumbersBase
	{
		set => SetProperty(ref numbers, new(value), nameof(Numbers));
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
		set => SetProperty(ref isRolling, value);
	}

	public bool IsSpinningNumbers { get => SpinNumbersTS != null; }
	private CancellationTokenSource? SpinNumbersTS;
	private Task SpinNumbersTask = Task.CompletedTask;

	public bool IsSpinningBonus { get => SpinBonusTS != null; }
	private CancellationTokenSource? SpinBonusTS;
	private Task SpinBonusTask = Task.CompletedTask;

	private const int FullRollDelay = 1000;
	private const int QuickSpinDelay = 50;

	public int? GetNumber(int index)
	{
		if (index > 4)
			return null;
		return Numbers[index];
	}

	public MainViewModel()
	{
		int bottom = 1, top = 60, count = 5;
		if (top <= bottom)
			throw new ArgumentException("Upper Limit of LotteryNumbers must be greater than the Lower Limit");
		if (top - bottom < count + 1)
			throw new ArgumentException($"Insuffient values between Limits to provide {count} unique numbers");

		Bottom = bottom;
		Top = top;
		Count = count;
		numbers = new(new(new int?[5]));
	}

	public async void RollNumbers()
	{
		if (IsRolling)
			return;
		IsRolling = true;
		_ = StartSpinningNumbers(QuickSpinDelay);
		await Task.Delay(FullRollDelay);
		await StopSpinningNumbers();
		_ = StartSpinningBonus(QuickSpinDelay);
		await Task.Delay(FullRollDelay);
		await StopSpinningBonus();
		IsRolling = false;
	}

	private async Task StartSpinningNumbers(int delay)
	{
		if (IsSpinningNumbers)
		{
			await SpinNumbersTask;
			return;
		}

		SpinNumbersTS = new CancellationTokenSource();
		CancellationToken ct = SpinNumbersTS.Token;

		await StopSpinningBonus();

		Random r = new();
		SpinNumbersTask = Task.Run(async () =>
		{
			while (!ct.IsCancellationRequested)
			{
				RegenerateNumbers(r);
				await Task.Delay(delay);
			}
		}, ct);
		await SpinNumbersTask;
		SpinNumbersTS.Dispose();
		SpinNumbersTS = null;

	}
	private async Task StopSpinningNumbers()
	{
		SpinNumbersTS?.Cancel();
		await SpinNumbersTask;
	}

	private async Task StartSpinningBonus(int delay)
	{
		if (IsSpinningBonus)
		{
			await SpinBonusTask;
			return;
		}

		SpinBonusTS = new CancellationTokenSource();
		CancellationToken ct = SpinBonusTS.Token;

		await StopSpinningNumbers();

		Random r = new();
		SpinBonusTask = Task.Run(async () =>
		{
			while (!ct.IsCancellationRequested)
			{
				RegenerateBonusNumber(r);
				await Task.Delay(delay);
			}
		}, ct);
		await SpinBonusTask;
		SpinBonusTS.Dispose();
		SpinBonusTS = null;

	}
	private async Task StopSpinningBonus()
	{
		SpinBonusTS?.Cancel();
		await SpinBonusTask;
	}

	private void RegenerateNumbers(Random r)
	{
		BonusNumber = null;
		int?[] numbers = new int?[Count];
		for (int i = 0; i < Count; i++)
			numbers[i] = GenerateNumber(r, numbers);

		NumbersBase = new(numbers.Order());
	}

	private void RegenerateBonusNumber(Random r)
	{
		if (Numbers.Any(n => n == null))
			RegenerateNumbers(r);
		BonusNumber = GenerateNumber(r, Numbers);
	}

	private int GenerateNumber(Random r, IEnumerable<int?> numbers)
	{
		//A little brute forcey and could be cleverer for ranges with size ~= count. But adequate for this.
		int? next = null;
		while (next == null || numbers.Contains(next.Value))
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
	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChangedEventHandler? changed = PropertyChanged;
		if (changed == null)
			return;

		changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}
