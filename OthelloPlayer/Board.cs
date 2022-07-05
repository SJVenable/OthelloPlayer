using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloPlayer
{
    class Board
    {
        public struct coordinate
        {
            public int column;
            public int row;

            public coordinate(int setRow, int setColumn)
            {
                column = setColumn;
                row = setRow;
            }
        }

        public static string[,] BoardState = new string[8, 8];

        public Board()
        {
            setUpBoard();

        }

        public void placeCounter(ref Board board, bool PlayerTurn, int row, int column)
        {
            if(PlayerTurn)
            {
                turnCountersForPlayer(ref board, row, column, true);
            }
            else BoardState[row, column] = "W";
            //add version to flip white counters (AI turn)
        }

        public  bool checkValidMove(ref Board board, bool PlayerTurn, int row, int column)
        {
            if (BoardState[row, column] != " ")
            {
                Console.WriteLine("already a counter here!");
                return false;
            }
            if (PlayerTurn)
            {
                if (turnCountersForPlayer(ref board, row, column, false).Count != 0)
                {
                    return true;
                }
                return false;
            }
            return false;
            //else do same for turnCountersForAI
        }
        
        public static List<coordinate> turnCountersForPlayer(ref Board board, int row, int column, bool flip)
        {
            List<coordinate> discsToTurn = new List<coordinate>();
            List<coordinate> tempDiscsToTurn = new List<coordinate>();

            //check horizontal right:
            if((column + 2) < 7)
            {
                if (BoardState[row, column + 1] == "W")
                {
                    tempDiscsToTurn.Add(new coordinate(row, (column + 1)));
                    for (int i = column + 2; i < 7; i++)
                    {
                        if (BoardState[row, i] == "W")
                        {
                            tempDiscsToTurn.Add(new coordinate(row, i));
                            //add them to temporary list
                        }
                        if(BoardState[row, i] == "B")
                        {
                            foreach(var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            i = 8;
                        }
                        if (i < 8)
                        {
                            if (BoardState[row, i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }

            //checked horizontal right

            //check horizontal left

            //flip counters
            if(flip)
            {
                BoardState[row, column] = "B";
                foreach (var item in discsToTurn)
                {
                    BoardState[item.row, item.column] = "B";
                }
            }
            return discsToTurn;

        }

        public static bool isFull(ref Board board)
        {
            return false;
        }


        public void setUpBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    BoardState[i, a] = " ";
                }
            }
            BoardState[3, 3] = "W";
            BoardState[4, 4] = "W";
            BoardState[4, 3] = "B";
            BoardState[3, 4] = "B";

        }

        public void displayBoard()
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("    - 1 --- 2 --- 3 --- 4 --- 5 --- 6 --- 7 --- 8 --");
            Console.WriteLine(" 1 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 0], BoardState[0, 1], BoardState[0, 2], BoardState[0, 3], BoardState[0, 4], BoardState[0, 5], BoardState[0, 6], BoardState[0, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 2 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[1, 0], BoardState[1, 1], BoardState[1, 2], BoardState[1, 3], BoardState[1, 4], BoardState[1, 5], BoardState[1, 6], BoardState[1, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 3 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[2, 0], BoardState[2, 1], BoardState[2, 2], BoardState[2, 3], BoardState[2, 4], BoardState[2, 5], BoardState[2, 6], BoardState[2, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 4 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[3, 0], BoardState[3, 1], BoardState[3, 2], BoardState[3, 3], BoardState[3, 4], BoardState[3, 5], BoardState[3, 6], BoardState[3, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 5 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[4, 0], BoardState[4, 1], BoardState[4, 2], BoardState[4, 3], BoardState[4, 4], BoardState[4, 5], BoardState[4, 6], BoardState[4, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 6 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[5, 0], BoardState[5, 1], BoardState[5, 2], BoardState[5, 3], BoardState[5, 4], BoardState[5, 5], BoardState[5, 6], BoardState[5, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 7 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[6, 0], BoardState[6, 1], BoardState[6, 2], BoardState[6, 3], BoardState[6, 4], BoardState[6, 5], BoardState[6, 6], BoardState[6, 7]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 8 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[7, 0], BoardState[7, 1], BoardState[7, 2], BoardState[7, 3], BoardState[7, 4], BoardState[7, 5], BoardState[7, 6], BoardState[7, 7]);
            Console.WriteLine("    ------------------------------------------------");
            
        }

    }
}
