using System.Collections.ObjectModel;

namespace LotteryPicker;

internal record struct HistoryItem(ReadOnlyCollection<int> Numbers, int BonusNumber)
{
	public HistoryItem(IEnumerable<int> numbers, int bonusNumber) : this(new ReadOnlyCollection<int>(numbers.ToArray()), bonusNumber)
	{ }
}
