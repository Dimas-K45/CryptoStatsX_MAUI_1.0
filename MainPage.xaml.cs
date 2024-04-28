using CryptoStatsX_MAUI.Resources;
using CryptoStatsX_MAUI.Resources.Services;
using CryptoStatsX_MAUI.Resources.Services.SQLite;
using DevExpress.Maui.Core;
using DevExpress.Maui.Core.Internal;
using DevExpress.Maui.DataGrid;
using DevExpress.Maui.Editors;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using RestSharp;
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

        ObservableCollection<ListToken> dataSource = new ObservableCollection<ListToken>();
        //int positionDataSourse = 20;

        List<SearchListHistoryItem> searchListHistoryItems = new List<SearchListHistoryItem>();
        public MainPage()
        {
            InitializeComponent();

            //SQL.AddDataToken("solana", 3, 160, 1);
            //SQL.AddBagToken("MyPortfile", "");

            //SQL.DelAll();
            //PageAssetsCoin.HeightRequest = PageAssetsCoin.Height + 300;
            //gg.HeightRequest = gg.Height + 300;

            _ = CreateCardListMainHistory(SQL.GetListTokensTransactionBuy(ActiveBagTokensId), SQL.GetListTokensTransactionSale(ActiveBagTokensId));
            try
            {
                InitializeMainCryptoList();
                _ = GetListAllToken();
                _ = GetListSearchHistory();
                //GetCryptoTokensAssetsCoin(SQL.GetListTokensTransactionBuy(), SQL.GetListTokensTransactionSale());
            }
            catch
            {
                _ = GetPushBox("Ошибка подключения к сети", ImgPushBox.wifi);
            }
            
        }

        private async Task GetListSearchHistory()
        {
            CryptoCurrency[] FullListCrypto = await APICoinGecko.GetListTokenS();
            List<TokensAssets> PortfileListCrypto = SQL.GetListTokens(ActiveBagTokensId);

            var All = FullListCrypto
                                .Where(crypto => PortfileListCrypto.Any(token => token.TokenID == crypto.Id));

            foreach (CryptoCurrency item in FullListCrypto)
            {
                if (All.Contains(item))
                {
                    searchListHistoryItems.Add(new SearchListHistoryItem { ImageCrypto = item.Image, ImagePortfile = "portfile_yes.svg", Name=$"{item.Name} ({item.Symbol.ToUpper()})", TokenId=item.Id } );
                }
                else
                {
                    searchListHistoryItems.Add(new SearchListHistoryItem { ImageCrypto = item.Image, ImagePortfile = "", Name = $"{item.Name} ({item.Symbol.ToUpper()})", TokenId = item.Id });
                }
            }
        }
        
        private void CreateCardDateMainListHistory(DateTime date)
        {
            var horizontalStackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(15, 5, 0, 0),
                HorizontalOptions = LayoutOptions.Start
            };

            var label1 = new Label
            {
                Text = date.ToString("dd.MM"),
                TextColor = Color.Parse("#B6B6B6"),
                FontSize = 15,
                FontAttributes = FontAttributes.Bold
            };

            var label2 = new Label
            {
                Text = date.Year.ToString(),
                TextColor = Color.Parse("#686868"),
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(10, 0, 0, 0)
            };

            horizontalStackLayout.Children.Add(label1);
            horizontalStackLayout.Children.Add(label2);

            MainHistoryStack.Add(horizontalStackLayout);
        }
        private async Task CreateCardListMainHistory(List<TokensTransactionBuy> buyList, List<TokensTransactionSale> saleList)
        {
            var combinedAndSorted = buyList.Cast<object>()
                            .Concat(saleList.Cast<object>())
                            .OrderByDescending(transaction => transaction is TokensTransactionBuy ? ((TokensTransactionBuy)transaction).Date : ((TokensTransactionSale)transaction).Date)
                            .ToList();
            CryptoCurrency[] ListCrypto = await APICoinGecko.GetListTokenS();


            MainHistoryStack.Children.Clear();
            if (combinedAndSorted.Count <= 0)
            {
                MainHistoryStack.Add(new Label
                {
                    TextColor = Colors.White,
                    Text = "Нет Транзакций",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 18,
                    Margin = new Thickness(0, 20, 0, 0)
                });
            }
            else
            {
                for (int i = 0; i < combinedAndSorted.Count; i++)
                {
                    if (combinedAndSorted[i] is TokensTransactionBuy BuyTransaction)
                    {
                        if (i == 0)
                        {
                            CreateCardDateMainListHistory(BuyTransaction.Date);
                        }
                        else
                        {
                            if (combinedAndSorted[i-1] is TokensTransactionBuy OldBuy)
                            {
                                if (OldBuy.Date.Date != BuyTransaction.Date.Date)
                                {
                                    CreateCardDateMainListHistory(BuyTransaction.Date);
                                }
                            }
                        }

                        var border = new DXBorder
                        {
                            Margin = new Thickness(10, 5, 10, 0),
                            CornerRadius = 10,
                            HeightRequest = 50,
                            BackgroundColor = Color.Parse("#2E2E2E")
                        };

                        var grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition());

                        var infoImage = new Image
                        {
                            Margin = new Thickness(10),
                            Source = ListCrypto.FirstOrDefault(x => x.Id == BuyTransaction.TokenId).Image
                        };

                    var currencyLabel = new Label
                    {
                        Text = ListCrypto.FirstOrDefault(x => x.Id == BuyTransaction.TokenId).Symbol.ToUpper(),
                        TextColor = Color.Parse("#D8D8D8"),
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var valueLabel = new Label
                    {
                        Text = BuyTransaction.Count.ToString(),
                        TextColor = Colors.White,
                        FontSize = 15,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var priceLabel = new Label
                    {
                        Text = InsertSeparator((BuyTransaction.Count * BuyTransaction.Price).ToString()),
                        TextColor = Color.Parse("#BABABA"),
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var grid2 = new Grid();
                    grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                    grid2.Children.Add(priceLabel);
                    grid2.Children.Add(valueLabel);
                    Grid.SetColumn(grid2, 2);
                    Grid.SetRow(priceLabel, 1);

                    var buyLabel = new Label
                    {
                        Text = "BUY",
                        TextColor = Color.Parse("#24FF00"),
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center
                    };

                    // Добавление элементов в иерархию
                    grid.Children.Add(infoImage);
                    grid.Children.Add(currencyLabel);
                    grid.Children.Add(grid2);
                    grid.Children.Add(buyLabel);

                    Grid.SetColumn(infoImage, 0);
                    Grid.SetColumn(currencyLabel, 1);
                    Grid.SetColumn(buyLabel, 3);

                    // Добавление grid в DXBorder
                    border.Content = grid;

                    MainHistoryStack.Add(border);

                    }
                    else if (combinedAndSorted[i] is TokensTransactionSale SaleTransaction)
                    {

                    }
                }
            }
        }
        private void InitializeMainCryptoList()
        {
            List<string> Tokens = new List<string>();
            foreach (var t in SQL.GetListTokens(ActiveBagTokensId))
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

        //добавление карточки токена на главной странице
        private async void GetCryptoTokensMainMenu(List<string> TokenIDs)
        {
            if (TokenIDs.Count <= 0)
            {
                StackMainTokens.Children.Add(new Label
                {
                    Text = "Нет активов",
                    FontSize = 20,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center
                });
            }
            else
            {
                APICoinGecko.Coin[] InfoTokens = await APICoinGecko.GetTokensInfoToIDs(TokenIDs);
                double? TotalAssetsCount = 0;
                StackMainTokens.Children.Clear();

                foreach (var Token in InfoTokens)
                {
                    var tokendb = SQL.GetTokenToId(Token.Id, ActiveBagTokensId);
                    TotalAssetsCount += tokendb.TokenCount * Token.current_price;
                    if (tokendb.TokenCount <= 0)
                    {
                        continue;
                    }
                    var dxBorder = new DXBorder
                    {
                        Margin = new Thickness(0, 6, 0, 0),
                        HeightRequest = 70,
                        BackgroundColor = Color.Parse("#282828"),
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

                    var tapIsImage = new TapGestureRecognizer();
                    tapIsImage.Tapped += TapCryptoIsMainList;
                    image.GestureRecognizers.Add(tapIsImage);
                    image.AutomationId = $"MainImg/{tokendb.TokenID}";

                    var label1 = new Label
                    {
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 20,
                        TextColor = Color.Parse("#989898"),
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
                        TextColor = Color.Parse("#24FF00"),
                        Text = InsertSeparator((((Token.current_price - tokendb.AVGPriceBuy) / Token.current_price) * 100).ToString()) + "%",
                        FontAttributes = FontAttributes.Bold
                    };
                    if (label3.Text[0] == '-')
                    {
                        label3.TextColor = Color.Parse("#FF0000");
                    }
                    Grid.SetColumn(label3, 2);
                    grid.Children.Add(label3);

                    var dxBorder2 = new DXBorder
                    {
                        CornerRadius = 15,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Start,
                        BackgroundColor = Color.Parse("#141414"),
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    Grid.SetColumn(dxBorder2, 2);
                    Grid.SetRow(dxBorder2, 1);
                    grid.Children.Add(dxBorder2);

                    var label4 = new Label
                    {
                        FontSize = 15,
                        TextColor = Color.Parse("#24FF00"),
                        Text = "$" + InsertSeparator((((tokendb.AVGPriceBuy / 100) * (((Token.current_price - tokendb.AVGPriceBuy) / Token.current_price) * 100)) * tokendb.TokenCount).ToString()),
                        FontAttributes = FontAttributes.Bold
                    };
                    if (label4.Text[1] == '-')
                    {
                        label4.TextColor = Color.Parse("#FF0000");
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
                        BackgroundColor = Color.Parse("#141414"),
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
            
        }

        //список транзакци по монете
        private void GetCryptoTokensAssetsCoin(List<TokensTransactionBuy> buyList, List<TokensTransactionSale> saleList)
        {
            var combinedAndSorted = buyList.Cast<object>()
                            .Concat(saleList.Cast<object>())
                            .OrderByDescending(transaction => transaction is TokensTransactionBuy ? ((TokensTransactionBuy)transaction).Date : ((TokensTransactionSale)transaction).Date)
                            .ToList();
            LayoutAssetsCoin.Children.Clear();
            if (combinedAndSorted.Count <= 0)
            {
                LayoutAssetsCoin.Add(new Label
                {
                    TextColor = Colors.White,
                    Text = "Нет Транзакций",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 18,
                    Margin = new Thickness(0,20,0,0)
                });
            }
            else
            {
                foreach (var transaction in combinedAndSorted)
                {
                    if (transaction is TokensTransactionBuy buyTransaction)
                    {
                        var dxBorder = new DXBorder
                        {
                            CornerRadius = 20,
                            BackgroundColor = Color.Parse("#222222"),
                            HeightRequest = 120,
                            Margin = new Thickness(12, 5, 12, 5)
                        };
                        dxBorder.AutomationId = $"buy/{buyTransaction.Id}/dxBorder";

                        var mainGrid = new Grid();
                        mainGrid.AutomationId = $"buy/{buyTransaction.Id}/mainGrid";

                        var grid = new Grid();
                        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) });
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());

                        grid.AutomationId = $"buy/{buyTransaction.Id}/grid";
                        grid.Margin = new Thickness(10, 5, 10, 5);

                        var innerGrid = new Grid();
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition());

                        innerGrid.RowDefinitions.Add(new RowDefinition());
                        innerGrid.RowDefinitions.Add(new RowDefinition());

                        var label1 = new Label { Text = $"{buyTransaction.Date.Day}.{buyTransaction.Date.Month}", FontSize = 12, TextColor = Color.Parse("#B7B7B7") };
                        Grid.SetRow(label1, 0);
                        Grid.SetColumn(label1, 0);

                        var label2 = new Label { Text = buyTransaction.Date.Year.ToString(), FontSize = 12, TextColor = Color.Parse("#6A6A6A"), VerticalOptions = LayoutOptions.Start };
                        Grid.SetRow(label2, 1);
                        Grid.SetColumn(label2, 0);

                        var label3 = new Label { Text = "Покупка", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.Parse("#05FF00"), HorizontalTextAlignment = TextAlignment.Center };
                        Grid.SetRow(label3, 0);
                        Grid.SetColumn(label3, 1);

                        var label4 = new Label { Text = InsertSeparator(buyTransaction.Count.ToString()), FontSize = 17, TextColor = Color.Parse("#EAEAEA"), HorizontalTextAlignment = TextAlignment.Center };
                        Grid.SetRow(label4, 0);
                        Grid.SetColumn(label4, 2);

                        var label5 = new Label { Text = "⁝", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.Parse("#EAEAEA"), HorizontalTextAlignment = TextAlignment.End };
                        var tapIsOption = new TapGestureRecognizer();
                        tapIsOption.Tapped += TapIsOptionAssetsHistoryTransactionBuy;
                        label5.GestureRecognizers.Add(tapIsOption);
                        label5.AutomationId = $"buy/{buyTransaction.Id}/label5";

                        Grid.SetRow(label5, 0);
                        Grid.SetColumn(label5, 3);

                        var label6 = new Label { Text = "Цена:", FontSize = 15, TextColor = Color.Parse("#E8E8E8") };
                        Grid.SetRow(label6, 1);
                        Grid.SetColumn(label6, 0);

                        var label7 = new Label { Text = "$" + InsertSeparator(buyTransaction.Price.ToString()), FontSize = 16, TextColor = Color.Parse("#E8E8E8"), HorizontalOptions = LayoutOptions.End };
                        Grid.SetRow(label7, 0);
                        Grid.SetColumn(label7, 1);

                        var label8 = new Label { Text = "Стоимость:", FontSize = 15, TextColor = Color.Parse("#E8E8E8") };
                        Grid.SetRow(label8, 0);
                        Grid.SetColumn(label8, 0);

                        var label9 = new Label { Text = "$" + InsertSeparator((buyTransaction.Count * buyTransaction.Price).ToString()), FontSize = 16, TextColor = Color.Parse("#E8E8E8"), HorizontalOptions = LayoutOptions.End };
                        Grid.SetRow(label9, 0);
                        Grid.SetColumn(label9, 1);

                        innerGrid.Children.Add(label1);
                        innerGrid.Children.Add(label2);
                        innerGrid.Children.Add(label3);
                        innerGrid.Children.Add(label4);
                        innerGrid.Children.Add(label5);

                        grid.Children.Add(innerGrid);
                        grid.Children.Add(label6);
                        grid.Children.Add(label7);
                        grid.Children.Add(label8);
                        grid.Children.Add(label9);

                        Grid.SetRow(label7, 1);
                        Grid.SetRow(label8, 2);
                        Grid.SetRow(label9, 2);

                        var grid2 = new Grid()
                        {
                            Margin = new Thickness(10, 5, 10, 5),
                            IsVisible = true
                        };
                        grid2.AutomationId = $"buy/{buyTransaction.Id}/grid2";

                        grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        var deleteImage = new Image()
                        {
                            Source = "delete.svg",
                            Aspect = Aspect.Center
                        };
                        deleteImage.AutomationId = $"buy/{buyTransaction.Id}/deleteImage";
                        var TapIsdeleteImage = new TapGestureRecognizer();
                        TapIsdeleteImage.Tapped += TapIsdeleteImageBuy; ;
                        deleteImage.GestureRecognizers.Add(TapIsdeleteImage);

                        var editImage = new Image()
                        {
                            Source = "edit.svg",
                            Aspect = Aspect.Center
                        };
                        editImage.AutomationId = $"buy/{buyTransaction.Id}/editImage";
                        var TapIseditImage = new TapGestureRecognizer();
                        TapIseditImage.Tapped += TapIsEditImageBuy;
                        editImage.GestureRecognizers.Add(TapIseditImage);

                        var backImage = new Image()
                        {
                            Source = "back.svg",
                            Aspect = Aspect.Center
                        };
                        backImage.AutomationId = $"buy/{buyTransaction.Id}/backImage";
                        var TapIsbackImage = new TapGestureRecognizer();
                        TapIsbackImage.Tapped += TapIsBackImageAssetsCoinBuy;
                        backImage.GestureRecognizers.Add(TapIsbackImage);


                        grid2.Children.Add(deleteImage);
                        grid2.Children.Add(editImage);
                        grid2.Children.Add(backImage);

                        Grid.SetColumn(editImage, 1);
                        Grid.SetColumn(backImage, 2);
                        grid2.IsVisible = false;

                        mainGrid.Add(grid2);
                        mainGrid.Add(grid);

                        dxBorder.Content = mainGrid;

                        LayoutAssetsCoin.Add(dxBorder);
                    }
                    else if (transaction is TokensTransactionSale saleTransaction)
                    {
                        var dxBorder = new DXBorder
                        {
                            CornerRadius = 20,
                            BackgroundColor = Color.Parse("#222222"),
                            HeightRequest = 120,
                            Margin = new Thickness(12, 5, 12, 5)
                        };
                        dxBorder.AutomationId = $"sale/{saleTransaction.Id}/dxBorder";

                        var mainGrid = new Grid();
                        mainGrid.AutomationId = $"sale/{saleTransaction.Id}/mainGrid";

                        var grid = new Grid();
                        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) });
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.RowDefinitions.Add(new RowDefinition());

                        grid.AutomationId = $"sale/{saleTransaction.Id}/grid";
                        grid.Margin = new Thickness(10, 5, 10, 5);

                        var innerGrid = new Grid();
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                        innerGrid.ColumnDefinitions.Add(new ColumnDefinition());

                        innerGrid.RowDefinitions.Add(new RowDefinition());
                        innerGrid.RowDefinitions.Add(new RowDefinition());

                        var label1 = new Label { Text = $"{saleTransaction.Date.Day}.{saleTransaction.Date.Month}", FontSize = 12, TextColor = Color.Parse("#B7B7B7") };
                        Grid.SetRow(label1, 0);
                        Grid.SetColumn(label1, 0);

                        var label2 = new Label { Text = saleTransaction.Date.Year.ToString(), FontSize = 12, TextColor = Color.Parse("#6A6A6A"), VerticalOptions = LayoutOptions.Start };
                        Grid.SetRow(label2, 1);
                        Grid.SetColumn(label2, 0);

                        var label3 = new Label { Text = "Продажа", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Color.Parse("#FF0000"), HorizontalTextAlignment = TextAlignment.Center };
                        Grid.SetRow(label3, 0);
                        Grid.SetColumn(label3, 1);

                        var label4 = new Label { Text = InsertSeparator(saleTransaction.Count.ToString()), FontSize = 17, TextColor = Color.Parse("#EAEAEA"), HorizontalTextAlignment = TextAlignment.Center };
                        Grid.SetRow(label4, 0);
                        Grid.SetColumn(label4, 2);

                        var label5 = new Label { Text = "⁝", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.Parse("#EAEAEA"), HorizontalTextAlignment = TextAlignment.End };
                        var tapIsOption = new TapGestureRecognizer();
                        tapIsOption.Tapped += TapIsOptionAssetsHistoryTransactionSale;
                        label5.GestureRecognizers.Add(tapIsOption);
                        label5.AutomationId = $"sale/{saleTransaction.Id}/label5";

                        Grid.SetRow(label5, 0);
                        Grid.SetColumn(label5, 3);

                        var label6 = new Label { Text = "Цена:", FontSize = 15, TextColor = Color.Parse("#E8E8E8") };
                        Grid.SetRow(label6, 1);
                        Grid.SetColumn(label6, 0);

                        var label7 = new Label { Text = "$" + InsertSeparator(saleTransaction.Price.ToString()), FontSize = 16, TextColor = Color.Parse("#E8E8E8"), HorizontalOptions = LayoutOptions.End };
                        Grid.SetRow(label7, 0);
                        Grid.SetColumn(label7, 1);

                        var label8 = new Label { Text = "Стоимость:", FontSize = 15, TextColor = Color.Parse("#E8E8E8") };
                        Grid.SetRow(label8, 0);
                        Grid.SetColumn(label8, 0);

                        var label9 = new Label { Text = "$" + InsertSeparator((saleTransaction.Count * saleTransaction.Price).ToString()), FontSize = 16, TextColor = Color.Parse("#E8E8E8"), HorizontalOptions = LayoutOptions.End };
                        Grid.SetRow(label9, 0);
                        Grid.SetColumn(label9, 1);

                        innerGrid.Children.Add(label1);
                        innerGrid.Children.Add(label2);
                        innerGrid.Children.Add(label3);
                        innerGrid.Children.Add(label4);
                        innerGrid.Children.Add(label5);

                        grid.Children.Add(innerGrid);
                        grid.Children.Add(label6);
                        grid.Children.Add(label7);
                        grid.Children.Add(label8);
                        grid.Children.Add(label9);

                        Grid.SetRow(label7, 1);
                        Grid.SetRow(label8, 2);
                        Grid.SetRow(label9, 2);

                        var grid2 = new Grid()
                        {
                            Margin = new Thickness(10, 5, 10, 5),
                            IsVisible = true
                        };
                        grid2.AutomationId = $"sale/{saleTransaction.Id}/grid2";

                        grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        var deleteImage = new Image()
                        {
                            Source = "delete.svg",
                            Aspect = Aspect.Center
                        };
                        deleteImage.AutomationId = $"sale/{saleTransaction.Id}/deleteImage";
                        var TapIsdeleteImage = new TapGestureRecognizer();
                        TapIsdeleteImage.Tapped += TapIsdeleteImageSale; ;
                        deleteImage.GestureRecognizers.Add(TapIsdeleteImage);

                        var editImage = new Image()
                        {
                            Source = "edit.svg",
                            Aspect = Aspect.Center
                        };
                        editImage.AutomationId = $"sale/{saleTransaction.Id}/editImage";
                        var TapIseditImage = new TapGestureRecognizer();
                        TapIseditImage.Tapped += TapIsEditImageSale;
                        editImage.GestureRecognizers.Add(TapIseditImage);

                        var backImage = new Image()
                        {
                            Source = "back.svg",
                            Aspect = Aspect.Center
                        };
                        backImage.AutomationId = $"sale/{saleTransaction.Id}/backImage";
                        var TapIsbackImage = new TapGestureRecognizer();
                        TapIsbackImage.Tapped += TapIsBackImageAssetsCoinSale;
                        backImage.GestureRecognizers.Add(TapIsbackImage);


                        grid2.Children.Add(deleteImage);
                        grid2.Children.Add(editImage);
                        grid2.Children.Add(backImage);

                        Grid.SetColumn(editImage, 1);
                        Grid.SetColumn(backImage, 2);
                        grid2.IsVisible = false;

                        mainGrid.Add(grid2);
                        mainGrid.Add(grid);

                        dxBorder.Content = mainGrid;

                        LayoutAssetsCoin.Add(dxBorder);
                    }
                }
            }
        }

        private async void TapIsdeleteImageBuy(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;
            string id = img.AutomationId.ToString().Split('/')[1];
            TokensTransactionBuy activeToken = SQL.GetTransactionBuyIsId(Convert.ToInt32(id));

            
            foreach (DXBorder dxborder in LayoutAssetsCoin.Children)
            {
                if (dxborder.AutomationId == $"buy/{id}/dxBorder")
                {
                    var animation = new Animation(callback: d => dxborder.Opacity = d, start: 1, end: 0, easing: Easing.Linear);
                    dxborder.Animate(nameof(Opacity), animation, length: 350);
                    await Task.Delay(350);
                    LayoutAssetsCoin.Remove(dxborder);
                    break;
                }
            }
            SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), activeToken.Count, activeToken.IdBagTokens, true);
            SQL.DelTransactionBuyIsId(Convert.ToInt32(id), ActiveInfoTokenTransaction["id"].ToString(), ActiveBagTokensId);
            InitializeMainCryptoList();
            if (LayoutAssetsCoin.Children.Count <= 0)
            {
                // Скрываем элементы с помощью анимации
                await Task.WhenAll(
                swipeLabelAssetsCoin.TranslateTo(swipeLabelAssetsCoin.TranslationX, PageAssetsCoin.Height, 250), // Скрываем элемент
                    PageAssetsCoin.TranslateTo(PageAssetsCoin.TranslationX, PageAssetsCoin.Height, 250) // Скрываем элемент
                );

                await Task.Delay(100);

                // Скрываем элементы с помощью анимации, если они видимы (это важно, чтобы избежать мигания элементов)
                if (MainPageAssetsCoin.IsVisible)
                {
                    // Скрываем элементы
                    MainPageAssetsCoin.IsVisible = false;
                }
            }
        }
        private async void TapIsdeleteImageSale(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;
            string id = img.AutomationId.ToString().Split('/')[1];
            TokensTransactionSale activeToken = SQL.GetTransactionSaleIsId(Convert.ToInt32(id));

            
            foreach (DXBorder dxborder in LayoutAssetsCoin.Children)
            {
                if (dxborder.AutomationId == $"sale/{id}/dxBorder")
                {
                    var animation = new Animation(callback: d => dxborder.Opacity = d, start: 1, end: 0, easing: Easing.Linear);
                    dxborder.Animate(nameof(Opacity), animation, length: 350);
                    await Task.Delay(350);
                    LayoutAssetsCoin.Remove(dxborder);
                    break;
                }
            }

            SQL.UpDateTokenPlus(ActiveInfoTokenTransaction["id"].ToString(), activeToken.Count, Convert.ToDouble(ActiveInfoTokenTransaction["current_price"].ToString().Replace(".", ",")), activeToken.IdBagTokens);
            SQL.DelTransactionSaleIsId(Convert.ToInt32(id), ActiveInfoTokenTransaction["id"].ToString(), ActiveBagTokensId);
            InitializeMainCryptoList();
        }

        
        private void TapIsBackImageAssetsCoinSale(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;

            string id = img.AutomationId.ToString().Split('/')[1];

            foreach (var item in LayoutAssetsCoin.Children)
            {
                if (item is DXBorder border)
                {
                    Grid mainGrid = border.Content as Grid;
                    foreach (Grid item2 in mainGrid.Children)
                    {
                        if (item2.AutomationId == $"sale/{id}/grid")
                        {
                            item2.IsVisible = true;
                        }
                        else if (item2.AutomationId == $"sale/{id}/grid2")
                        {
                            item2.IsVisible = false;
                        }
                    }
                }
            }
        }
        private void TapIsOptionAssetsHistoryTransactionSale(object sender, TappedEventArgs e)
        {
            Label lab = sender as Label;
            
            string id = lab.AutomationId.ToString().Split('/')[1];
            
            foreach(var item in LayoutAssetsCoin.Children)
            {
                if (item is DXBorder border)
                {
                    Grid mainGrid = border.Content as Grid;
                    foreach(Grid item2 in mainGrid.Children)
                    {
                        if (item2.AutomationId == $"sale/{id}/grid")
                        {
                            item2.IsVisible = false;
                        }
                        else if (item2.AutomationId == $"sale/{id}/grid2")
                        {
                            item2.IsVisible = true;
                        }
                    }
                }
            }
        }
        
        private void TapIsBackImageAssetsCoinBuy(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;

            string id = img.AutomationId.ToString().Split('/')[1];

            foreach (var item in LayoutAssetsCoin.Children)
            {
                if (item is DXBorder border)
                {
                    Grid mainGrid = border.Content as Grid;
                    foreach (Grid item2 in mainGrid.Children)
                    {
                        if (item2.AutomationId == $"buy/{id}/grid")
                        {
                            item2.IsVisible = true;
                        }
                        else if (item2.AutomationId == $"buy/{id}/grid2")
                        {
                            item2.IsVisible = false;
                        }
                    }
                }
            }
        }
        private void TapIsOptionAssetsHistoryTransactionBuy(object sender, TappedEventArgs e)
        {
            Label lab = sender as Label;
            
            string id = lab.AutomationId.ToString().Split('/')[1];
            
            foreach(var item in LayoutAssetsCoin.Children)
            {
                if (item is DXBorder border)
                {
                    Grid mainGrid = border.Content as Grid;
                    foreach(Grid item2 in mainGrid.Children)
                    {
                        if (item2.AutomationId == $"buy/{id}/grid")
                        {
                            item2.IsVisible = false;
                        }
                        else if (item2.AutomationId == $"buy/{id}/grid2")
                        {
                            item2.IsVisible = true;
                        }
                    }
                }
            }
        }

        private async void TapCryptoIsMainList(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;
            string CoinId = img.AutomationId.ToString().Split("/")[1];
            TokensAssets tokendbinfo = SQL.GetTokenToId(CoinId, ActiveBagTokensId);
            ActiveInfoTokenTransaction = await APICoinGecko.GetInfoTokenToID(CoinId, "usd");

            double ActivePriceCoin = Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","));

            swipeLabelAssetsCoin.Text = ActiveInfoTokenTransaction["name"].ToString();
            LabelSymbolToAssetcCoin.Text = $"Накопления {ActiveInfoTokenTransaction["symbol"].ToString().ToUpper()}";
            TotalCoinCountToAssetsCoin.Text = InsertSeparator(tokendbinfo.TokenCount.ToString());
            TotalPriceCoinCountToAssetsCoin.Text = "$" + InsertSeparator((tokendbinfo.TokenCount * ActivePriceCoin).ToString());
            DiversificationToAssetsCoin.Text = "Диверсификация: " + InsertSeparator((((tokendbinfo.TokenCount * ActivePriceCoin) / Convert.ToDouble(TotalAssets.Text.Replace("$", ""))) * 100).ToString()) + "%";
            AvgPriceToAssetsCoin.Text = "$" + InsertSeparator(tokendbinfo.AVGPriceBuy.ToString());
            AvgPriceSaleToAssetsCoin.Text = "$" + InsertSeparator(tokendbinfo.AVGPriceSale.ToString());
            PLToAssetsCoin.Text = InsertSeparator((((ActivePriceCoin - tokendbinfo.AVGPriceBuy) / ActivePriceCoin) * 100).ToString()) + "%";
            PLUsdToAssetsCoin.Text = "$" + InsertSeparator((((tokendbinfo.AVGPriceBuy / 100) * (((ActivePriceCoin - tokendbinfo.AVGPriceBuy) / ActivePriceCoin) * 100)) * tokendbinfo.TokenCount).ToString());

            MainPageAssetsCoin.IsVisible = true;
            swipeLabelAssetsCoin.TranslationY += swipeLabelAssetsCoin.Height;
            PageAssetsCoin.TranslationY += PageAssetsCoin.Height;
            await Task.WhenAll(swipeLabelAssetsCoin.TranslateTo(swipeLabelAssetsCoin.TranslationX, 0, 250),
            PageAssetsCoin.TranslateTo(PageAssetsCoin.TranslationX, 0, 250)
            );
            await Task.Delay(50);

            if (InsertSeparator((((ActivePriceCoin - tokendbinfo.AVGPriceBuy) / ActivePriceCoin) * 100).ToString())[0] == '-')
            {
                PLToAssetsCoin.TextColor = Color.Parse("#FF0000");
                PLUsdToAssetsCoin.TextColor = Color.Parse("#FF0000");
            }
            else
            {
                PLToAssetsCoin.TextColor = Color.Parse("#05FF00");
                PLUsdToAssetsCoin.TextColor = Color.Parse("#05FF00");
            }

            GetCryptoTokensAssetsCoin(SQL.GetListTokensTransactionBuyToTokenId(tokendbinfo.TokenID, ActiveBagTokensId), SQL.GetListTokensTransactionSaleToTokenId(tokendbinfo.TokenID, ActiveBagTokensId));
            
        }

        
        //метод для загрузки списка всех криптовалют
        private async Task GetListAllToken()
        {
            CryptoCurrency[] coins = await APICoinGecko.GetListTokenS();

            //for (int i = 0; i < coins.Length; i++)
            //{
            //    dataSource.Add(new ListToken { Id = $"{coins[i].Id}", Name = $"{coins[i].Name} ({coins[i].Symbol.ToUpper()})", Symbol = coins[i].Symbol, Price = (double)coins[i].Current_price, Image = coins[i].Image.ToString() });
            //}

            for (int i = 0; i < coins.Length; i++)
            {
                dataSource.Add(new ListToken { Id = $"{coins[i].Id}", Name = $"{coins[i].Name} ({coins[i].Symbol.ToUpper()})", Symbol = coins[i].Symbol, Price = (double)coins[i].Current_price, Image = coins[i].Image.ToString() });
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
            //string searchText = ((TextEdit)sender).Text;
            //if (searchText.Length > 1)
            //{
            //    var dataClone = dataSource;
            //    foreach (var item in dataClone)
            //    {
            //        if (!item.Name.Contains(searchText) || !item.Symbol.Contains(searchText))
            //        {
            //            dataClone.Remove(item);
            //        }
            //    }
            //    if (dataClone.Count > 0)
            //    {
            //        DataGridListTokens.ItemsSource = dataClone;
            //    }
            //    else
            //    {
            //        DataGridListTokens.ItemsSource = dataSource;
            //    }
            //}
            //else if (searchText == "" || searchText == null)
            //{
            //    DataGridListTokens.ItemsSource = dataSource;
            //}

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
                        if(element.TranslationY >= 0)
                        {
                            PageTransaction.TranslationY += e.TotalY;
                            element.TranslationY += e.TotalY;
                        }
                    }
                    break;

                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    // Если жест завершился до того, как элемент скрылся за экраном,
                    // возвращаем его на исходное место
                    // Проверяем, сдвинулся ли элемент вниз более чем на 50%
                    if (isPanning)
                    {
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
                        else
                        {
                            await Task.WhenAll(
                                element.TranslateTo(element.TranslationX, 0, 250),
                                PageTransaction.TranslateTo(PageTransaction.TranslationX, 0, 250)
                                );

                        }
                    }
                    break;
            }
        }
        async void OnPanUpdatedAssetsCoin(object sender, PanUpdatedEventArgs e)
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

                        // Получаем координаты верхнего края элемента относительно экрана
                        var topOfElementInScreen = element.Y + element.TranslationY;

                        // Проверяем, достиг ли верхний край элемента верхнего края экрана
                        bool isAtTopEdge = topOfElementInScreen <= 10;


                        if (isAtTopEdge)
                        {
                            if (e.TotalY >= 0)
                            {
                                
                                PageAssetsCoin.TranslationY += e.TotalY;
                                element.TranslationY += e.TotalY;
                                BorderIsFullPageAssetsCoin.Opacity = 0;
                            }
                            else
                            {
                                isPanning = false;
                                BorderIsFullPageAssetsCoin.Opacity = 1;
                            }
                            
                        }
                        else
                        {
                            element.TranslationY += e.TotalY;
                            PageAssetsCoin.TranslationY += e.TotalY;
                        }
                    }
                    break;

                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    // Если жест завершился до того, как элемент скрылся за экраном,
                    // возвращаем его на исходное место
                    // Проверяем, сдвинулся ли элемент вниз более чем на 50%
                    if (isPanning)
                    {
                        if (element.TranslationY > (element.Height * 0.5) && PageAssetsCoin.TranslationY > (PageAssetsCoin.Height * 0.5))
                        {
                            isPanning = false; // Прекращаем обработку перемещения

                            // Скрываем элементы с помощью анимации
                            await Task.WhenAll(
                                element.TranslateTo(element.TranslationX, PageAssetsCoin.Height, 250), // Скрываем элемент
                                PageAssetsCoin.TranslateTo(PageAssetsCoin.TranslationX, PageAssetsCoin.Height, 250) // Скрываем элемент
                            );

                            await Task.Delay(100);

                            // Скрываем элементы с помощью анимации, если они видимы (это важно, чтобы избежать мигания элементов)
                            if (MainPageAssetsCoin.IsVisible)
                            {
                                // Скрываем элементы
                                MainPageAssetsCoin.IsVisible = false;
                            }
                        }
                        else
                        {
                            await Task.WhenAll(
                                element.TranslateTo(element.TranslationX, 0, 250),
                                PageAssetsCoin.TranslateTo(PageAssetsCoin.TranslationX, 0, 250)
                                );
                            
                        }
                    }
                    break;
            }
        }

        // завершение транзакции
        private async void TapSuccesTransaction(object sender, TappedEventArgs e)
        {
            void ClosePage()
            {
                InitializeMainCryptoList();
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
                                if (SQL.UpDateTokenMinus("tether", Convert.ToDouble(CountCoinOrUsd.Value), ComboBoxPortfileTransaction.SelectedIndex + 1, false))
                                {
                                    SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / PriceTransaction.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex + 1);
                                    
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
                                if (SQL.UpDateTokenMinus("tether", (Convert.ToDouble(CountCoinOrUsd.Value) * Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), ComboBoxPortfileTransaction.SelectedIndex + 1, false))
                                {
                                    SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex + 1);
                                    
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
                                if (SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), (Convert.ToDouble(CountCoinOrUsd.Value) / Convert.ToDouble(ActiveInfoTokenTransaction[APICoinGecko.CoinField.Current_Price.ToString().ToLower()].ToString().Replace(".", ","))), ComboBoxPortfileTransaction.SelectedIndex + 1, false))
                                {
                                    SQL.AddTransactionSale(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / PriceTransaction.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex+1, true);
                                    
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
                                if (SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId, false) && SQL.CheckExist(ActiveInfoTokenTransaction["id"].ToString(), ComboBoxPortfileTransaction.SelectedIndex + 1))
                                {
                                    SQL.AddTransactionSale(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex + 1, true);
                                    
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
                                SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / PriceTransaction.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex+1);

                                ClosePage();
                                await GetPushBox("Успешно куплено!", ImgPushBox.yes);
                            }
                            else
                            {
                                SQL.AddTransactionBuy(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex + 1);

                                ClosePage();
                                await GetPushBox("Успешно куплено!", ImgPushBox.yes);
                            }
                        }
                        else if (LabelSellTransaction.TextColor.ToArgbHex() == Color.Parse("#FF0000").ToRgbaHex())
                        {
                            string[] info = LabelCountCoinOrUsd.Text.Split(' ');

                            if (info[info.Length - 1] == "USD")
                            {
                                if (SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId, false) && SQL.CheckExist(ActiveInfoTokenTransaction["id"].ToString(), ComboBoxPortfileTransaction.SelectedIndex + 1))
                                {
                                    SQL.AddTransactionSale(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value / PriceTransaction.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex + 1, false);

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
                                if (SQL.UpDateTokenMinus(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(CountCoinOrUsd.Value), ActiveBagTokensId, false) && SQL.CheckExist(ActiveInfoTokenTransaction["id"].ToString(), ComboBoxPortfileTransaction.SelectedIndex + 1))
                                {
                                    SQL.AddTransactionSale(ActiveInfoTokenTransaction["id"].ToString(), Convert.ToDouble(PriceTransaction.Value), Convert.ToDouble(CountCoinOrUsd.Value), (DateTime)DateTimeTransaction.Date, ComboBoxPortfileTransaction.SelectedIndex + 1, false);

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

        
        //подгрузка списка токенов
        //private async void DataGridListTokens_Scrolled(object sender, DataGridViewScrolledEventArgs e)
        //{

        //    // Получение текущего смещения (вертикальной позиции прокрутки)
        //    double offsetY = e.OffsetY;

        //    // Получение высоты всего контента
        //    double extentHeight = e.ExtentHeight;

        //    // Получение высоты видимой области (окна просмотра)
        //    double viewportHeight = e.ViewportHeight;

        //    // Проверка, достигнут ли конец списка
        //    bool scrolledToEnd = offsetY + viewportHeight >= extentHeight;
        //    if (scrolledToEnd)
        //    {
        //        CryptoCurrency[] coins = await APICoinGecko.GetListTokenS();

        //        for (int i = positionDataSourse; i < positionDataSourse+10 && i < coins.Length; i++)
        //        {
        //            dataSource.Add(new ListToken { Id = $"{coins[i].Id}", Name = $"{coins[i].Name} ({coins[i].Symbol.ToUpper()})", Symbol = coins[i].Symbol, Price = (double)coins[i].Current_price, Image = coins[i].Image.ToString() });
        //        }
        //        positionDataSourse += 10;
        //    }
        //}

        private void FacePanRecon(object sender, PanUpdatedEventArgs e)
        {

        }

        //редактирование транзакции 
        private async void TapIsEditImageBuy(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;
            string id = img.AutomationId.ToString().Split("/")[1];
            ActiveIdTransaction.Text = $"buy/{id}";
            var transact = SQL.GetTransactionBuyIsId(Convert.ToInt32(id));

            MainPageAssetsCoinEditTransaction.IsVisible = true;
            PageAssetsCoinEditTransaction.TranslationY += PageAssetsCoinEditTransaction.Height;
            await Task.Delay(20);
            await PageAssetsCoinEditTransaction.TranslateTo(PageAssetsCoinEditTransaction.TranslationX, 0, 250);
            await Task.Delay(50);

            CountAssetsCoinEditTransaction.Value = (decimal?)transact.Count;
            PriceAssetsCoinEditTransaction.Value = (decimal?)transact.Price;
            DateAssetsCoinEditTransaction.Date = transact.Date;
            CountPriceAssetsCoinEditTransaction.Text = "$ " + (transact.Count * transact.Price);
        }
        private async void TapIsEditImageSale(object sender, TappedEventArgs e)
        {
            Image img = sender as Image;
            string id = img.AutomationId.ToString().Split("/")[1];
            ActiveIdTransaction.Text = $"sale/{id}";
            var transact = SQL.GetTransactionBuyIsId(Convert.ToInt32(id));

            MainPageAssetsCoinEditTransaction.IsVisible = true;
            PageAssetsCoinEditTransaction.TranslationY += PageAssetsCoinEditTransaction.Height;
            await Task.Delay(20);
            await PageAssetsCoinEditTransaction.TranslateTo(PageAssetsCoinEditTransaction.TranslationX, 0, 250);
            await Task.Delay(50);

            CountAssetsCoinEditTransaction.Value = (decimal?)transact.Count;
            PriceAssetsCoinEditTransaction.Value = (decimal?)transact.Price;
            DateAssetsCoinEditTransaction.Date = transact.Date;
            CountPriceAssetsCoinEditTransaction.Text = "$ " + (transact.Count * transact.Price);
        }

        private async void TapSuccesEditTransaction(object sender, TappedEventArgs e)
        {
            if (CountAssetsCoinEditTransaction.Value > 0)
            {
                if (PriceAssetsCoinEditTransaction.Value > 0)
                {
                    if (DateAssetsCoinEditTransaction.Date != null)
                    {
                        if (ActiveIdTransaction.Text.Split("/")[0] == "buy")
                        {
                            SQL.UpDateTransactionBuy(Convert.ToInt32(ActiveIdTransaction.Text.Split('/')[1]), ActiveInfoTokenTransaction["id"].ToString(), (double)PriceAssetsCoinEditTransaction.Value, (double)CountAssetsCoinEditTransaction.Value, (DateTime)DateAssetsCoinEditTransaction.Date, ActiveBagTokensId);

                            PageAssetsCoinEditTransaction.TranslationY = 0;
                            await Task.Delay(20);
                            await PageAssetsCoinEditTransaction.TranslateTo(PageAssetsCoinEditTransaction.TranslationX, PageAssetsCoinEditTransaction.Height, 250);
                            await Task.Delay(50);
                            MainPageAssetsCoinEditTransaction.IsVisible = false;

                            await GetPushBox("Успешно изменено", ImgPushBox.yes);
                            //GetCryptoTokensAssetsCoin(SQL.GetListTokensTransactionBuyToTokenId(ActiveInfoTokenTransaction["id"].ToString(), ActiveBagTokensId), SQL.GetListTokensTransactionSaleToTokenId(ActiveInfoTokenTransaction["id"].ToString(), ActiveBagTokensId));
                            InitializeMainCryptoList();
                            TapCryptoIsMainList(new Image { AutomationId = $"new/{ActiveInfoTokenTransaction["id"].ToString()}" }, e);
                        }
                        else
                        {
                            SQL.UpDateTransactionSale(Convert.ToInt32(ActiveIdTransaction.Text.Split('/')[1]), ActiveInfoTokenTransaction["id"].ToString(), (double)PriceAssetsCoinEditTransaction.Value, (double)CountAssetsCoinEditTransaction.Value, (DateTime)DateAssetsCoinEditTransaction.Date, ActiveBagTokensId);

                            PageAssetsCoinEditTransaction.TranslationY = 0;
                            await Task.Delay(20);
                            await PageAssetsCoinEditTransaction.TranslateTo(PageAssetsCoinEditTransaction.TranslationX, PageAssetsCoinEditTransaction.Height, 250);
                            await Task.Delay(50);
                            MainPageAssetsCoinEditTransaction.IsVisible = false;

                            await GetPushBox("Успешно изменено", ImgPushBox.yes);
                            //GetCryptoTokensAssetsCoin(SQL.GetListTokensTransactionBuyToTokenId(ActiveInfoTokenTransaction["id"].ToString(), ActiveBagTokensId), SQL.GetListTokensTransactionSaleToTokenId(ActiveInfoTokenTransaction["id"].ToString(), ActiveBagTokensId));
                            InitializeMainCryptoList();
                            TapCryptoIsMainList(new Image { AutomationId = $"new/{ActiveInfoTokenTransaction["id"].ToString()}" }, e);
                        }
                    }
                    else
                    {
                        await GetPushBox("Заполни поле Дата", ImgPushBox.error_circle);
                    }
                }
                else
                {
                    await GetPushBox("Заполни поле Цена", ImgPushBox.error_circle);
                }
            }
            else
            {
                await GetPushBox("Заполни поле Кол-во", ImgPushBox.error_circle);
            }
        }

        private async void TapCloseMainPageAssetsCoinEditTransaction(object sender, TappedEventArgs e)
        {
            PageAssetsCoinEditTransaction.TranslationY = 0;
            await Task.Delay(20);
            await PageAssetsCoinEditTransaction.TranslateTo(PageAssetsCoinEditTransaction.TranslationX, PageAssetsCoinEditTransaction.Height, 250);
            await Task.Delay(50);
            MainPageAssetsCoinEditTransaction.IsVisible = false;
        }

        private void TapToMainAssetsListOrHistory(object sender, TappedEventArgs e)
        {
            Label label = sender as Label;

            if (label.Text == "Активы")
            {
                
            }
            else if (label.Text == "История")
            {

            }
        }

        private void OnDelegateRequested(object sender, ItemsRequestEventArgs e)
        {
            if (e.Text.Length > 1)
            {
                e.Request = () => {
                    return searchListHistoryItems.Where(i => i.Name.Contains(e.Text, StringComparison.CurrentCultureIgnoreCase)).ToList();
                };
            }
            else
            {
                e.Request = () => {
                    return searchListHistoryItems.Where(i => i.Name.StartsWith(e.Text, StringComparison.CurrentCultureIgnoreCase)).ToList();
                };
            }
            
        }

        












        //async void OnOpenWebButtonClicked(System.Object sender, System.EventArgs e)
        //{
        //    await Browser.OpenAsync("https://www.devexpress.com/maui/");
        //}
    }
}