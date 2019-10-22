namespace MinesweeperHost
{
    using System;
    using System.ComponentModel;

    public class MinesCellModel : INotifyPropertyChanged
    {
        public string _cellText = "";
        public bool _isRevealed = false;
        private bool _useFlag = false;

        public int Idx
        {
            get;
            set;
        }

        /// <summary>
        /// 지뢰 여부
        /// </summary>
        public bool IsMines
        {
            get;
            set;
        }

        /// <summary>
        /// 깃발 꽃은 여부
        /// </summary>
        public bool UseFlag
        {
            get
            {
                return _useFlag;
            }
            set
            {
                _useFlag = value;
                if (_useFlag)
                    CellText = "🚩";
            }
        }

        /// <summary>
        /// 선택한 셀인지 여부
        /// </summary>
        public bool IsRevealed
        {
            get { return _isRevealed; }
            private set
            {
                _isRevealed = value;
                OnPropertyChanged("IsRevealed");
            }
        }

        /// <summary>
        /// 주변 지뢰 개수
        /// </summary>
        public int MineCountHint
        {
            get;
            set;
        }

        public string CellText
        {
            get { return _cellText; }
            private set
            {
                _cellText = value;
                OnPropertyChanged("CellText");
            }
        }

        /// <summary>
        /// 셀 선택
        /// </summary>
        public void Reveal()
        {
            if (MineCountHint > 0)
            {
                CellText = MineCountHint.ToString();
            }
            if (IsMines)
            {
                CellText = "(!폭탄!)";
            }

            IsRevealed = true;
            UseFlag = false;
        }

        /// <summary>
        /// 깃발 꽃기
        /// </summary>
        public void Flag()
        {
            IsRevealed = false;
            UseFlag = true;
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void ClearAllPropertyChangedHandlers()
        {
            if (this.PropertyChanged == null)
            {
                return;
            }

            foreach (PropertyChangedEventHandler handler in this.PropertyChanged.GetInvocationList())
            {
                this.PropertyChanged -= handler;
            }
        }
        #endregion
    }
}
