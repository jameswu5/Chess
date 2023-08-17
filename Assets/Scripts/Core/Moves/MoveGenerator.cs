using System;
using System.Collections.Generic;

public class MoveGenerator
{


    bool inCheck;
    bool inDoubleCheck;

    ulong checkRayMask;
    ulong pinRays;

    int heroColour;
    int opponentColour;
    int heroIndex;
    int opponentIndex;
    int heroKingIndex;
    int opponentKingIndex;

    ulong hero;
    ulong opponent;
    ulong allPieces;
    ulong emptySquares;

    ulong opponentAttacks;

    Board board;

    public List<int> GenerateMoves(Board board)
    {
        List<int> moves = new();
        this.board = board;

        Initialise();


        return moves;
    }



    void Initialise()
    {
        inCheck = false;
        inDoubleCheck = false;
        checkRayMask = 0ul;
        pinRays = 0ul;


        heroColour = board.turn;
        opponentColour = board.opponentColour;
        heroIndex = board.GetColourIndex(board.turn);
        opponentIndex = board.GetOpponentColourIndex(board.turn);
        heroKingIndex = board.kingIndices[heroIndex];
        opponentKingIndex = board.kingIndices[opponentIndex];

        // bitboards of the player and opponent's pieces
        hero = board.colourBitboards[heroIndex];
        opponent = board.colourBitboards[opponentIndex];
        allPieces = board.AllPiecesBiboard;
        emptySquares = ~allPieces;

    }


    // Pseudolegal moves

    // hero -> bitboard of pieces of your own piece
    // opponent -> bitboard of pieces of opponent's piece

