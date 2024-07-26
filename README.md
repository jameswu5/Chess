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

### Pseudolegal move generation

Pseudolegal move generation is relatively straightforward for non-sliding pieces (i.e. knight, king and pawn). We can retrieve the attack map from the relevant cell in the lookup table, and remove a square if that square is already occupied by a friendly piece. This is where bitboards of our pieces comes in handy, because it becomes an AND operation. Take this as an example:

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/b93c5af2-a2a3-4bce-a3e8-a118e5124a4b" style="width: 350px;">
      <p>The squares the white knight attacks</p>
    </td>
    <td style="align: center; vertical-align: top; padding-left: 60px;">
      <img src="https://github.com/user-attachments/assets/700c617a-f8a3-4c7c-80dd-a996fc224ca5" style="width: 350px;">
      <p>The squares occupied by a friendly piece</p>
    </td>
  </tr>
</table>

The first board is all the squares the knight attacks, and this can be found by the lookup table. The second board is all the squares our friendly pieces occupy. So if we perform `knightAttacks & ~whitePieces` we will get all the squares the knight can land on.


<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/3d17c8bc-4a7b-4543-acb5-5327dc993b52" style="width: 350px;">
      <p>The squares the white knight can land on</p>
    </td>
  </tr>
</table>

The idea is similar for sliding pieces, but we introduce the idea of blockers. If there is a piece in the path of the direction, we must truncate the ray there. If that piece is an enemy piece, we include that square. This can, again, be done by an AND operation.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/7b02bc0b-23ad-4ab1-8209-ace87c433931" style="width: 350px;">
      <p>The attacks in the SW direction from f7</p>
    </td>
    <td style="align: center; vertical-align: top; padding-left: 60px;">
      <img src="https://github.com/user-attachments/assets/e2d7692d-b1f4-430f-bfad-baddf1be4908" style="width: 350px;">
      <p>The bitboard of where all pieces occupy</p>
    </td>
  </tr>
</table>

Performing the AND operation of the two boards above gets the blockers.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/96167c31-e0f3-4318-9a61-4aec3a050a9e" style="width: 350px;">
      <p>The blockers in the SW ray starting at f7</p>
    </td>
  </tr>
</table>

We can run a forward bitscan (or reverse bitscan, depending on whether the direction is a left shift or a right shift) to find the first blocker. In this case, since our direction is SW which is a right shift, we want to find the position of the most significant bit that is a 1. This is at d5 (index 35). When we do that, we can look up the attack ray from d5 in the same direction (we want to remove all the squares in the same direction beyond the blocker).

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/87edf720-2a82-4bb4-98bd-37f815f21fa6" style="width: 350px;">
      <p>The attacks in the SW direction from d5</p>
    </td>
    <td style="align: center; vertical-align: top; padding-left: 60px;">
      <img src="https://github.com/user-attachments/assets/f35ce7aa-eca7-4c11-b3b7-c0134ff9c9b9" style="width: 350px;">
      <p>The attacks in the SW direction from f7</p>
    </td>
  </tr>
</table>

The XOR operation is convenient here because it sets a bit to 1 only if the two inputs are different. Therefore we get by performing the XOR on the above two boards.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/2e746f14-1a60-4cbe-b2a1-3dbd35a42481" style="width: 350px;">
      <p>The squares the black bishop can move to</p>
    </td>
  </tr>
</table>

All that remains is to check whether our blocker is a friendly piece or not. If it is friendly, we remove that square from our available squares. In this case, our blocker is an enemy piece so we are done.

```csharp
// hero: bitboard of friendly pieces
// opponent: bitboard of opponent pieces
public static ulong GetRayAttacks(ulong hero, ulong opponent, int dirIndex, int squareIndex)
{
    ulong attacks = Data.RayAttacks[dirIndex][squareIndex];
    ulong blockers = attacks & (hero | opponent);

    if (blockers > 0) {
        int blocker = Direction.directions[dirIndex] > 0 ? BitScanForward(blockers) : BitScanReverse(blockers);
        ulong block = ShiftLeft(1ul, blocker);
        
        attacks ^= Data.RayAttacks[dirIndex][blocker];

        // if the blocker is my own piece then clear that square
        if ((block & hero) > 0) {
            attacks &= ~block;
        }
    }

    return attacks;
}
```

Of course we need to do this for all 4 directions of the bishop, but it can be done with a simple for loop iterating through the directions.

### Legal move generation

Of course we must find a way to remove moves that leads the the king being able to be captured. The naive method (and my first approach) is implementing an undo move function and I iterate through all possible moves and check if the king is attacked after making that move. If so, our move must be illegal. This, however, is incredibly inefficient because you are technically doing a 2-ply search on every move.

