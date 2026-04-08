using StockAnalyzer.Core;
using StockAnalyzer.Core.Domain;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace StockAnalyzer.Windows;

public partial class MainWindow : Window
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
    private static readonly HttpClient _httpClient = new();
    private Stopwatch stopwatch = new Stopwatch();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        BeforeLoadingStockData();

        /* 
         * 
         * HTTPClient call

        var response = await _httpClient.GetAsync($"{API_URL}/{StockIdentifier.Text}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Simulate that the web call takes a very long time
            await Task.Delay(10000);

            var data = JsonSerializer.Deserialize<IEnumerable<StockPrice>>(content, JsonSerializerOptions.Web);

            Stocks.ItemsSource = data;
        }    
        
        */

        var store = new DataStore();

        var data = await store.GetStockPrices(StockIdentifier.Text);

        Stocks.ItemsSource = data;

        AfterLoadingStockData();
    }








    private void BeforeLoadingStockData()
    {
        stopwatch.Restart();
        StockProgress.Visibility = Visibility.Visible;
        StockProgress.IsIndeterminate = true;
    }

    private void AfterLoadingStockData()
    {
        StocksStatus.Text = $"Loaded stocks for {StockIdentifier.Text} in {stopwatch.ElapsedMilliseconds}ms";
        StockProgress.Visibility = Visibility.Hidden;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });

        e.Handled = true;
    }

    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}