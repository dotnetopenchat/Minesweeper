using Minesweeper;
using System.Collections.Generic;

namespace minesweeperController
{
    public class MinesweeperController : IControl
    {
        static List<List<MinesCellInfo>> state;
        static int[] dx = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };
        static int[] dy = new int[8] { -1, 1, 0, -1, 1, 0, -1, 1 };
        static double[,] _aMxProb;
        static double[,] _aMnProb;
        public void Reveal
        (out bool useFlag, out int y, out int x, List<List<MinesCellInfo>> mines)
        {
            state = mines;

            int h, w;
            h = w = state.Count;

            _aMxProb = new double[h, w];
            _aMnProb = new double[h, w];

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    _aMxProb[i, j] = -2;
                    _aMnProb[i, j] = 2;
                }
            }

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (mines[i][j].UseFlag) continue;
                    if (mines[i][j].IsRevealed)
                    {
                        int? cnt = mines[i][j].MineCountHint;
                        if (cnt == 0) continue;
                        int n = 8;

                        for (int k = 0; k < 8; k++)
                        {
                            int ny = i + dy[k];
                            int nx = j + dx[k];

                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                n--;
                                continue;
                            }
                            if (mines[ny][nx].UseFlag)
                            {
                                cnt--;
                                n--;
                            }
                            else if (mines[ny][nx].IsRevealed) n--;
                        }
                        for (int k = 0; k < 8; k++)
                        {
                            int ny = i + dy[k];
                            int nx = j + dx[k];

                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                            {
                                continue;
                            }
                            if (mines[ny][nx].UseFlag) ;
                            else if (mines[ny][nx].IsRevealed) ;
                            else
                            {
                                if (_aMxProb[ny, nx] < ((double)cnt) / n)
                                    _aMxProb[ny, nx] = ((double)cnt) / n;
                                if (_aMnProb[ny, nx] > ((double)cnt) / n)
                                    _aMnProb[ny, nx] = ((double)cnt) / n;
                            }
                        }
                    }
                }
            }

            double mxProb = -1;
            double mnProb = 2;
            x = y = 0;
            int mnx = 0, mny = 0;

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if ((mines[i][j].IsRevealed) || (mines[i][j].UseFlag)) continue;

                    if (mxProb < _aMxProb[i, j])
                    {
                        y = i;
                        x = j;
                        mxProb = _aMxProb[i, j];
                    }

                    if (_aMnProb[i, j] < mnProb)
                    {
                        mny = i;
                        mnx = j;
                        mnProb = _aMnProb[i, j];
                    }
                }
            }

            if (mxProb * 100 == 100)
            {
                useFlag = true;
                return;
            }

            y = mny;
            x = mnx;
            useFlag = false;
        }
    }
}