To generate legal moves, we need the following observations:

- There are three ways of escaping check: by moving the king, blocking the check or by capturing the piece giving the check. If we are in double check (king is attacked by 2 pieces), then the only way of escaping check is by moving the king.
- We may not move pinned pieces away from the ray in which is being pinned in.
- We may not move our king into a square the opponent attacks.

Given these observations, it would be useful to keep track of the rays in which the pins are taking place, the squares the opponent attacks, and a **checkmask**.

Checkmasks are very useful because we can filter out illegal moves with the AND operation.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/5d11b3ae-df16-4aa4-8420-e8552689002d" style="width: 350px;">
      <p>Checkmask between the king and queen</p>
    </td>
    <td style="align: center; vertical-align: top; padding-left: 60px;">
      <img src="https://github.com/user-attachments/assets/218b458e-302c-4f7e-a53f-2f87a27447d8" style="width: 350px;">
      <p>The rook's vertical pseudolegal moves</p>
    </td>
  </tr>
</table>

When we AND these two bitboards we obtain

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/8afcb56b-10c0-4bc7-a729-4d6c4100e156" style="width: 350px;">
      <p>The rook can only move to e3</p>
    </td>
  </tr>
</table>

which is the only square the rook can reach. This is how we can use checkmasks to determine how to block the check.

Pins occur when the an opponent sliding piece is not attacking the king, but would be if a friendly piece moved out of the way. This can also be known as an X-Ray attack.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/78fbd9e9-85e6-4696-8d2c-a28beb0d34ac" style="width: 350px;">
      <p>The queen is eyeing the king through the bishop</p>
    </td>
    <td style="align: center; vertical-align: top; padding-left: 60px;">
      <img src="https://github.com/user-attachments/assets/3ec0b0b9-d07e-4b85-bc28-8c0d052887d8" style="width: 350px;">
      <p>The bishops's pseudolegal moves</p>
    </td>
  </tr>
</table>

Again, performing the AND operation will grant the legal moves for the pinned bishop.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/bc656e8f-cfbf-4593-a716-85db3e1c36ed" style="width: 350px;">
      <p>The squares the pinned bishop can move to</p>
    </td>
  </tr>
</table>

You can see that in both cases the enemy piece is included in the legal moves which encompasses capturing the piece when in check as well.

Now to see where the king can move to, we need to identify which squares the opponent attacks. Note this is different to which squares the opponent can reach, because the pawn is a special case. It can move forward, but it doesn’t attack the square in front of it.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/0d274190-fefb-4fc6-b6c6-6dceafca9741" style="width: 350px;">
      <p>The squares that black attacks</p>
    </td>
    <td style="align: center; vertical-align: top; padding-left: 60px;">
      <img src="https://github.com/user-attachments/assets/5db60382-156d-40e1-81b1-99862d4cbeab" style="width: 350px;">
      <p>The white king's pseudolegal moves</p>
    </td>
  </tr>
</table>

We can AND the complement of opponent’s attacks with our king’s squares to get the squares that the king can move to legally.

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/a0f48372-f442-466b-b766-eb6ef8f716d2" style="width: 350px;">
      <p>The squares the white king can move to</p>
    </td>
  </tr>
</table>

Note that the square in front of the pawn (d4) is allowed because while the pawn is able of moving there, it doesn’t attack that square (if the king moved to d4, the pawn would be obstructed).

### En passant

The move to be careful of is en passant because there is one special case:

<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/59f83c4e-4d89-427e-8780-cf13339012c3" style="width: 350px;">
      <p>The pawn can be captured exposing the king</p>
    </td>
  </tr>
</table>

Here the pawn has moved to f4 and is able to be captured by the black pawn on g4 by en passant. Based on my implementation, the black pawn is not considered to be pinned because there are two pieces between the white rook and the black king. Therefore, the en passant would be allowed, even though it is illegal as it exposes an attack on the king. This is the only case where two pieces can be removed from one ray, so we can make an exceptional check for this.

We observe that this can only happen in orthogonal directions so we only need to check for rooks and queens. We can calculate the orthogonal piece’s attacks (in this case the white rook) while ignoring the two pawns as blockers. We can see the king will be attacked so we disallow the move.


<table align="center">
  <tr>
    <td style="align: center; vertical-align: top;">
      <img src="https://github.com/user-attachments/assets/ad0baa4c-d377-4b60-b1ea-3ec5e3684159" style="width: 350px;">
      <p>The rook's attacks after ignoring</p>
    </td>
  </tr>
</table>

## Zobrist Hashing

