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

            public coordinate(int setColumn, int setRow)
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

        public void placeCounter(bool PlayerTurn, int row, int column)
        {
            if(PlayerTurn)
            {
                BoardState[row, column] = "B";
            }
            else BoardState[row, column] = "W";

        }

        public  bool checkValidMove(ref Board board, bool PlayerTurn, int row, int column)
        {


            return true;
        }
        
        public static List<coordinate> turnCounters(ref Board board, int row, int column, bool flip)
        {
            List<coordinate> discsToTurn = new List<coordinate>();

            //check horizontal right:
            if((column + 2) < 7)
            {
                for(int i = column + 2; i < 7; i++)
                {
                    if(BoardState[i, row] == "B")
                    {
                        for(int a = column + 1; a < i; a++)
                        {
                            discsToTurn.Add(new coordinate(a, row));
                        }
                        
                    }
                    if(BoardState[i, row] == " ")
                    {
                        i = 8;
                    }
                }
            }
            //checked horizontal right

            if(flip)
            {
                foreach(var item in discsToTurn)
                {
                    BoardState[item.column, item.row] = "B";
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
            Console.WriteLine(" 1 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 0], BoardState[1, 0], BoardState[2, 0], BoardState[3, 0], BoardState[4, 0], BoardState[5, 0], BoardState[6, 0], BoardState[7, 0]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 2 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 1], BoardState[1, 1], BoardState[2, 1], BoardState[3, 1], BoardState[4, 1], BoardState[5, 1], BoardState[6, 1], BoardState[7, 1]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 3 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 2], BoardState[1, 2], BoardState[2, 2], BoardState[3, 2], BoardState[4, 2], BoardState[5, 2], BoardState[6, 2], BoardState[7, 2]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 4 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 3], BoardState[1, 3], BoardState[2, 3], BoardState[3, 3], BoardState[4, 3], BoardState[5, 3], BoardState[6, 3], BoardState[7, 3]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 5 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 4], BoardState[1, 4], BoardState[2, 4], BoardState[3, 4], BoardState[4, 4], BoardState[5, 4], BoardState[6, 4], BoardState[7, 4]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 6 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 5], BoardState[1, 5], BoardState[2, 5], BoardState[3, 5], BoardState[4, 5], BoardState[5, 5], BoardState[6, 5], BoardState[7, 5]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 7 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 6], BoardState[1, 6], BoardState[2, 6], BoardState[3, 6], BoardState[4, 6], BoardState[5, 6], BoardState[6, 6], BoardState[7, 6]);
            Console.WriteLine("    ------------------------------------------------");
            Console.WriteLine(" 8 |  {0}  |  {1}  |  {2}  |  {3}  |  {4}  |  {5}  |  {6}  |  {7}  |", BoardState[0, 7], BoardState[1, 7], BoardState[2, 7], BoardState[3, 7], BoardState[4, 7], BoardState[5, 7], BoardState[6, 7], BoardState[7, 7]);
            Console.WriteLine("    ------------------------------------------------");
            
        }

    }
}
