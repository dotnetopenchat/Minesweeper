using Minesweeper;
using System.Collections.Generic;

namespace MinesweeperController
{
    public class Class1 : IControl
    {
        List<List<MinesCellInfo>> Copy;
        CellInfo LastCell { get; set; }
        List<List<CellInfo>> Map;
        List<CellInfo> HintList { get; set; }
        List<CellInfo> FlagList { get; set; }

        int row, col;
        bool start = true;

        public void Reveal(out bool useFlag, out int y, out int x, List<List<MinesCellInfo>> mines)
        {
            // ½ÃÀÛÁÂÇ¥
            if (start)
            {
                Copy = mines;
                LastCell = new CellInfo();
                HintList = new List<CellInfo>();
                FlagList = new List<CellInfo>();
                Map = new List<List<CellInfo>>();
                y = x = LastCell.Y = LastCell.X = 0;
                useFlag = start = false;
                row = col = mines.Count;
                CopyMap(mines);
                return;
            }

            int? hintValue = Copy[LastCell.Y][LastCell.X].MineCountHint;
            bool _useFlag = Copy[LastCell.Y][LastCell.X].UseFlag;

            if (hintValue == 0 || hintValue == null)
            {
                if (_useFlag)
                {
                    CheckHintList(); // empty cell
                }
                else
                {
                    ScanArea(LastCell.Y, LastCell.X);
                    CheckHintList();
                }
            }
            else if (hintValue > 0)
            {
                AddHintList(LastCell.Y, LastCell.X, LastCell.Hint, LastCell.UseFlag);
                CheckHintList();
            }

            CellInfo result = NextCell(LastCell);

            y = LastCell.Y = result.Y;
            x = LastCell.X = result.X;
            LastCell.Hint = result.Hint;
            useFlag = LastCell.UseFlag = result.UseFlag;
        }

        private void AddHintList(int y, int x, int? hintValue, bool useFlag)
        {
            HintList.Add(new CellInfo(y, x, hintValue, true, useFlag));
            Map[y][x].IsCheck = true;
        }

        private CellInfo NextCell(CellInfo LastCell)
        {
            CellInfo rtnValue = new CellInfo();

            if (FlagList.Count > 0)
            {
                rtnValue = FlagList[FlagList.Count - 1];
                FlagList.RemoveAt(FlagList.Count - 1);
            }
            else
            {
                for (int y = 0; y < row; y++)
                {
                    for (int x = 0; x < row; x++)
                    {
                        if (Map[y][x].IsCheck == false)
                        {
                            if ( !(LastCell.Y == y && LastCell.X == x) )
                            {
                                rtnValue.Y = y;
                                rtnValue.X = x;
                                rtnValue.UseFlag = false;
                                Copy[y][x].IsRevealed = true;
                                return rtnValue;
                            }
                        }
                    }
                }
            }

            return rtnValue;
        }

        private int FindAtList(List<CellInfo> list, int y, int x)
        {
            int rtnValue = -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Y == y && list[i].X == x)
                {
                    rtnValue = i;
                }
            }

