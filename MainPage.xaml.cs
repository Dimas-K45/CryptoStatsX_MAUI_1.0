using CryptoStatsX_MAUI.Resources.Services;
using CryptoStatsX_MAUI.Resources.Services.SQLite;
using DevExpress.Maui.Core;

namespace CryptoStatsX_MAUI
{
    public partial class MainPage : ContentPage
    {
        SQLiteService SQL = new SQLiteService();
        public MainPage()
        {
            InitializeComponent();
            //GetCryptoTokensMainMenu("TOR");
            //SQL.AddData("Tor", 1);

            GetCryptoTokensMainMenu(SQL.GetData());

        }

        private void GetCryptoTokensMainMenu(string TokenID)
        {
            var dxBorder = new DXBorder
            {
                Margin = new Thickness(0, 6, 0, 0),
                HeightRequest = 70,
                BackgroundColor = Color.FromHex("#282828"),
                CornerRadius = 10
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            var image = new Image
            {
                HeightRequest = 35,
                WidthRequest = 35,
                Source = "https://assets.coingecko.com/coins/images/1/small/bitcoin.png",
                Aspect = Aspect.Fill
            };
            Grid.SetRowSpan(image, 2);
            grid.Children.Add(image);

            var label1 = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = 20,
                TextColor = Color.FromHex("#989898"),
                Text = TokenID,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetColumn(label1, 1);
            grid.Children.Add(label1);

            var label2 = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16,
                TextColor = Colors.White,
                Text = "$70400,95",
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetColumn(label2, 1);
            Grid.SetRow(label2, 1);
            grid.Children.Add(label2);

            var label3 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 15,
                TextColor = Color.FromHex("#24FF00"),
                Text = "+100%",
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetColumn(label3, 2);
            grid.Children.Add(label3);

            var dxBorder2 = new DXBorder
            {
                CornerRadius = 15,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromHex("#141414"),
                Padding = new Thickness(5, 2, 5, 2)
            };
            Grid.SetColumn(dxBorder2, 2);
            Grid.SetRow(dxBorder2, 1);
            grid.Children.Add(dxBorder2);

            var label4 = new Label
            {
                FontSize = 15,
                TextColor = Color.FromHex("#24FF00"),
                Text = "$10000",
                FontAttributes = FontAttributes.Bold
            };
            dxBorder2.Content = label4;

            var label5 = new Label
            {
                Margin = new Thickness(0, 0, 5, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 15,
                TextColor = Colors.White,
                Text = "0,000234568",
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetColumn(label5, 3);
            grid.Children.Add(label5);

            var dxBorder3 = new DXBorder
            {
                Margin = new Thickness(0, 0, 5, 0),
                CornerRadius = 15,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromHex("#141414"),
                Padding = new Thickness(5, 2, 5, 2)
            };
            Grid.SetColumn(dxBorder3, 3);
            Grid.SetRow(dxBorder3, 1);
            grid.Children.Add(dxBorder3);

            var label6 = new Label
            {
                FontSize = 15,
                TextColor = Colors.White,
                Text = "$10000",
                FontAttributes = FontAttributes.Bold
            };
            dxBorder3.Content = label6;

            dxBorder.Content = grid;

            StackMainTokens.Children.Add(dxBorder);
        }




        //async void OnOpenWebButtonClicked(System.Object sender, System.EventArgs e)
        //{
        //    await Browser.OpenAsync("https://www.devexpress.com/maui/");
        //}
    }
}