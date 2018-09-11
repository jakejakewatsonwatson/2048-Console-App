using System;
using System.Collections.Generic;

namespace ConsoleApp25
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Run(); //Runs our game
        }
        
        class Game
        {
            public ulong Score { get; private set; }
            public ulong[,] Board { get; private set; }

            private int Rows;
            private int Cols;
            private Random random = new Random();

            public Game()
            {
                Board = new ulong[4, 4]; //Gives the dimensions of our game board, original 2048 was 4x4
                Rows = Board.GetLength(0);
                Cols = Board.GetLength(1);
                Score = 0;
            }

            public void Run()
            {
                bool hasUpdated = true;
                do
                {
                    if (hasUpdated)
                    {
                        PutNewValue();
                    }

                    Display();

                    if (IsDead())
                    {
                        using (new ColorOutput(ConsoleColor.Red))
                        {
                            Console.WriteLine("GIT GUD"); //Unfunny meme
                         
                        }
                        Console.WriteLine("Would you like to play again? y/n");
                        string userInput = Console.ReadLine();

                        if(userInput == "y")
                        {
                            Game game = new Game();
                            game.Run(); //Runs our game again
                        }
                        break;
                    }

                    Console.WriteLine("Use arrow keys to move the tiles. Press Ctrl-C to exit.");
                    ConsoleKeyInfo input = Console.ReadKey(true); // Checking input to make sure it's a directional arrow
                    Console.WriteLine(input.Key.ToString());

                    switch (input.Key) //Converts input keys to directional commands
                    {
                        case ConsoleKey.UpArrow:
                            hasUpdated = Update(Direction.Up);
                            break;

                        case ConsoleKey.DownArrow:
                            hasUpdated = Update(Direction.Down);
                            break;

                        case ConsoleKey.LeftArrow:
                            hasUpdated = Update(Direction.Left);
                            break;

                        case ConsoleKey.RightArrow:
                            hasUpdated = Update(Direction.Right);
                            break;

                        default:
                            hasUpdated = false;
                            break;
                    }
                }
                while (true); // use CTRL-C to break out of loop

                Console.WriteLine("Press any key to quit...");
                Console.Read();
            }

            private static ConsoleColor GetNumberColor(ulong num)
            {
                switch (num) //There's no 2048 number color because I don't think anyone is good enough to make it there 
                {
                    case 0:
                        return ConsoleColor.DarkGray;
                    case 2:
                        return ConsoleColor.Cyan;
                    case 4:
                        return ConsoleColor.Magenta;
                    case 8:
                        return ConsoleColor.Red;
                    case 16:
                        return ConsoleColor.Green;
                    case 32:
                        return ConsoleColor.Yellow;
                    case 64:
                        return ConsoleColor.Yellow;
                    case 128:
                        return ConsoleColor.DarkCyan;
                    case 256:
                        return ConsoleColor.Cyan;
                    case 512:
                        return ConsoleColor.DarkMagenta;
                    case 1024:
                        return ConsoleColor.Magenta;
                    default:
                        return ConsoleColor.Red;
                }
            }

            private static bool Update(ulong[,] board, Direction direction, out ulong score)
            {
                int nRows = board.GetLength(0);
                int nCols = board.GetLength(1);

                score = 0;
                bool hasUpdated = false;

                // You shouldn't be dead at this point. We always check if you're dead at the end of the Update()

                // Drop along row or column? true: process inner along row; false: process inner along column
                bool isAlongRow = direction == Direction.Left || direction == Direction.Right;

                // Should we process inner dimension in increasing index order?
                bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

                int outterCount = isAlongRow ? nRows : nCols;
                int innerCount = isAlongRow ? nCols : nRows;
                int innerStart = isIncreasing ? 0 : innerCount - 1;
                int innerEnd = isIncreasing ? innerCount - 1 : 0;

                Func<int, int> drop = isIncreasing
                    ? new Func<int, int>(innerIndex => innerIndex - 1)
                    : new Func<int, int>(innerIndex => innerIndex + 1);

                Func<int, int> reverseDrop = isIncreasing
                    ? new Func<int, int>(innerIndex => innerIndex + 1)
                    : new Func<int, int>(innerIndex => innerIndex - 1);

                Func<ulong[,], int, int, ulong> getValue = isAlongRow
                    ? new Func<ulong[,], int, int, ulong>((x, i, j) => x[i, j])
                    : new Func<ulong[,], int, int, ulong>((x, i, j) => x[j, i]);

                Action<ulong[,], int, int, ulong> setValue = isAlongRow
                    ? new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[i, j] = v)
                    : new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[j, i] = v);

                bool innerCondition(int index) => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

                for (int i = 0; i < outterCount; i++)
                {
                    for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                    {
                        if (getValue(board, i, j) == 0)
                        {
                            continue;
                        }

                        int newJ = j;
                        do
                        {
                            newJ = drop(newJ);
                        }
                        // Continue probing along as long as we haven't hit the boundary and the new position isn't occupied
                        while (innerCondition(newJ) && getValue(board, i, newJ) == 0);

                        if (innerCondition(newJ) && getValue(board, i, newJ) == getValue(board, i, j))
                        {
                            // We did not hit the canvas boundary (we hit a node) AND no previous merge occurred AND the nodes' values are the same
                            // Let's merge the two values
                            ulong newValue = getValue(board, i, newJ) * 2;
                            setValue(board, i, newJ, newValue);
                            setValue(board, i, j, 0);

                            hasUpdated = true;
                            score += newValue;
                        }
                        else
                        {
                            // Reached the boundary OR...
                            // we hit a node with different value OR...
                            // we hit a node with same value BUT a prevous merge had occurred that made one node equal to another
                            
                            // Simply stack along

                            newJ = reverseDrop(newJ); // reverse back to its valid position
                            if (newJ != j)
                            {
                                // there's an update
                                hasUpdated = true;
                            }

                            ulong value = getValue(board, i, j);
                            setValue(board, i, j, 0);
                            setValue(board, i, newJ, value);
                        }
                    }
                }
                return hasUpdated;
            }

            private bool Update(Direction dir)
            {
                ulong score;
                bool isUpdated = Game.Update(this.Board, dir, out score);
                Score += score;
                return isUpdated;
            }

            private bool IsDead() //This checks all possible directions to check if there are any legal moves for the player to use
            {
                ulong score;
                foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
                {
                    ulong[,] clone = (ulong[,])Board.Clone();
                    if (Update(clone, dir, out score))
                    {
                        return false;
                    }
                }

                // tried all directions, no direction can move a piece
                return true;
            }

            private void Display() //This creates our game board, by building our set rows and columns
            {
                Console.Clear();
                Console.WriteLine();
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        using (new ColorOutput(GetNumberColor(Board[i, j]))) //This checks our number at each spot on the board, and return the corresponding color
                        {
                            Console.Write(string.Format("{0,6}", Board[i, j]));
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                }

                Console.WriteLine("Score: {0}", this.Score);
                Console.WriteLine();
            }

            private void PutNewValue()
            {
                // Find all empty slots
                List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();
                for (int iRow = 0; iRow < Rows; iRow++)
                {
                    for (int iCol = 0; iCol < Cols; iCol++)
                    {
                        if (Board[iRow, iCol] == 0)
                        {
                            emptySlots.Add(new Tuple<int, int>(iRow, iCol));
                        }
                    }
                }

                // We should have at least 1 empty slot. Since we know the player is not dead
                int iSlot = random.Next(0, emptySlots.Count); // randomly pick an empty slot to place either 2 or 4 into
                ulong value = random.Next(0, 100) < 95 ? (ulong)2 : (ulong)4; // randomly pick 2 (with 95% chance) or 4 (rest of the chance)
                Board[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2] = value;
            }
            
            enum Direction
            {
                Up,
                Down,
                Right,
                Left,
            }

            class ColorOutput : IDisposable
            {
                public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
                {
                    Console.ForegroundColor = fg;
                    Console.BackgroundColor = bg;
                }

                public void Dispose()
                {
                    Console.ResetColor();
                }
            }
        }
    }
}