In my first version, I used the FEN string of the current position to keep track of game state which I used to detect threefold repetiton. This, however, is slow because we have to generate the FEN string every time we play a move (whether as a player or in search) which leads to a lot of repeated work, as for example all the black pieces are unmoved when white makes the first move. Zobrist hashing is a way of efficiently hashing the game state and what is nice about it is that you don’t have to regenerate the hash every time you make a move.

In Zobrist hashing, we make use of the XOR operation to  represent a gamestate with a 64-bit unsigned integer (`ulong`). This is a hash therefore it is impossible to recover the original gamestate from the hash alone without bruteforcing, therefore this cannot replace the bitboard method. We can uniquely identify the game state with the following information:

* The location of where the pieces are on the board
  - We can assign a `ulong` to each possible piece in each possible location. This means we will need a 12 x 64 array (12 pieces, 64 squares) but for ease of how we modelled our pieces we can use a 16 x 64 array. Each of these will contain a different `ulong`. For example, if there is a white knight (id 4) on g1 (index 6), we can find the `ulong` associated with this information with `pieceKeys[4, 6]`.
* The castling rights
  - We model castling rights with a 4-bit integer (nibble) because we only need to keep track of the right to kingside and queenside castle for both black and white. It is evident that we will only need an array of size 16 and we can just access the value with `castlingKeys[castlingRights]`, where `castlingRights` stores the current castling rights, e.g. `0b1100` when white can castle both ways but black cannot castle either way.
* En passant file
  - There are 9 cases: either there is no en passant target available or there is one, and it could be in any of the 8 files. So we can have an array of size 9.
* Player to move
  - We will just need a single `ulong` for this, and we will activate it when it is white’s turn to move.

All of these numbers in the array are generated pseudo-randomly.

Now to calculate the hash, what we can do is

1. Initialise our zobrist key `zobristKey` to be 0.
2. Iterate through every square on the board, and say we have piece `piece` on square `sq`, we can perform `zobristKey ^= pieceKeys[piece, sq]`.
3. Perform `zobristKey ^= castlingKeys[castlingRights]`
4. Identify the file of our en passant target and set it to 0 if there isn’t one. Perform `zobristKey ^= enPassantKeys[file]`.
5. If it is white’s turn to move, perform `zobristKey ^= turnKey`.

Here’s the code:

```csharp
public static ulong CalculateKey(Board board)
    {
        ulong key = 0;

        for (int sq = 0; sq < 64; sq++)
        {
            int piece = board.GetPieceAtIndex(sq);
            if (piece != Piece.None)
            {
                key ^= pieceKeys[piece, sq];
            }
        }

        key ^= castlingKeys[board.castlingRights];

        key ^= enPassantKeys[GetTargetFile(board.enPassantTarget)];

        if (board.turn == Piece.White)
        {
            key ^= turnKey;
        }

        return key;
    }
```

What is nice about this is we don’t need to call `CalculateKey` every time we make a move. Because only limited information is changed, we can make use of the self-inverse property of XOR to modify our Zobrist key directly.

The self-inverse property of XOR proves to be very useful, because we can add a piece `piece` to a square `sq` to the board with `zobristKey ^= pieceKeys[piece, sq]`, which is the same as removing the piece from the board. So I write a simple toggle function that allows you to add / remove a piece:

```csharp
public static void TogglePiece(ref ulong key, int piece, int square)
{
    key ^= pieceKeys[piece, square];
}
```

So for example, if we want to move a knight (id 4) from b1 (index 1) to c3 (index 18), we can just perform the following

```csharp
key ^= pieceKeys[4, 1]; // remove knight from b1
key ^= pieceKeys[4, 18]; // add knight to c3
```

If we are capturing on c3, say a black knight (id 12), we can simply remove the piece with the same XOR operation.

```csharp
key ^= pieceKeys[12, 18];
```

For castling rights, we can simply perform this

```csharp
key ^= castlingKeys[oldCastlingRights];
key ^= castlingKeys[newCastlingRights];
```

Conveniently, due to the self-inverse nature of XOR, it also works if the castling rights are unchanged.

Similarly for en passant files, we can perform

```csharp
key ^= enPassantKeys[oldEnPassantFile];
key ^= enPassantKeys[newEnPassantFile];
```

And since we change turns every move, we do

```csharp
key ^= turnKey;
```

And that’s it! We can cache these Zobrist keys with a stack so when we undo a move, we can simply pop the stack and set the new Zobrist key to be the key on the top of the stack.

You don’t need to recalculate the key with every move as explained above, but we need to consider the possibility of collisions, which is when two different positions have the same key. Now hash collisions demonstrate the [birthday paradox](https://en.wikipedia.org/wiki/Birthday_problem) which means the chance of collisions approaches certainty at around the square root of the number of possible keys. We use 64 bits so we can ‘expect’ a collision at around $2^{32}$ positions, around 4 billion.
