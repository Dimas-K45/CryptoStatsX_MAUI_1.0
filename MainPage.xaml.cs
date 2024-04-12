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

            //SQL.AddDataToken("solana", 3, 160);
            
            //SQL.DelAll();

            List<string> Tokens = new List<string>();
            foreach (var t in SQL.GetListTokens())
            {
                Tokens.Add(t.TokenID);
            }
            GetCryptoTokensMainMenu(Tokens);

        }
        static string InsertSeparator(string input)
        {
            if (input.Length <= 3)
                return input;
            else if (Convert.ToDouble(input.Replace(".", ",")) < 99999 && Convert.ToDouble(input.Replace(".", ",")) > 999)
                return Math.Round(Convert.ToDouble(input.Replace(".", ",")), 0).ToString();

            if (input.ToLower().Contains('e'))
            {
                string[] numConvert = input.ToLower().Split('e');
                string[] span = input.Split('-');
                string resultSpan = (Convert.ToDouble(numConvert[0]) / Math.Pow(10, Convert.ToDouble(span[span.Length - 1]))).ToString("G");
                //decimal number = decimal.Parse(resultSpan.Replace(".", ","));
                string result = resultSpan;
                return result;
            }
            else
            {
                decimal number = decimal.Parse(input.Replace(".", ","));
                string result = number.ToString("N");
                return result;
            }
        }
        private async void GetCryptoTokensMainMenu(List<string> TokenIDs)
        {
            APICoinGecko.Coin[] InfoTokens = await APICoinGecko.GetTokensInfoToIDs(TokenIDs);
            double? TotalAssetsCount = 0;
            double? TotalPrecettageCount = 0;
            foreach (var Token in InfoTokens)
            {
                var tokendb = SQL.GetTokenToId(Token.Id);
                TotalAssetsCount += Token.current_price;
                
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
                    Source = Token.Image,
                    Aspect = Aspect.Fill
                };
                Grid.SetRowSpan(image, 2);
                grid.Children.Add(image);

                var label1 = new Label
                {
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 20,
                    TextColor = Color.FromHex("#989898"),
                    Text = Token.Symbol.ToUpper(),
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetColumn(label1, 1);
                grid.Children.Add(label1);

                var label2 = new Label
                {
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 16,
                    TextColor = Colors.White,
                    Text = "$" + InsertSeparator(Token.current_price.ToString()),
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
                    Text = InsertSeparator((((Token.current_price - tokendb.AVGPrice) / Token.current_price)*100).ToString()) +"%",
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
                    Text = "$" + InsertSeparator(((tokendb.AVGPrice / 100) * (((Token.current_price - tokendb.AVGPrice) / Token.current_price) * 100)).ToString()),
                    FontAttributes = FontAttributes.Bold
                };
                dxBorder2.Content = label4;

                var label5 = new Label
                {
                    Margin = new Thickness(0, 0, 5, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 15,
                    TextColor = Colors.White,
                    Text = InsertSeparator(tokendb.TokenCount.ToString()),
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetColumn(label5, 3);
                grid.Children.Add(label5);

                var dxBorder3 = new DXBorder
                {
                    Margin = new Thickness(0, 0, 5, 0),
                    CornerRadius = 15,
                    HorizontalOptions = LayoutOptions.Center,
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
                    Text = "$" + InsertSeparator((tokendb.TokenCount * Token.current_price).ToString()),
                    FontAttributes = FontAttributes.Bold
                };
                dxBorder3.Content = label6;

                dxBorder.Content = grid;

                StackMainTokens.Children.Add(dxBorder);
            }

            TotalAssets.Text = "$" + InsertSeparator(TotalAssetsCount.ToString());
        }

        private void TapToAddTransaction(object sender, TappedEventArgs e)
        {

        }

        private void TapPlus(object sender, TappedEventArgs e)
        {
            if (BorderPlus.IsVisible == true)
            {
                BorderPlus.IsVisible = false;
            }
            else
            {
                BorderPlus.IsVisible = true;
            }
        }




        //async void OnOpenWebButtonClicked(System.Object sender, System.EventArgs e)
        //{
        //    await Browser.OpenAsync("https://www.devexpress.com/maui/");
        //}
    }
}