            return rtnValue;
        }

        /// <summary>
        /// ÇÃ·¡±× ¼¿ °Ë»ö
        /// </summary>
        /// <returns></returns>
        private bool CheckHintList()
        {
            bool rtnValue = true;

            for (int i = 0; i < HintList.Count; i++)
            {
                List<CellInfo> aroundList = GetAroundCell(HintList[i].Y, HintList[i].X, out int cmdValue); // ÁÖº¯¼¿°¡Á®¿À±â.
                if (cmdValue != 40)
                {
                    if (cmdValue == 30)
                    {
                        int removeIdx = FindAtList(HintList, HintList[i].Y, HintList[i].X);
                        if (removeIdx > 0)
                        {
                            HintList.RemoveAt(removeIdx);
                            i--;
                        }
                    }
                    else
                    {
                        AddCellByCmd(aroundList, cmdValue);
                    }
                }
            }
            return rtnValue;
        }

        /// <summary>
        /// Áßº¹°ª Ã¼Å©
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cellInfo"></param>
        /// <returns></returns>
        private bool ExistCell(List<CellInfo> list, CellInfo cellInfo)
        {
            foreach (CellInfo item in list)
            {
                if (item.Y == cellInfo.Y && item.X == cellInfo.X)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ¸í·É ½ÇÇà
        /// </summary>
        /// <param name="aroundList"></param>
        /// <param name="cmdValue"></param>
        private bool AddCellByCmd(List<CellInfo> aroundList, int cmdValue)
        {
            bool rtnValue = false;
            foreach (CellInfo item in aroundList)
            {
                if (Copy[item.Y][item.X].MineCountHint == null)
                {
                    if (ExistCell(FlagList, item) == false)
                    {
                        FlagList.Add(new CellInfo(item.Y, item.X, null, true, cmdValue == 10 ? true : false));
                        rtnValue = true;
                    }   
                }
            }
            return rtnValue;
        }

        /// <summary>
        /// ÁÖº¯ 8Ä­ ¼¿ Á¤º¸ °¡Á®¿À±â
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private List<CellInfo> GetAroundCell(int y, int x, out int cmdValue)
        {
            List<CellInfo> rtnValue = new List<CellInfo>();
            int qCnt = 0;
            int mCnt = 0;

            for (int ty = y - 1; ty <= y + 1; ty++)
            {
                for (int tx = x - 1; tx <= x + 1; tx++)
                {
                    if (ty < 0 || ty > row - 1 || tx < 0 || tx > col - 1 || (ty == y && tx == x)) // ¹üÀ§ÃÊ°ú.
                    {
                        continue;
                    }

                    if (Copy[ty][tx].MineCountHint == null && Copy[ty][tx].UseFlag == false) // ¹°À½Ç¥ ¼¿.
                    {
                        qCnt++;
                        rtnValue.Add(new CellInfo(ty, tx, Copy[ty][tx].MineCountHint, true, Copy[ty][tx].UseFlag));
                    }

                    if (Copy[ty][tx].UseFlag) // Áö·Ú ¼¿.
                    {
                        mCnt++;
                    }
                }
            }

            int? hintValue = Copy[y][x].MineCountHint;
            if ((hintValue == qCnt && mCnt == 0) || hintValue == (qCnt + mCnt)) // ¹°À½Ç¥¼¿ ÀüºÎ Áö·Ú.
                cmdValue = 10; 
            else if (hintValue == mCnt && qCnt > 0) // ¹°À½Ç¥¼¿ ÀüºÎ Ã¼Å©.
                cmdValue = 20;
            else if (hintValue == mCnt && qCnt == 0) // ÈùÆ®¸®½ºÆ® »èÁ¦
                cmdValue = 30;
            else // Ãß°¡¼¿ ¾øÀ½.
                cmdValue = 40; 

            return rtnValue;
        }

        /// <summary>
        /// Áö¿ª Å½»ö
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        private void ScanArea(int y, int x)
        {
            if (y < 0 || y >= row || x < 0 || x >= col)
                return;

            if (Copy[y][x].UseFlag)
            {
                return;
            }
            else
            {
                if (Map[y][x].IsCheck == false)
                {
                    Map[y][x].IsCheck = true;
                    if (Copy[y][x].MineCountHint != null && Copy[y][x].MineCountHint > 0)
                    {
                        HintList.Add(new CellInfo(y, x, Copy[y][x].MineCountHint, true, false));
                    }

                    if (Copy[y][x].MineCountHint == 0)
                    {
                        ScanArea(y - 1, x - 1);
                        ScanArea(y - 1, x);
                        ScanArea(y - 1, x + 1);
                        ScanArea(y, x - 1);
                        ScanArea(y, x + 1);
                        ScanArea(y + 1, x - 1);
                        ScanArea(y + 1, x);
                        ScanArea(y + 1, x + 1);
                    }
                }
            }
        }


        /// <summary>
        /// ¸Ê º¹»ç
        /// </summary>
        /// <param name="mines"></param>
        private void CopyMap(List<List<MinesCellInfo>> mines)
        {
            Map = new List<List<CellInfo>>();
            for (int y = 0; y < row; y++)
            {
                List<CellInfo> rowList = new List<CellInfo>();
                for (int x = 0; x < row; x++)
                {
                    CellInfo item = new CellInfo
                    {
                        IsCheck = false
                    };
                    rowList.Add(item);
                }
                Map.Add(rowList);
            }
        }

        class CellInfo
        {
            public int Y { get; set; }
            public int X { get; set; }
            public int? Hint { get; set; }
            public bool IsCheck { get; set; }
            public bool UseFlag { get; set; }
            
            public CellInfo() { }
            public CellInfo(int y, int x, int? hint, bool isCheck, bool useFlag)
            {
                Y = y;
                X = x;
                Hint = hint;
                IsCheck = isCheck;
                UseFlag = useFlag;
            }
        }
    }
}