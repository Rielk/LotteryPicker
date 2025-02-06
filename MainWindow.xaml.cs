using System.Windows;
using System.Windows.Controls;

namespace LotteryPicker;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		var ds = ((Button)sender).DataContext as MainViewModel;
		ds?.RollNumbers();
	}
}