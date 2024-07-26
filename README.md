# Chess
This is my implementation of a chess GUI made with Unity. Makes use of techniques such as bitboards for move generation, and Zobrist hashing for game states.

## Optimising Move Generation

Having an efficient move generating algorithm is key to any chess engine. You want move generation to be as little a limiting factor as possible when you want to construct an engine, therefore I took extra care to optimise how my program generates moves.

The steps I took to optimise move generation can be broken down into:

- Use primitive data types as much as possible (such as `int` and `string`), instead of ‘custom-made’ objects such as `Piece` and `Move`. As a result, we aim for a more procedural approach instead of object-oriented.
- Use unsigned integers instead of 1D arrays or worse, 2D arrays to represent the current board state when computing new moves. These are known as bitboards.
- Use a hashing algorithm to represent a game state, which is preferably as fast as possible to speed up threefold repetition checking. I used Zobrist Hashing.

### Using primitive data types

In the first version of my chess program, I made use of custom objects but these are much more expensive to perform operations on, such as mutating and copying. Therefore, I transformed my `Piece` and `Move` classes into static classes.

`Piece` is relatively easy to transform because all you need to do is assign a unique key or ID to every type of piece. There are 6 chess pieces therefore it can fit into 3 bits of memory. I also store a flag for the colour of the piece, therefore each piece can be uniquely determined with 4 bits of memory. The most significant bit being a 1 if the piece is black, the other three bits determining the type of piece.

Fitting a piece in 4 bits (or a nibble) is convenient because we can easily identify the colour of the piece by checking whether `piece & 0b1000` is greater than 0 (if so the piece is black) and get the type of the piece by performing `piece & 0b0111`.

For moves, I used to have a class storing attributes such as its `startIndex`, `endIndex`, `moveType`, `movedPieceType` and `capturedPieceType`. However, we can avoid all of this entirely by encoding a move as a 32-bit integer.

```

[MovedPiece] [CapturedPiece] [MoveType] [StartIndex] [DestinationIndex]
[     3    ] [      3      ] [    3   ] [     6    ] [       6        ]
```

When we want to retrieve some information, we can right-shift `move` by the appropriate index and perform the AND operation with the associated mask. For example, if we want to find the `moveType`, we can perform `(move >> 12) & 0b111`. This encoding speeds up the program immensely because we are using a primitive data type.

It is also possible to encode it in a 16-bit integer but since I was going to use `int` which is 32-bit anyway I suppose it couldn’t hurt to store more information in the move.

```
Alternative encoding:

[MoveType] [StartIndex] [DestinationIndex]
[    4   ] [     6    ] [       6        ]
```

### Bitboards

Instead of using an array of integers to generate moves, we can use the convenient fact that there are 64 squares in a board to store information about the board using an unsigned 64-bit integer (`ulong`). However, we can only toggle bits on and off which isn’t enough states to store each type of piece. Therefore, we can have a specialised `ulong` for every piece (there are 12 of them), and for convenience we can also have a `ulong` for which squares a player is occupying. Therefore, we can store the game state with 14 `ulong`s, or 14 bitboards.

For example, for the specialised bitboard for white rooks, in the starting position we will have

```
0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0 0
1 0 0 0 0 0 0 0 1

=> 0b10000001
```

We can set the bit at the index `index` to 1 by performing

```csharp
bitboard ^= (1ul << index);
```

and we can set the bit at the index `index` to 0 by performing

```csharp
bitboard &= ~(1ul << index);
```

### Sliding pieces

As with generating moves in a 1D array, we can move one square in one direction at a position on the board with the following shifts:

```
+7   +8   +9          <<7   <<8   <<9
-1   +0   +1    =>    >>1   <<0   <<1
-9   -8   -7          >>9   >>8   >>7
```

Where `<<` represents a left-shift, and `>>` represents a right-shift. Therefore, if we want to move one square north-east from d1 (index 3, represented by the bitboard `1ul << 3`), we can do

```csharp
(1ul << 3) << 9
```

Now we need to worry about reaching the edge of the board, but we can detect whether our square is at the edge of board with masks that represent files and ranks.

```
FileA: 0x01010101
FileH: FileA << 7
Rank1: 0b11111111
Rank8: Rank1 << 56
```

Now to generate moves for each pieces, we can classify them into two categories: sliding or not sliding. If they are sliding, we can calculate the bitboard of all the squares they can reach in a certain direction from a certain square.

```
0 0 0 1 0 0 0 0
0 0 0 1 0 0 0 0
0 0 0 1 0 0 0 0
0 0 0 1 0 0 0 0
0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0
0 0 0 0 0 0 0 0
```

This can be computed with repeated OR operations while we are not looking at the edge of the board.

```csharp
public static ulong GetRayAttacksEmptyBoard(int direction, int index)
{
    ulong attacks = 0ul;
    ulong current = 1ul << index;

    while (current > 0 && !CheckAtEdgeOfBoard(direction, current))
    {
        current = ShiftLeft(current, direction);
        attacks |= current;
    }

    return attacks;
}
```

Since these operations would be calculated all the time, it would be useful to store a lookup table of the rays in each direction from every square. This would be a 2D array of size $8 \times 64$.

It is also useful to store lookup tables for all the squares a knight, pawn and king can reach from every square. The logic is similar, but without the while loop since these pieces are not sliding.