    public static HashSet<int> GetSlideMoves(int index, int pieceType, ulong hero, ulong opponent, int[] boardState)
    {
        HashSet<int> legalMoves = new();

        if (pieceType == Piece.Rook || pieceType == Piece.Queen)
        {
            // first 4 are orthogonal directions
            for (int i = 0; i < 4; i++)
            {
                int direction = Direction.directions[i];
                ulong rayAttacks = Bitboard.GetRayAttacks(hero, opponent, direction, index);
                foreach (int target in Bitboard.GetIndicesFromBitboard(rayAttacks))
                {
                    legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Rook, Piece.GetPieceType(boardState[target])));
                }
            }
        }
        if (pieceType == Piece.Bishop || pieceType == Piece.Queen)
        {
            // last 4 are diagonal directions
            for (int i = 4; i < 8; i++)
            {
                int direction = Direction.directions[i];
                ulong rayAttacks = Bitboard.GetRayAttacks(hero, opponent, direction, index);
                foreach (int target in Bitboard.GetIndicesFromBitboard(rayAttacks))
                {
                    legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Rook, Piece.GetPieceType(boardState[target])));
                }
            }

        }
        return legalMoves;
    }

    public static HashSet<int> GetKnightMoves(int index, ulong hero, int[] boardState)
    {
        HashSet<int> legalMoves = new();

        ulong knightAttacks = Data.KnightAttacks[index] & ~hero;

        foreach (int target in Bitboard.GetIndicesFromBitboard(knightAttacks))
        {
            legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Knight, Piece.GetPieceType(boardState[target])));
        }

        return legalMoves;
    }

    public static HashSet<int> GetKingMoves(int index, ulong hero, int castlingRights, int[] boardState)
    {
        HashSet<int> legalMoves = new();
        ulong kingAttacks = Data.KingAttacks[index] & ~hero;
        bool pieceIsWhite = Piece.IsColour(boardState[index], Piece.White);

        foreach (int target in Bitboard.GetIndicesFromBitboard(kingAttacks))
        {
            legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.King, Piece.GetPieceType(boardState[target])));
        }

        // Castling

        if (pieceIsWhite && index == Square.e1) // king is in original position
        {
            if ((castlingRights & 0b1000) > 0 && boardState[Square.h1] == Piece.White + Piece.Rook
                && boardState[Square.f1] == Piece.None && boardState[Square.g1] == Piece.None)
            {
                // can castle kingside
                legalMoves.Add(Move.Initialise(Move.Castling, index, index + 2, Piece.King, Piece.None));
            }
            if ((castlingRights & 0b0100) > 0 && boardState[Square.a1] == Piece.White + Piece.Rook
                && boardState[Square.b1] == Piece.None && boardState[Square.c1] == Piece.None && boardState[Square.d1] == Piece.None)
            {
                // can castle queenside
                legalMoves.Add(Move.Initialise(Move.Castling, index, index - 2, Piece.King, Piece.None));

            }
        }
        else if (!pieceIsWhite && index == 60)
        {
            if ((castlingRights & 0b0010) > 0 && boardState[Square.h8] == Piece.Black + Piece.Rook
                && boardState[Square.f8] == Piece.None && boardState[Square.g8] == Piece.None)
            {
                legalMoves.Add(Move.Initialise(Move.Castling, index, index + 2, Piece.King, Piece.None));

            }
            if ((castlingRights & 0b0001) > 0 && boardState[Square.a8] == Piece.Black + Piece.Rook
                && boardState[Square.b8] == Piece.None && boardState[Square.c8] == Piece.None && boardState[Square.d8] == Piece.None)
            {
                legalMoves.Add(Move.Initialise(Move.Castling, index, index - 2, Piece.King, Piece.None));
            }
        }

        return legalMoves;
    }

    public static HashSet<int> GetPawnMoves(int index, int colourIndex, ulong hero, ulong opponent, int[] boardState, int enPassantTarget)
    {
        HashSet<int> legalMoves = new();

        // pushes
        int direction = colourIndex == 0 ? 8 : -8;

        ulong pawnPushes = Data.PawnPushes[colourIndex][index] & ~hero & ~opponent;
        ulong pawnAttacks = Data.PawnAttacks[colourIndex][index] & opponent;

        bool promote = ((pawnPushes | pawnAttacks) & (Bitboard.Rank1 | Bitboard.Rank8)) > 0;

        foreach (int target in Bitboard.GetIndicesFromBitboard(pawnPushes))
        {
            if (Math.Abs(target - index) == 16)
            {
                if (boardState[index + direction] == Piece.None)
                {
                    legalMoves.Add(Move.Initialise(Move.PawnTwoSquares, index, target, Piece.Pawn, Piece.None));
                }
            }
            else if (promote)
            {
                legalMoves.Add(Move.Initialise(Move.PromoteToBishop, index, target, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToKnight, index, target, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToQueen, index, target, Piece.Pawn, Piece.None));
                legalMoves.Add(Move.Initialise(Move.PromoteToRook, index, target, Piece.Pawn, Piece.None));
            }
            else
            {
                legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Pawn, Piece.None));
            }
        }

        // captures

        foreach (int target in Bitboard.GetIndicesFromBitboard(pawnAttacks))
        {

            // Problem:
            // If I include the en passant code further down, boardState[target] can hold Piece.None and still make it to this
            // part of the code. As a result you can move a pawn diagonally without capturing. I am suspicious this is because I make every move
            // possible to check if the king is attacked.

            // Here is a plaster solution where I just enforce that there must be a piece at that square. Hopefully when I rewrite
            // legal move generation this else if statement (*) can be removed and replaced with just else


            if (promote)
            {
                legalMoves.Add(Move.Initialise(Move.PromoteToBishop, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                legalMoves.Add(Move.Initialise(Move.PromoteToKnight, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                legalMoves.Add(Move.Initialise(Move.PromoteToQueen, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
                legalMoves.Add(Move.Initialise(Move.PromoteToRook, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));

            }
            else if (boardState[target] != Piece.None) // (*)
            {
                legalMoves.Add(Move.Initialise(Move.Standard, index, target, Piece.Pawn, Piece.GetPieceType(boardState[target])));
            }

        }

        // en passant

        if (enPassantTarget != -1 && (Data.PawnAttacks[colourIndex][index] & (1ul << enPassantTarget)) > 0)
        {
            legalMoves.Add(Move.Initialise(Move.EnPassant, index, enPassantTarget, Piece.Pawn, Piece.Pawn));
        }

        return legalMoves;
    }


    void GetAttackData()
    {
        // get the attack map / coverage of the opponent for sliding pieces
        opponentAttacks = GetAttackBitboardSlidingPieces(opponentIndex);

        // look for pinned pieces

        int start = 0;
        int end = 8;

        // adjust the indices to save some time if they don't have specific pieces
        if (board.GetPieceBitboard(Piece.Queen, opponentIndex) == 0)
        {
            start = board.GetPieceBitboard(Piece.Rook, opponentIndex) == 0 ? 4 : 0;
            end = board.GetPieceBitboard(Piece.Bishop, opponentIndex) == 0 ? 4 : 8;
        }

        for (int d = start; d < end; d++)
        {
            // if i < 4, then i represents a orthogonal direction
            bool isOrthogonal = d < 4;
            ulong slider = isOrthogonal ? board.GetOrthogonalSlidingBitboard(opponentIndex) : board.GetDiagonalSlidingBitboard(opponentIndex);

            // check the ray starting from the king and see if it intersects with the slider bitboard
            if ((Data.RayAttacks[d][heroKingIndex] & slider) == 0) continue;

            int offset = Direction.directions[d];
            bool foundHeroPiece = false;
            ulong rayMask = 0ul;

            for (int i = 1; i <= Data.SquaresFromEdge[d, heroKingIndex]; i++)
            {
                int newIndex = heroKingIndex + offset * i;
                rayMask |= 1ul << newIndex;
                int piece = board.GetPieceAtIndex(newIndex);

                if (piece == Piece.None) continue;

                // our piece
                if (Piece.IsColour(piece, heroColour))
                {
                    // only the first piece of ours that we find along the ray can be pinned
                    if (!foundHeroPiece)
                        foundHeroPiece = true;
                    else
                        break;
                }
                // enemy piece
                else
                {
                    int pieceType = Piece.GetPieceType(piece);
                    // check if the pieceType can slide in the direction we are looking at
                    if ((isOrthogonal && (pieceType == Piece.Rook || pieceType == Piece.Queen)) || (!isOrthogonal && (pieceType == Piece.Bishop || pieceType == Piece.Queen)))
                    {
                        // one of our pieces is blocking the attack
                        if (foundHeroPiece)
                            pinRays |= rayMask;

                        // no pieces blocking the attack so we are in check
                        else
                        {
                            checkRayMask |= rayMask;
                            inDoubleCheck = inCheck;
                            inCheck = true;
                        }
                    }

                    // we don't need to look any further as we have found a piece
                    break;
                    
                }
            }

            // if we are in double check, we can only move the king so no need to search further
            if (inDoubleCheck)
                break;
        }

    }

    ulong GetAttackBitboardSlidingPieces(int colourIndex)
    {
        ulong attacks = 0ul;
        attacks |= GetSlidingAttacks(board.GetDiagonalSlidingBitboard(colourIndex), false);
        attacks |= GetSlidingAttacks(board.GetOrthogonalSlidingBitboard(colourIndex), true);

        return attacks;
    }

    ulong GetSlidingAttacks(ulong pieceBoard, bool orthogonal)
    {
        ulong attacks = 0ul;
        foreach (int index in Bitboard.GetIndicesFromBitboard(pieceBoard))
        {
            // we want to be able to x-ray the king so we don't consider it a blocker
            ulong blockers = allPieces & ~(1ul << heroKingIndex);

            int offset = orthogonal ? 0 : 4;
            for (int i = offset; i < offset + 4; i++)
            {
                attacks |= Bitboard.GetRayAttacks(hero, opponent, Direction.directions[i], index);
            }
        }

        return attacks;
    }
}
