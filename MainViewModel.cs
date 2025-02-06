using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LotteryPicker;
internal class MainViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private readonly int?[] lotteryNumbers = [null, null, null, null, null];
	public int? Number1 { get { return lotteryNumbers[0]; } }
	public int? Number2 { get { return lotteryNumbers[1]; } }
	public int? Number3 { get { return lotteryNumbers[2]; } }
	public int? Number4 { get { return lotteryNumbers[3]; } }
	public int? Number5 { get { return lotteryNumbers[4]; } }

	private static readonly IList<string> propertyNames = new ReadOnlyCollection<string>([
		nameof(Number1),
		nameof(Number2),
		nameof(Number3),
		nameof(Number4),
		nameof(Number5),
		]);

	public void RollNumbers()
	{
		ClearNumbers();

		var r = new Random();
		for (int i = 0; i < lotteryNumbers.Length; i++)
		{
			lotteryNumbers[i] = r.Next(0, 100); ;
			OnPropertyChanged(propertyNames[i]);
		}
	}

	private void ClearNumbers()
	{
		for (int i = 0; i < lotteryNumbers.Length; i++)
			lotteryNumbers[i] = null;
		foreach (string name in propertyNames)
			OnPropertyChanged(name);
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChangedEventHandler? changed = PropertyChanged;
		if (changed == null)
			return;

		changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
