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

        public static int LetterToInt(char letter)
        {
            string strLetter = letter.ToString().ToUpper();
            char UpperCaseChar = strLetter.ToCharArray()[0];

            return UpperCaseChar - (int) 'A';
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Player One (Black)'s turn...");
                    Console.ForegroundColor = ConsoleColor.White;
                    

                    if (Board.canPlaceCounter(ref board, PlayerTurn))
                    {
                        do
                        {
                            Console.WriteLine("Where would you like to place your counter?");
                            Console.Write("Enter column: ");
                            column = LetterToInt(char.Parse(Console.ReadLine()));
                            Console.Write("Enter row: ");
                            row = int.Parse(Console.ReadLine()) - 1;
                            if (!board.checkValidMove(ref board, PlayerTurn, row, column))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Cannot place here! Try again");
                                Console.ForegroundColor = ConsoleColor.White;
                            }



                        } while (!board.checkValidMove(ref board, PlayerTurn, row, column));
                        board.placeCounter(ref board, PlayerTurn, row, column);
                        Console.Clear();
                        board.displayBoard();
                    }
                    else
                    {

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Player 1 can't go anywhere! Go skipped...");
                        Console.ForegroundColor = ConsoleColor.White;
                        
                    }
                    PlayerTurn = false;
                }

                else
                {

                    if (Board.canPlaceCounter(ref board, PlayerTurn)) {


                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Player Two (White)'s turn...");
                        Console.ForegroundColor = ConsoleColor.White;

                        do
                        {

                            Console.Write("Enter column: ");
                            column = LetterToInt(char.Parse(Console.ReadLine()));
                            Console.Write("Enter row: ");
                            row = int.Parse(Console.ReadLine()) - 1;

                            if (!board.checkValidMove(ref board, PlayerTurn, row, column))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Cannot place here! Try again");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } while (!board.checkValidMove(ref board, PlayerTurn, row, column));
                        board.placeCounter(ref board, PlayerTurn, row, column);
                        Console.Clear();
                        board.displayBoard();
                        
                    }

                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Player 2 can't go anywhere! Go skipped...");
                        Console.ForegroundColor = ConsoleColor.White;
                        
                    }
                    PlayerTurn = true;

                }

                
            }
            Console.WriteLine("GAME OVER, winner was...");
            System.Threading.Thread.Sleep(3000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Board.checkWinner(ref board));

            Console.ReadKey();

        }

        public static int displayMenu()
        {
            Console.WriteLine(" > Play Game");
            Console.WriteLine("   Select difficulty");
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
