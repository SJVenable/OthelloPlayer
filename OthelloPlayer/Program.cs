using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloPlayer
{
    class Program
    {



        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Othello Player!");
            //set up board with starting discs

            Board gameBoard = new Board();

            //returns a menu option which sends you down one of three switch statements.
            switch (displayMenu())
            {
                case 1:
                    Console.Clear();
                    gameBoard.displayBoard();
                    playGame(ref gameBoard);
                    break;
                case 2:
                    Console.Clear();
                    Console.WriteLine(" Difficulties:");
                    Console.ReadKey();
                    break;
                case 3:
                    Console.Clear();
                    Console.WriteLine("Thanks for Playing!");
                    break;

            }


        }

        public static void playGame(ref Board board)
        {
            bool PlayerTurn = true;
            int row;
            int column;
            while (!Board.isFull(ref board))
            {
                if (PlayerTurn)
                {

                    Console.WriteLine("Where would you like to place your counter?");

                    Console.Write("Enter column: ");
                    column = int.Parse(Console.ReadLine()) - 1;
                    Console.Write("Enter row: ");
                    row = int.Parse(Console.ReadLine()) - 1;
                    if (board.checkValidMove(ref board, PlayerTurn, row, column))
                    {
                        board.placeCounter(ref board, true, row, column);
                    }

                    Console.Clear();
                    board.displayBoard();
                    Console.WriteLine("AI's turn... ");
                    PlayerTurn = false;
                    Console.Write("Enter column: ");
                    column = int.Parse(Console.ReadLine()) - 1;
                    Console.Write("Enter row: ");
                    row = int.Parse(Console.ReadLine()) - 1;
                    if (board.checkValidMove(ref board, PlayerTurn, row, column))
                    {
                        board.placeCounter(ref board, PlayerTurn, row, column);
                    }
                    Console.ReadKey();

                }

                // else call ai move
            }

        }

        }

        public static int displayMenu()
        {
            Console.WriteLine(" > Play Game");
            Console.WriteLine("   Select diffuculty");
            Console.WriteLine("   Exit");

            ConsoleKeyInfo choice = Console.ReadKey(true);
            int option = 1;
            while (choice.Key != ConsoleKey.Enter)
            {

                if (choice.Key == ConsoleKey.UpArrow && option > 1)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = option;
                    Console.Write("  ");
                    Console.CursorTop = option - 1;
                    Console.CursorLeft = 0;
                    option--;
                    Console.Write(" >");
                }
                if (choice.Key == ConsoleKey.DownArrow && option < 3)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = option;
                    Console.Write("  ");
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    option++;
                    Console.Write(" >");
                }

                choice = Console.ReadKey(true);

            }
            return option;


        }
        
    }
}
