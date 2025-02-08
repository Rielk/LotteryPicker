using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LotteryPicker;
internal class MainViewModel : INotifyPropertyChanged
{
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

	private const int FullRollDelay = 1000;
	private const int QuickSpinDelay = 50;

	public int? GetNumber(int index)
	{
		if (index > 4)
			return null;
		return Numbers[index];
	}

	public MainViewModel() : this(1, 60, 5)
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
		numbers = new(new(new int?[5]));
	}

	public async void RollNumbers()
	{
		//Queue the full animation.
		if (IsRolling)
			return;
		IsRolling = true;
		StartSpinningNumbers(QuickSpinDelay);
		await Task.Delay(FullRollDelay);
		await StopSpinningNumbers();
		StartSpinningBonus(QuickSpinDelay);
		await Task.Delay(FullRollDelay);
		await StopSpinningBonus();
		IsRolling = false;
	}

	private CancellationTokenSource? SpinNumbersTS;
	private Task SpinNumbersTask = Task.CompletedTask;
	private void StartSpinningNumbers(int delay)
	{
		if (!SpinNumbersTask.IsCompleted)
			return;

		SpinNumbersTS = new CancellationTokenSource();
		CancellationToken ct = SpinNumbersTS.Token;

		Random r = new();
		//Until the cancellation token is recieved, regenerate then wait.
		SpinNumbersTask = Task.Run(async () =>
		{
			while (!ct.IsCancellationRequested)
			{
				RegenerateNumbers(r);
				await Task.Delay(delay);
			}
		}, ct);
	}
	private async Task StopSpinningNumbers()
	{
		//Cancel the task, wait for it to finish, then clean up.
		SpinNumbersTS?.Cancel();
		await SpinNumbersTask;
		SpinNumbersTS?.Dispose();
		SpinNumbersTS = null;
	}

	private CancellationTokenSource? SpinBonusTS;
	private Task SpinBonusTask = Task.CompletedTask;
	private void StartSpinningBonus(int delay)
	{
		if (!SpinBonusTask.IsCompleted)
			return;

		SpinBonusTS = new CancellationTokenSource();
		CancellationToken ct = SpinBonusTS.Token;

		Random r = new();
		//Until the cancellation token is recieved, regenerate then wait.
		SpinBonusTask = Task.Run(async () =>
		{
			while (!ct.IsCancellationRequested)
			{
				RegenerateBonusNumber(r);
				await Task.Delay(delay);
			}
		}, ct);
	}
	private async Task StopSpinningBonus()
	{
		//Cancel the task, wait for it to finish, then clean up
		SpinBonusTS?.Cancel();
		await SpinBonusTask;
		SpinBonusTS?.Dispose();
		SpinBonusTS = null;
	}

	private IEnumerable<int?> RegenerateNumbers(Random r)
	{
		//Populate then sort an array into an ObservableCollection
		BonusNumber = null;
		int?[] numbers = new int?[Count];
		for (int i = 0; i < Count; i++)
			numbers[i] = GenerateNumber(r, numbers);
		//Only return the enumerator because nothing should modify outside of this.
		return NumbersBase = new(numbers.Order());
	}

	private int? RegenerateBonusNumber(Random r)
	{
		//Check to make sure Numbers is generated first.
		if (Numbers.Any(n => n == null))
			RegenerateNumbers(r);
		return BonusNumber = GenerateNumber(r, Numbers);
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
