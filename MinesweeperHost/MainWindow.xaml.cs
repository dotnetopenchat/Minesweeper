namespace MinesweeperHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Threading;
    using Minesweeper;

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private MinesweeperProc _minesweeperProc;
        private IControl _controller;

        private DispatcherTimer _gameTimer;
        private DateTime _startTime;

        public MainWindow()
        {
            InitializeComponent();

            _minesweeperProc = new MinesweeperProc();
            this.Loaded += this.MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.xMinesArray.ItemsSource = _minesweeperProc.MinesArray;
            _minesweeperProc.CreateMines();
        }

        private bool IsImplementationOf(Type type, Type @interface)
        {
            Type[] interfaces = type.GetInterfaces();

            return interfaces.Any(current => IsSubtypeOf(ref current, @interface));
        }

        private bool IsSubtypeOf(ref Type a, Type b)
        {
            if (a == b)
            {
                return true;
            }

            if (a.IsGenericType)
            {
                a = a.GetGenericTypeDefinition();

                if (a == b)
                {
                    return true;
                }
            }

            return false;
        }

        private void XStartGame_Click(object sender, RoutedEventArgs e)
        {
            this.xStartGame.IsEnabled = false;

            var controllerDLL = Assembly.LoadFile(System.AppDomain.CurrentDomain.BaseDirectory + "\\MinesweeperController.dll");
            foreach (Type type in controllerDLL.GetExportedTypes())
            {
                if (this.IsImplementationOf(type, typeof(IControl)))
                {
                    _controller = (IControl)Activator.CreateInstance(type);
                }
            }
            if (_controller == null)
            {
                MessageBox.Show("IControl를 생성할 수 없습니다.");
                return;
            }

            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = new TimeSpan(0, 0, 0, 0, 0);
            _gameTimer.Tick += this.GameTimer_Tick;
            _startTime = DateTime.Now;
            _gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            _gameTimer.Stop();

            string strResult;
            bool useFlag = false;
            int y = 0;
            int x = 0;
            _controller.Reveal(out useFlag, out y, out x, _minesweeperProc.MinesCellInfoArray);
            bool? result = _minesweeperProc.ChoiceCell(y, x, useFlag, out strResult);

            if (result == null)
            {
                _gameTimer.Start();
            }
            else if (result != null && result.Value)
            {
                _gameTimer.Stop();

                TimeSpan playTime = DateTime.Now - _startTime;
                string endGameDesc = string.Format("총 플레이 시간 : {0}분{1}초\r\n찾은 지뢰 수 : {2}",
                    playTime.Minutes,
                    playTime.Seconds,
                    _minesweeperProc.MineFound);

                MessageBox.Show(strResult);
                MessageBox.Show(endGameDesc);
            }
            else if (result != null && result.Value == false)
            {
                _gameTimer.Stop();

                TimeSpan playTime = DateTime.Now - _startTime;
                string endGameDesc = string.Format("총 플레이 시간 : {0}분{1}초\r\n찾은 지뢰 수 : {2}",
                    playTime.Minutes,
                    playTime.Seconds,
                    _minesweeperProc.MineFound);

                MessageBox.Show(strResult);
                MessageBox.Show(endGameDesc);
            }
        }
    }
}