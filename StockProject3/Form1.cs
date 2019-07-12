using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockProject3
{
    public partial class Form1 : Form
    {
        string currentItemCode = null;

        public Form1()
        {
            InitializeComponent();

            searchButton.Click += ButtonClicked; // 검색 버튼을 눌릴 경우

            axKHOpenAPI1.OnEventConnect += OnEventConnect; // 로그인 결과를 받는 이벤트 함수를 먼저 등록해준다.
            axKHOpenAPI1.OnReceiveTrData += OnReceiveTrData; // TR데이터 요청 후 결과 콜백함수 등록
            axKHOpenAPI1.OnReceiveRealData += OnReceiveRealData; // 실시간 정보 요청 결과 콜백함수 등록

            axKHOpenAPI1.CommConnect();  // 1. 로그인창 열기

            priceListBox.DrawItem += ListBoxDrawItem;
            priceListBox.DrawMode = DrawMode.OwnerDrawVariable;
            priceListBox.ItemHeight = priceListBox.Height / 20;

            volumeListBox.DrawItem += ListBoxDrawItem;
            volumeListBox.DrawMode = DrawMode.OwnerDrawVariable;
            volumeListBox.ItemHeight = volumeListBox.Height / 20;
        }

        void ListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (sender.Equals(priceListBox))
            {
                try
                {
                    if(e.Index < 10)
                        e.Graphics.FillRectangle(Brushes.LightSteelBlue, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                    else
                        e.Graphics.FillRectangle(Brushes.LightPink, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));

                    String value = priceListBox.Items[e.Index].ToString();
                    Brush brush;
                    if(value[0] == '-')
                        brush = Brushes.Blue;
                    else
                        brush = Brushes.Red;

                    int x = e.Bounds.X + e.Font.Height / 2;
                    int y = e.Bounds.Y + e.Font.Height / 2;

                    e.Graphics.DrawString(value.Replace("-",""), e.Font, brush, x, y, StringFormat.GenericDefault);

                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }
            }
            else if (sender.Equals(volumeListBox))
            {
                try
                {
                    string value = volumeListBox.Items[e.Index].ToString();

                    int x = e.Bounds.X + e.Font.Height / 2 + e.Bounds.Width / 2;
                    int y = e.Bounds.Y + e.Font.Height / 2;

                    e.Graphics.DrawString(value, e.Font, Brushes.Black, x, y, StringFormat.GenericDefault);
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }

        void OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {
            if (e.sRealType == "주식호가잔량")
            {
                priceListBox.Items.Clear();
                volumeListBox.Items.Clear();

                if (e.sRealKey.Equals(currentItemCode))
                {
                    for (int i = 50; i >= 41; i--)
                    {
                        int 매도호가 = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealType, i));
                        priceListBox.Items.Add(매도호가);
                    }
                    for (int i = 70; i >= 61; i--)
                    {
                        int 매도잔량 = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealType, i));
                        volumeListBox.Items.Add(매도잔량);
                    }
                    for (int i = 51; i <= 60; i++)
                    {
                        int 매수호가 = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealType, i));
                        priceListBox.Items.Add(매수호가);
                    }
                    for (int i = 71; i <= 80; i++)
                    {
                        int 매수잔량 = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealType, i));
                        volumeListBox.Items.Add(매수잔량);
                    }

                }

            }
            if (e.sRealType == "주식체결")
            {
                currentPriceLabel.Text = axKHOpenAPI1.GetCommRealData(e.sRealType, 10).ToString();
                fluctuationLabel.Text = axKHOpenAPI1.GetCommRealData(e.sRealType, 12).ToString() + "%";
            }
        }

        // 로그인을 요청 결과 (콜백함수)
        void OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            // 로그인 요청 결과
            if (e.nErrCode == 200)
            {

            }
            else if (e.nErrCode == 201)
            {
            }
        }


        // TR코드 요청 결과 (콜백함수)
        void OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            // TR코드 요청 결과 (RQ Name으로 구별)
            if (e.sRQName == "주식호가")
            {
                priceListBox.Items.Clear();
                volumeListBox.Items.Clear();

                for (int i = 10; i > 1; i--)
                {
                    int 차선호가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매도"+ i + "차선호가"));
                    priceListBox.Items.Add(차선호가);

                    if (i == 6)
                    {
                        int 차선잔량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매도" + i + "우선잔량"));
                        volumeListBox.Items.Add(차선잔량);
                    }
                    else
                    {
                        int 차선잔량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매도" + i + "차선잔량"));
                        volumeListBox.Items.Add(차선잔량);
                    }
                }

                int 매도최우선호가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매도최우선호가"));
                priceListBox.Items.Add(매도최우선호가);
                int 매도최우선잔량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매도최우선잔량"));
                volumeListBox.Items.Add(매도최우선잔량);
                
                int 매수최우선호가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매수최우선호가"));
                priceListBox.Items.Add(매수최우선호가);
                int 매수최우선잔량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매수최우선잔량"));

                for (int i = 2; i < 11; i++)
                {
                    if (i == 6)
                    {
                        int 호가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매수" + i + "우선호가"));
                        priceListBox.Items.Add(호가);
                        int 잔량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매수" + i + "우선잔량"));
                        volumeListBox.Items.Add(잔량);
                    }
                    else
                    {
                        int 호가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매수" + i + "차선호가"));
                        priceListBox.Items.Add(호가);
                        int 잔량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "매수" + i + "차선잔량"));
                        volumeListBox.Items.Add(잔량);
                    }
                }
                //MessageBox.Show("매도최우선호가: " + 매도최우선호가 + "  ,매수최우선호가: " + 매수최우선호가);
            }
        }

        void ButtonClicked(object sender, EventArgs e)
        {
            if (sender.Equals(searchButton))
            {
                string itemCode = searchTextBox.Text;
                itemNameLabel.Text = axKHOpenAPI1.GetMasterCodeName(itemCode);
                currentItemCode = itemCode;

                // 2. TR코드 값관련 결과 요청
                axKHOpenAPI1.SetInputValue("종목코드", itemCode);
                int res = axKHOpenAPI1.CommRqData("주식호가", "opt10004", 0, "5001");

                if (res == 0)
                {
                    Console.WriteLine("주식호가 요청 성공");
                }
                else
                {
                    Console.WriteLine("주식호가 요청 실패");
                }

            }
        }

        private void volumeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }



}
