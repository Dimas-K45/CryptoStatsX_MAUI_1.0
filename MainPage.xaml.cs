using CryptoStatsX_MAUI.Resources;
using CryptoStatsX_MAUI.Resources.Services;
using CryptoStatsX_MAUI.Resources.Services.SQLite;
using DevExpress.Maui.Core;
using DevExpress.Maui.Core.Internal;
using DevExpress.Maui.DataGrid;
using DevExpress.Maui.Editors;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace CryptoStatsX_MAUI
{
    public partial class MainPage : ContentPage
    {
        SQLiteService SQL = new SQLiteService();
        bool isPanning = true;

        int ActiveBagTokensId = 1;
        List<AssetsPortfileList> ListPortfile = [];
        Dictionary<string,object> ActiveInfoTokenTransaction = [];
        public MainPage()
        {
            InitializeComponent();

            //SQL.AddDataToken("solana", 3, 160, 1);
            //SQL.AddBagToken("Test2", "");

            //SQL.DelAll();

            List<string> Tokens = new List<string>();
            foreach (var t in SQL.GetListTokens())
            {
                Tokens.Add(t.TokenID);
            }
            try
            {
                GetCryptoTokensMainMenu(Tokens);
                _ = GetListAllToken();
            }
            catch
            {
                _ = GetPushBox("Ошибка подключения к сети", ImgPushBox.wifi);
            }
            
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

        //добавление карточки токена на главной странице
        private async void GetCryptoTokensMainMenu(List<string> TokenIDs)
        {
            APICoinGecko.Coin[] InfoTokens = await APICoinGecko.GetTokensInfoToIDs(TokenIDs);
            double? TotalAssetsCount = 0;
            StackMainTokens.Children.Clear();
            foreach (var Token in InfoTokens)
            {
                var tokendb = SQL.GetTokenToId(Token.Id);
                TotalAssetsCount += Token.current_price;
                if (tokendb.TokenCount <= 0)
                {
                    continue;
                }
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
                if (label3.Text[0] == '-')
                {
                    label3.TextColor = Color.FromHex("#FF0000");
                }
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
                    Text = "$" + InsertSeparator((((tokendb.AVGPrice / 100) * (((Token.current_price - tokendb.AVGPrice) / Token.current_price) * 100))* tokendb.TokenCount).ToString()),
                    FontAttributes = FontAttributes.Bold
                };
                if (label4.Text[1] == '-')
                {
                    label4.TextColor = Color.FromHex("#FF0000");
                }
                if (label4.Text.Length > 10)
                {
                    label4.FontSize = 12;
                }
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
                if (label6.Text.Length > 10)
                {
                    label6.FontSize = 12;
                }
                dxBorder3.Content = label6;

                dxBorder.Content = grid;

                StackMainTokens.Children.Add(dxBorder);
            }

            TotalAssets.Text = "$" + InsertSeparator(TotalAssetsCount.ToString());
        }

        //метод для загрузки списка всех криптовалют
        private async Task GetListAllToken()
        {
            var dataSource = new ObservableCollection<ListToken>();
            CryptoCurrency[] coins = await APICoinGecko.GetListTokenS();
            
            foreach (var coin in coins)
            {
                dataSource.Add(new ListToken {Id = $"{coin.Id}", Name = $"{coin.Name} ({coin.Symbol.ToUpper()})", Symbol = coin.Symbol, Price = (double)coin.Current_price, Image = coin.Image.ToString() });
            }
            
            DataGridListTokens.ItemsSource = dataSource;
        }

        // клик по добавить транзакцию в меню плюса
        private void TapToAddTransaction(object sender, TappedEventArgs e)
        {
            ListCoinsAndSearch.IsVisible = true;
        }

        //открытие меню плюса
        private async void TapPlus(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;
            if (BorderPlus.IsVisible == true)
            {
                BorderPlus.TranslateTo(PageTransaction.TranslationX, BorderPlus.Height, 300);
                await Task.Delay(300);
                BorderPlus.IsVisible = false;
                img.Source = "plus_no_active.svg";
            }
            else
            {
                BorderPlus.IsVisible = true;
                BorderPlus.TranslationY += BorderPlus.Height;
                BorderPlus.TranslateTo(PageTransaction.TranslationX, 0, 300);
                
                img.Source = "plus_active.svg";
            }
        }

        // поиск коина по списку криптовалют
        private void SearchListCrypto(object sender, EventArgs e)
        {
            try
            {
                string searchText = ((TextEdit)sender).Text;
                DataGridListTokens.FilterString = $"Contains([Name], '{searchText}')";
            }
            catch { }
        }

        //открытие пикера для выбора даты транзакции
        private void ShowPickerDate(object sender, TappedEventArgs e)
        {
            DateEdit dateEdit = sender as DateEdit;
            dateEdit.IsPickerShown = true;
        }
        
        //спайп попаппа транзакций для закрытия
        async void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            var element = (VisualElement)sender;
            
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    // Сохраняем изначальные значения TranslationX и TranslationY
                    isPanning = true;
                    break;

                case GestureStatus.Running:
                    if (isPanning)
                    {
                        // Сдвигаем элементы вместе с движением пальца
                        //element.TranslationX += e.TotalX;
                        element.TranslationY += e.TotalY;
                        PageTransaction.TranslationY += e.TotalY;
                        // Проверяем, сдвинулся ли элемент вниз более чем на 50%
                        if (element.TranslationY > (element.Height * 0.5) && PageTransaction.TranslationY > (PageTransaction.Height * 0.5))
                        {
                            isPanning = false; // Прекращаем обработку перемещения

                            // Скрываем элементы с помощью анимации
                            await Task.WhenAll(
                                element.TranslateTo(element.TranslationX, PageTransaction.Height, 250), // Скрываем элемент
                                PageTransaction.TranslateTo(PageTransaction.TranslationX, PageTransaction.Height, 250) // Скрываем элемент
                            );

                            await Task.Delay(100);

                            // Скрываем элементы с помощью анимации, если они видимы (это важно, чтобы избежать мигания элементов)
                            if (MainPageTransaction.IsVisible)
                            {
                                // Скрываем элементы
                                MainPageTransaction.IsVisible = false;
                            }
                        }
                    }
                    break;

                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    // Если жест завершился до того, как элемент скрылся за экраном,
                    // возвращаем его на исходное место
                    if (isPanning)
                    {
                        element.TranslateTo(element.TranslationX, 0, 250); // Возвращаем элемент
                        PageTransaction.TranslateTo(PageTransaction.TranslationX, 0, 250); // Возвращаем элемент
                    }
                    break;
            }
        }

        // завершение транзакции
        private async void TapSuccesTransaction(object sender, TappedEventArgs e)
        {
            void ClosePage()
            {
                List<string> Tokens = new List<string>();
                foreach (var t in SQL.GetListTokens())
                {
                    Tokens.Add(t.TokenID);
                }
                GetCryptoTokensMainMenu(Tokens);
                MainPageTransaction.IsVisible = false;
            }

            if (CountCoinOrUsd.Value > 0)
            {
                if (PriceTransaction.Value > 0)
                {
                    if (CheckUsdTransaction.IsChecked == true)
                    {
                        if (LabelBuyTransaction.TextColor.ToRgbaHex() == Color.Parse("#05FF00").ToRgbaHex())
                        {
                            string[] info = LabelCountCoinOrUsd.Text.Split(' ');
                            
                            if (info[info.Length - 1] == "USD")
                            {
                                if (SQL.UpDateTokenMinus("tether", Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId))
                                {
                                    SQL.UpDateTokenMinus("tether", Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId);
                                    SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex);
                                    
                                    ClosePage();
                                    await GetPushBox("Успешно куплено!", ImgPushBox.yes);
                                }
                                else
                                {
                                    await GetPushBox("Недостаточно активов USDT", ImgPushBox.error_circle);
                                }
                            }
                            else
                            {
                                if (SQL.UpDateTokenMinus("tether", (Convert.ToDouble(CountCoinOrUsd.Value) * Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), ActiveBagTokensId))
                                {
                                    SQL.UpDateTokenMinus("tether", (Convert.ToDouble(CountCoinOrUsd.Value) * Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), ActiveBagTokensId);
                                    SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ActiveBagTokensId);
                                    
                                    ClosePage();
                                    await GetPushBox("Успешно куплено!", ImgPushBox.yes);
                                }
                                else
                                {
                                    await GetPushBox("Недостаточно активов USDT", ImgPushBox.error_circle);
                                }
                            }
                            
                        }
                        else if (LabelSellTransaction.TextColor.ToArgbHex() == Color.Parse("#FF0000").ToRgbaHex())
                        {
                            string[] info = LabelCountCoinOrUsd.Text.Split(' ');

                            if (info[info.Length - 1] == "USD")
                            {
                                if (SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), (Convert.ToDouble(CountCoinOrUsd.Value) / Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), ActiveBagTokensId))
                                {
                                    SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), (Convert.ToDouble(CountCoinOrUsd.Value) / Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), ActiveBagTokensId);
                                    SQL.AddTransactionSell(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex, true);
                                    
                                    ClosePage();
                                    await GetPushBox("Успешно Продано!", ImgPushBox.yes);
                                }
                                else
                                {
                                    await GetPushBox($"Недостаточно активов {ActiveInfoTokenTransaction["symbol"]}", ImgPushBox.error_circle);
                                }
                            }
                            else
                            {
                                if (SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId) && SQL.CheckExist(ActiveInfoTokenTransaction["id"].ToString()))
                                {
                                    SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId);
                                    SQL.AddTransactionSell(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ActiveBagTokensId, true);
                                    
                                    ClosePage();
                                    await GetPushBox("Успешно Продано!", ImgPushBox.yes);
                                }
                                else
                                {
                                    await GetPushBox($"Недостаточно активов {ActiveInfoTokenTransaction["symbol"]}", ImgPushBox.error_circle);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (LabelBuyTransaction.TextColor.ToRgbaHex() == Color.Parse("#05FF00").ToRgbaHex())
                        {
                            string[] info = LabelCountCoinOrUsd.Text.Split(' ');

                            if (info[info.Length - 1] == "USD")
                            {
                                SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex);

                                ClosePage();
                                await GetPushBox("Успешно куплено!", ImgPushBox.yes);
                            }
                            else
                            {
                                SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ActiveBagTokensId);

                                ClosePage();
                                await GetPushBox("Успешно куплено!", ImgPushBox.yes);
                            }
                        }
                        else if (LabelSellTransaction.TextColor.ToArgbHex() == Color.Parse("#FF0000").ToRgbaHex())
                        {
                            string[] info = LabelCountCoinOrUsd.Text.Split(' ');

                            if (info[info.Length - 1] == "USD")
                            {
                                SQL.AddTransactionSell(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex, false);

                                ClosePage();
                                await GetPushBox("Успешно Продано!", ImgPushBox.yes);
                            }
                            else
                            {
                                SQL.AddTransactionSell(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ActiveBagTokensId, false);

                                ClosePage();
                                await GetPushBox("Успешно Продано!", ImgPushBox.yes);
                            }
                        }
                    }
                }
                else
                {
                    await GetPushBox("Заполните Цену!", ImgPushBox.error_circle);
                }
            }
            else
            {
                await GetPushBox("Заполните Кол-во!", ImgPushBox.error_circle);
            }
        }

        private enum ImgPushBox
        {
            info_circle,
            error_circle,
            yes,
            wifi
        }
        //метод для анимации и вызова уведомления об ошибке
        private async Task GetPushBox(string message, ImgPushBox image)
        {
            PushImage.Source = $"{image}.svg";
            
            if (PushBorder.IsVisible == false)
            {
                Vibration.Default.Vibrate();
                PushBorder.TranslationY += -PushBorder.Height + 20;
                PushMessage.Text = message;
                PushBorder.IsVisible = true;
                await PushBorder.TranslateTo(PushBorder.TranslationX, 0, 250);
                await Task.Delay(3000);
                await PushBorder.TranslateTo(PushBorder.TranslationX, -PushBorder.Height * 2 + 20, 250);
                await Task.Delay(1000);
                PushBorder.IsVisible = false;
            }
        }

        //выбор в транзакции купить или продать
        private void TapBuyAndSellTransaction(object sender, TappedEventArgs e)
        {
            Label lab = sender as Label;
            if (lab != null)
            {
                if (lab.Text == "Купить")
                {
                    lab.TextColor = Color.Parse("#05FF00");
                    BorderBuyTransaction.BorderThickness = 1;
                    TextUsdTransactionCheck.Text = "Вычесть из накоплений USD/USDT";

                    BorderSellTransaction.BorderThickness = 0;
                    LabelSellTransaction.TextColor = Color.Parse("#C4C4C4");
                }
                else if (lab.Text == "Продать")
                {
                    lab.TextColor = Color.Parse("#FF0000");
                    BorderSellTransaction.BorderThickness = 1;
                    TextUsdTransactionCheck.Text = "Добавить в накопления USD/USDT";

                    BorderBuyTransaction.BorderThickness = 0;
                    LabelBuyTransaction.TextColor = Color.Parse("#C4C4C4");
                }
                
            }
        }

        //тап по тексту добавить из запасов usd
        private void UsdTransactionCheck(object sender, TappedEventArgs e)
        {
            if (CheckUsdTransaction.IsChecked == true)
            {
                CheckUsdTransaction.IsChecked = false;
            }
            else
            {
                CheckUsdTransaction.IsChecked = true;
            }
        }

        //тап по стрелкам в добавлении транзакции для конвертации
        private void TapConvertToTransactionCoinOrUsd(object sender, TappedEventArgs e)
        {
            string[] str = LabelCountCoinOrUsd.Text.Split(' ');
            if (str[str.Length -1] == "USD")
            {
                LabelCountCoinOrUsd.Text = str[0] + $" {ActiveInfoTokenTransaction[APICoinGecko.CoinField.Symbol.ToString().ToLower()].ToString().ToUpper()}";
                decimal count = (decimal)(CountCoinOrUsd.Value == null ? 0 : CountCoinOrUsd.Value);
                CountCoinOrUsd.Value = Math.Round(count / Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 8);
            }
            else
            {
                LabelCountCoinOrUsd.Text = str[0] + $" USD";
                decimal count = (decimal)(CountCoinOrUsd.Value == null ? 0 : CountCoinOrUsd.Value);
                CountCoinOrUsd.Value = Math.Round(count * Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 8);
            }
        }

        //тап по коину из списка для перехода к добавлению транзакции
        private async void TapGetAddTransaction(object sender, TappedEventArgs e)
        {
            Label lab = sender as Label;
            if (ActiveInfoTokenTransaction.Count != 0 && lab.AutomationId.ToString() != ActiveInfoTokenTransaction[APICoinGecko.CoinField.Id.ToString().ToLower()].ToString())
            {
                CountCoinOrUsd.Value = 0;
            }
            ActiveInfoTokenTransaction = await APICoinGecko.GetInfoTokenToID(lab.AutomationId.ToString(), "usd");
            
            ListCoinsAndSearch.IsVisible = false;
            MainPageTransaction.IsVisible = true;
            swipeLabel.TranslationY += swipeLabel.Height;
            PageTransaction.TranslationY += swipeLabel.Height;
            await Task.WhenAll(swipeLabel.TranslateTo(swipeLabel.TranslationX, 0, 250),
            PageTransaction.TranslateTo(PageTransaction.TranslationX, 0, 250)
            );

            DateTimeTransaction.Date = DateTime.Now;
            ListPortfile = SQL.GetListBagTokens();
            List<string> NamePortfiles = new List<string>();
            foreach (var list in ListPortfile)
            {
                NamePortfiles.Add(list.Name);
            }
            ComboBoxPortfileTransaction.ItemsSource = NamePortfiles;
            ComboBoxPortfileTransaction.SelectedIndex = ActiveBagTokensId - 1;

            swipeLabel.Text = ActiveInfoTokenTransaction[APICoinGecko.CoinField.Name.ToString().ToLower()].ToString();
            LabelCountCoinOrUsd.Text = $"Всего {ActiveInfoTokenTransaction[APICoinGecko.CoinField.Symbol.ToString().ToLower()].ToString().ToUpper()}";
            PriceTransaction.Value = Math.Round(Convert.ToDecimal(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ",")), 8);
        }

        private void TapBackIsListCoinsAndSearch(object sender, TappedEventArgs e)
        {
            ListCoinsAndSearch.IsVisible = false;
        }

        






        //async void OnOpenWebButtonClicked(System.Object sender, System.EventArgs e)
        //{
        //    await Browser.OpenAsync("https://www.devexpress.com/maui/");
        //}
    }
}