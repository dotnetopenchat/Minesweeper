namespace MinesweeperHost
{
    using Minesweeper;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MinesweeperProc
    {
        // 지뢰 찾기 가로 칸 수
        private int _minesCol = 9;
        // 지뢰 찾기 세로 칸 수
        private int _minesRow = 9;
        // 찾은 지뢰 개수
        private int _mineFound = 0;
        // 사용한 깃발 개수
        private int _flagCount = 0;
        private List<List<MinesCellModel>> _minesArray;
        // 지뢰 칸 정보 리스트 [컨트롤러 제공 용도]
        private List<List<MinesCellInfo>> _minesCellInfoArray;
        // 지뢰 위치 리스트
        private List<int> _minesPostion = new List<int>(10);
        private readonly int _maxMineCount = 10;

        public MinesweeperProc()
        {
            _minesArray = new List<List<MinesCellModel>>();
            for (int y = 0; y < _minesRow; y++)
            {
                _minesArray.Add(new List<MinesCellModel>());

                for (int x = 0; x < _minesCol; x++)
                {
                    _minesArray[y].Add(new MinesCellModel());
                }
            }

            this.RndMinePostion();
        }

        public List<List<MinesCellModel>> MinesArray
        {
            get { return _minesArray; }
        }

        public List<List<MinesCellInfo>> MinesCellInfoArray
        {
            get { return _minesCellInfoArray; }
        }

        public int MineFound
        {
            get { return _mineFound; }
        }

        /// <summary>
        /// 랜덤 지뢰 위치 생성
        /// </summary>
        private void RndMinePostion()
        {
            while (_minesPostion.Count < _maxMineCount)
            {
                int rndNum = this.Rand(1, (_minesCol * _minesRow));
                if (_minesPostion.Any(p => p == rndNum) == false)
                {
                    _minesPostion.Add(rndNum);
                }
            }
        }

        /// <summary>
        /// 지뢰 생성
        /// </summary>
        public void CreateMines()
        {
            var idx = 0;
            // 지뢰 심기
            for (int y = 0; y < _minesArray.Count; y++)
            {
                for (int x = 0; x < _minesArray[y].Count; x++)
                {
                    _minesArray[y][x].Idx = idx;
                    if (_minesPostion.Any(p => p == idx))
                    {
                        _minesArray[y][x].IsMines = true;
                    }
                    idx++;
                }
            }

            // 지뢰 개수 힌트 적용
            for (int y = 0; y < _minesArray.Count; y++)
            {
                for (int x = 0; x < _minesArray[y].Count; x++)
                {
                    this.SetMineHintNumber(y, x);
                }
            }

            // 컨트롤러 제공 지뢰 셀 정보 리스트 생성
            _minesCellInfoArray = new List<List<MinesCellInfo>>();
            for (int y = 0; y < _minesArray.Count; y++)
            {
                _minesCellInfoArray.Add(new List<MinesCellInfo>());

                for (int x = 0; x < _minesArray[y].Count; x++)
                {
                    _minesCellInfoArray[y].Add(new MinesCellInfo(_minesArray[y][x].Idx,
                        _minesArray[y][x].UseFlag,
                        _minesArray[y][x].IsRevealed,
                        null));
                }
            }
        }

        /// <summary>
        /// 지뢰 존재 여부
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private bool IsExistMine(int y, int x)
        {
            if (y < 0 || y >= _minesRow || x < 0 || x >= _minesCol)
            {
                return false;
            }

            return (_minesArray[y][x].IsMines);
        }

        /// <summary>
        /// 주변 지뢰 개수 힌트 설정
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        private void SetMineHintNumber(int y, int x)
        {
            if (this.IsExistMine(y, x))
            {
                return;
            }

            var mineCnt = 0;

            if (this.IsExistMine(y - 1, x - 1))
                mineCnt++;
            if (this.IsExistMine(y - 1, x))
                mineCnt++;
            if (this.IsExistMine(y - 1, x + 1))
                mineCnt++;
            if (this.IsExistMine(y, x - 1))
                mineCnt++;
            if (this.IsExistMine(y, x + 1))
                mineCnt++;
            if (this.IsExistMine(y + 1, x - 1))
                mineCnt++;
            if (this.IsExistMine(y + 1, x))
                mineCnt++;
            if (this.IsExistMine(y + 1, x + 1))
                mineCnt++;

            _minesArray[y][x].MineCountHint = mineCnt;
        }

        private int Rand(int from, int to)
        {
            Random rnd = new Random();
            return rnd.Next(to - from) + from;
        }

        /// <summary>
        /// 셀 선택
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="useFlag"></param>
        /// <returns>셀 선택 결과 null = 정상 / false = 지뢰 선택(게임종료) / true = 모든 지뢰찾기 완료</returns>
        public bool? ChoiceCell(int y, int x, bool useFlag, out string result)
        {
            result = "";

            if (y < 0 || y >= _minesRow || x < 0 || x >= _minesCol)
            {
                result = "잘못된 좌표 선택 입니다.";
                Console.WriteLine("잘못된 좌표 선택 입니다.");
                return null;
            }

            if (_minesArray[y][x].IsRevealed)
            {
                result = "이미 선택한 좌표 입니다.";
                Console.WriteLine("이미 선택한 좌표 입니다.");
                return null;
            }
            if (_minesArray[y][x].UseFlag)
            {
                result = "깃발이 꽃힌 좌표 입니다.";
                Console.WriteLine("깃발이 꽃힌 좌표 입니다.");
                return null;
            }
            if (useFlag == false && this.IsExistMine(y, x))
            {
                result = "지뢰를 선택했습니다!!\r\n게임 종료";
                Console.WriteLine("지뢰를 선택했습니다!! 게임 종료");
                this.ShowMinesArray();
                return false;
            }

            if (useFlag)
            {
                _minesArray[y][x].UseFlag = true;
                _minesCellInfoArray[y][x].UseFlag = true;
                if (_minesArray[y][x].IsMines)
                {
                    _mineFound++;
                }
                _flagCount++;

                // 모든 깃발 사용 완료, 게임 종료
                if (_flagCount >= _maxMineCount)
                {
                    Console.WriteLine("모든 깃발 사용 완료");
                    // 모든 지뢰 찾기 완료, 승리
                    if (_mineFound >= _maxMineCount)
                    {
                        result = "모든 지뢰를 찾았습니다.!!";
                        Console.WriteLine("모든 지뢰를 찾았습니다.!!");
                        return true;
                    }
                    else
                    {
                        result = "깃발을 모두 사용했습니다.\r\n모든 지뢰를 찾지 못했습니다!";
                        Console.WriteLine("깃발을 모두 사용했습니다. 모든 지뢰를 찾지 못했습니다!");
                        return false;
                    }
                }
                return null;
            }

            // 선택한 좌표의 사방으로 숫자를 만날때 까지 모두 오픈한다.
            this.RevealCells(y, x);

            int remainingCells = 0;
            for (int row = 0; row < _minesArray.Count; row++)
            {
                for (int col = 0; col < _minesArray[row].Count; col++)
                {
                    if (_minesArray[row][col].IsRevealed == false)
                    {
                        remainingCells++;
                    }
                }
            }

            // 깃발은 모두 꽃지 않았지만 지뢰 빼고 모든 땅을 clear한 경우
            if ((remainingCells) == _maxMineCount)
            {
                result = "모든 지뢰를 찾았습니다.!!";
                Console.WriteLine("모든 지뢰를 찾았습니다.!!");
                return true;
            }
            else
            {
                return null;
            }
        }

        private void RevealCells(int y, int x)
        {
            if (y < 0 || y >= _minesRow || x < 0 || x >= _minesCol)
                return;

            MinesCellModel minesCellInfo = _minesArray[y][x];
            if (minesCellInfo.IsMines)
            {
                return;
            }
            else
            {
                if (minesCellInfo.IsRevealed == false)
                {
                    minesCellInfo.Reveal();

                    _minesCellInfoArray[y][x].IsRevealed = minesCellInfo.IsRevealed;
                    _minesCellInfoArray[y][x].UseFlag = minesCellInfo.UseFlag;
                    _minesCellInfoArray[y][x].MineCountHint = minesCellInfo.MineCountHint;

                    if (minesCellInfo.MineCountHint == 0)
                    {
                        // call recursive
                        RevealCells(y - 1, x - 1);
                        RevealCells(y - 1, x);
                        RevealCells(y - 1, x + 1);
                        RevealCells(y, x - 1);
                        RevealCells(y, x + 1);
                        RevealCells(y + 1, x - 1);
                        RevealCells(y + 1, x);
                        RevealCells(y + 1, x + 1);
                    }
                }
            }
        }

        private void ShowMinesArray()
        {
            for (int y = 0; y < _minesArray.Count; y++)
            {
                for (int x = 0; x < _minesArray[y].Count; x++)
                {
                    _minesArray[y][x].Reveal();
                }
            }
        }
    }
}
