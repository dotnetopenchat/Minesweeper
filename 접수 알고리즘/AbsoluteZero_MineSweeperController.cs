using Minesweeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperController
{
    public class Cell
    {
        public static Cell Default = new Cell(0, 0, null);
        public readonly int X;
        public readonly int Y;
        private MinesCellInfo _mine;
        public bool IsExistCell => _mine != null;
        public int Idx => _mine == null ? 0 : _mine.Idx;
        public bool IsRevealed => _mine == null ? false : _mine.IsRevealed;
        public bool UseFlag => _mine == null ? false : _mine.UseFlag;
        public int? MineCountHint => _mine == null ? 0 : _mine.MineCountHint;
        public Cell(int X, int Y, MinesCellInfo mine)
        {
            this.X = X;
            this.Y = Y;
            this._mine = mine;
        }
    }
    public class RevealResult
    {
        public readonly int RemainMines;
        public readonly int unFlagCount;

        public readonly Cell cell;

        public RevealResult(int unFlagCount, int remainMines, Cell cell)
        {
            this.unFlagCount = unFlagCount;
            this.RemainMines = remainMines;
            this.cell = cell;
        }
    }
    public class MinesweeperControl : IControl
    {
        private int MaxColumns = 8;
        private int MaxRows = 8;
        private int revealCount = 0;
        private Dictionary<int, Cell> _Cells = null;
        private IEnumerable<Cell> _AvailableCells = null;
        private int GetIndex(int x, int y)
        {
            return (y * MaxColumns) + x;
        }
        /// <summary>
        /// 입력 받은 전체 셀 목록을 Cell 객체 목록으로 변환하여 반환
        /// </summary>
        /// <param name="mines">전체 셀 목록</param>
        /// <returns>가공된 Cell 목록</returns>
        private void InitializeCells(List<List<MinesCellInfo>> mines)
        {
            _Cells = new Dictionary<int, Cell>();
            MaxRows = mines.Count;
            MaxColumns = mines[0].Count;
            for (var y = 0; y < MaxRows; y++)
            {
                for (var x = 0; x < MaxColumns; x++)
                {
                    _Cells.Add(GetIndex(x, y), new Cell(x, y, mines[y][x]));
                }
            }
        }
        /// <summary>
        /// 입력 받은 셀 주변 셀을 반환한다.
        /// Max와 Min을 이용하여 Boundary를 벗어 나지 않도록 처리하였다.
        /// 반환 값은 중심 셀을 제외한 8개의 셀로 정의된다.
        /// </summary>
        /// <param name="cell">중심 셀</param>
        /// <returns>주변 셀들</returns>
        private IEnumerable<Cell> GetAdjacentCells(Cell cell)
        {
            var startY = Math.Max(0, cell.Y - 1);
            var endY = Math.Min(MaxRows - 1, cell.Y + 1);

            var startX = Math.Max(0, cell.X - 1);
            var endX = Math.Min(MaxColumns - 1, cell.X + 1);

            for (var y = startY; y <= endY; y++)
            {
                for (var x = startX; x <= endX; x++)
                {
                    if (x == cell.X && y == cell.Y) continue;
                    yield return _Cells[GetIndex(x, y)];
                }
            }
        }
        /// <summary>
        /// 선택 가능한 셀(탐색 되지 않았고, 깃발이 꽂히지 않고, 주변에 지뢰수가 0인 셀) 중에 랜덤하게 하나 선택하여 반환
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        public Cell RandomChoice(out int y, out int x)
        {
            Random r = new Random();
            // 다음 셀을 랜덤 선택한다.(사용하지 않은 셀 중에서)
            var selection = _AvailableCells.ElementAt(r.Next(_AvailableCells.Count() - 1));
            y = selection.Y;
            x = selection.X;
            return selection;
        }
        /// <summary>
        /// 선택가능하지 않거나, 깃발이 꽂힌 셀 주변을 탐색하여 지뢰가 꽂히지 않은 셀 반환
        /// </summary>
        /// <param name="cell">선택 가능하지 않거나 깃발이 꽂힌 셀</param>
        /// <returns></returns>
        private RevealResult RevealCondition(Cell cell)
        {
            var MineCountHint = cell.MineCountHint.Value;
            if (MineCountHint != 0)
            {
                var adjacentCells = GetAdjacentCells(cell);
                var unrevealCells = adjacentCells.Where(c => !c.IsRevealed && !c.UseFlag);
                var unrevealCount = unrevealCells.Count();
                int flagCount = adjacentCells.Count(c => !c.IsRevealed && c.UseFlag);
                int remainMines = MineCountHint - flagCount;
                return new RevealResult(unrevealCount, remainMines, unrevealCount != 0 ? unrevealCells.First() : new Cell(0, 0, null));
            }
            return null;
        }
        public void Reveal(out bool useFlag, out int y, out int x, List<List<MinesCellInfo>> mines)
        {
            /*
            * 지뢰찾기 게임 룰 설명
            * 일반 지뢰찾기 게임과 동일하다
            *
            * 1. 지뢰는 모두 10개 이다 [랜덤 배치]
            * 2. 칸은 9 X 9 배열 판이다.
            * 3. mines변수에서 현재 지뢰찾기 판 상태를 확인 할 수 있다.
            * 	- 2차원 배열로 제공되며, 좌표계는 Y,X 좌표 다.
            * 4. 특정 칸을 선택하면 선택한 칸 기준으로 8칸 주변(상 하 좌 우 대각선 모두)의 지뢰가 있는 숫자를 표시해준다.
            * 5. 지뢰를 선택하지 않고 일반 칸을 모두 선택하거나, 지뢰로 판단되는 칸에 깃발을 모두 꽃으면 승리로 게임 종료 된다.
            * 6. 지뢰를 선택하면 바로 게임은 종료 된다.
            *
            */

            // 초기에 입력 받은 mines를 이용하여 Dictionary 형태로 변환하여 저장해둔다.
            if (_Cells == null)
            {
                InitializeCells(mines);
            }
            _AvailableCells = _Cells.Values.Where(c => !c.UseFlag && !c.IsRevealed && !c.MineCountHint.HasValue);
            var _UnavailableCells = _Cells.Values.Except(_AvailableCells);
            Cell cell = Cell.Default;
            // 깃발이 꽂힌 셀을 기준으로 주변 셀을 탐색하여 대상이 되는 셀을 찾아서 선택한다.
            foreach (var flaggedCell in _UnavailableCells.Where(c => c.UseFlag))
            {
                foreach (var flag in GetAdjacentCells(flaggedCell).Where(c => c.IsRevealed))
                {
                    var RevealItem = RevealCondition(flag);
                    if (RevealItem == null) continue;
                    if (RevealItem.RemainMines == 0)
                    {
                        cell = RevealItem.cell;
                        if (cell.IsExistCell)
                        {
                            y = cell.Y;
                            x = cell.X;
                            useFlag = false;
                            revealCount++;
                            Console.WriteLine($"{revealCount} {y},{x} {useFlag} flagging");
                            return;
                        }

                    }

                }
            }
            // 지뢰를 찾아서 마킹한다.
            // 지뢰는 선택한 셀을 기준으로 8개까지 존재 할 수 있다.(선택 셀을 제외한 나머지가 모두 지뢰) 
            for (var n = 1; n <= 8; n++)
            {
                foreach (var unavailableCell in _UnavailableCells.Where(c => c.IsRevealed && c.MineCountHint.Value == n))
                {
                    var RevealItem = RevealCondition(unavailableCell);
                    if (RevealItem == null) continue;
                    if (RevealItem.unFlagCount == RevealItem.RemainMines)
                    {
                        cell = RevealItem.cell;
                        if (cell.IsExistCell)
                        {
                            y = cell.Y;
                            x = cell.X;
                            useFlag = true;
                            revealCount++;
                            Console.WriteLine($"{revealCount} {y},{x} {useFlag} mining");
                            return;
                        }
                    }
                }
            }
            // 첫 위치를 선택한다.
            cell = RandomChoice(out y, out x);
            useFlag = false;
            revealCount++;
            Console.WriteLine($"{revealCount} {y},{x} {useFlag} first");
            //Console.WriteLine($"{y}, {x}");
        }
    }
}
