using System;
using System.Collections.Generic;

public class MoveGenerator
{
    public bool inCheck;
    private bool inDoubleCheck;

    private ulong checkMask;
    private ulong pinRays;

    private int heroColour;
    private int heroIndex;
    private int opponentIndex;
    private int heroKingIndex;
    private int opponentKingIndex;

    private ulong hero;
    private ulong opponent;
    private ulong allPieces;
    private ulong emptySquares;

    private ulong opponentAttacks;

    private Board board;

    private bool CheckIfPinned(int index) => (pinRays & (1ul << index)) > 0;

    public List<int> GenerateMoves(Board board)
    {
        List<int> moves = new();
        this.board = board;

        Initialise();

        GenerateKingMoves(moves);

        if (!inDoubleCheck)
        {
            GenerateKnightMoves(moves);
            GenerateSlidingMoves(moves);
            GeneratePawnMoves(moves);
        }

        return moves;
    }

    private void Initialise()
    {
        inCheck = false;
        inDoubleCheck = false;
        checkMask = 0ul;
        pinRays = 0ul;

        heroColour = board.turn;
        heroIndex = board.GetColourIndex(board.turn);
        opponentIndex = board.GetOpponentColourIndex(board.turn);
        heroKingIndex = board.kingIndices[heroIndex];
        opponentKingIndex = board.kingIndices[opponentIndex];

        // bitboards of the player and opponent's pieces
        hero = board.colourBitboards[heroIndex];
        opponent = board.colourBitboards[opponentIndex];
        allPieces = board.AllPiecesBiboard;
        emptySquares = ~allPieces;

        GetAttackData();
    }

    // hero -> bitboard of pieces of your own piece
    // opponent -> bitboard of pieces of opponent's piece

    private void GetAttackData()
    {
        // sliding pieces

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
                            checkMask |= rayMask;
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

        // knights
        ulong knightAttacks = 0ul;
        ulong knights = board.GetPieceBitboard(Piece.Knight, opponentIndex);
        ulong heroKingBitboard = board.GetPieceBitboard(Piece.King, heroIndex);

        foreach (int knightSquare in Bitboard.GetIndicesFromBitboard(knights))
        {
            knightAttacks |= Data.KnightAttacks[knightSquare];
            if ((Data.KnightAttacks[knightSquare] & heroKingBitboard) > 0)
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkMask |= 1ul << knightSquare;
            }
        }

        opponentAttacks |= knightAttacks;

        // pawns
        ulong pawns = board.GetPieceBitboard(Piece.Pawn, opponentIndex);
        ulong pawnAttacks = 0ul;

        foreach (int pawnSquare in Bitboard.GetIndicesFromBitboard(pawns))
        {
            pawnAttacks |= Data.PawnAttacks[opponentIndex][pawnSquare];
            if ((Data.PawnAttacks[opponentIndex][pawnSquare] & heroKingBitboard) > 0)
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkMask |= 1ul << pawnSquare;
            }
        }

        opponentAttacks |= pawnAttacks;

        // opponent king
        opponentAttacks |= Data.KingAttacks[opponentKingIndex];

        // If not in check we have no restrictions in move choice
        if (!inCheck)
        {
            checkMask = ulong.MaxValue;
        }
    }

    private ulong GetAttackBitboardSlidingPieces(int colourIndex)
    {
        ulong attacks = 0ul;
        attacks |= GetSlidingAttacks(board.GetDiagonalSlidingBitboard(colourIndex), false);
        attacks |= GetSlidingAttacks(board.GetOrthogonalSlidingBitboard(colourIndex), true);

        return attacks;
    }

    private ulong GetSlidingAttacks(ulong pieceBoard, bool orthogonal)
    {
        ulong attacks = 0ul;
        foreach (int index in Bitboard.GetIndicesFromBitboard(pieceBoard))
        {
            int offset = orthogonal ? 0 : 4;
            for (int i = offset; i < offset + 4; i++)
            {
                // we want to be able to x-ray the king so we don't consider it a blocker
                attacks |= Bitboard.GetRayAttacks(hero, opponent, i, index, 1ul << heroKingIndex);
            }
        }

        return attacks;
    }

    private void GenerateKingMoves(List<int> moves)
    {
        ulong kingMoves = Data.KingAttacks[heroKingIndex] & ~(opponentAttacks | hero);
        foreach (int target in Bitboard.GetIndicesFromBitboard(kingMoves))
        {
            moves.Add(Move.Initialise(Move.Standard, heroKingIndex, target, Piece.King, Piece.GetPieceType(board.GetPieceTypeAtIndex(target))));
        }

        // Castling
        if (!inCheck)
        {
            ulong blockers = opponentAttacks | allPieces;

            // If we have castling rights, then it implies the king and rook are in their original position
            if (board.CanCastleKingside(heroColour))
            {
                ulong castleMask = heroColour == Piece.White ? Bitboard.WhiteKingsideMask : Bitboard.BlackKingsideMask;
                if ((castleMask & blockers) == 0)
                {
                    int target = heroColour == Piece.White ? Square.g1 : Square.g8;
                    moves.Add(Move.Initialise(Move.Castling, heroKingIndex, target, Piece.King, Piece.None));
                }
            }

            if (board.CanCastleQueenside(heroColour))
            {
                ulong castleMask = heroColour == Piece.White ? Bitboard.WhiteQueensideMask : Bitboard.BlackQueensideMask;
                ulong castleMask2 = heroColour == Piece.White ? (1ul << Square.b1) : (1ul << Square.b8);
                if ((castleMask & blockers) == 0 && (castleMask2 & allPieces) == 0)
                {
                    int target = heroColour == Piece.White ? Square.c1 : Square.c8;
                    moves.Add(Move.Initialise(Move.Castling, heroKingIndex, target, Piece.King, Piece.None));
                }
            }
        }
    }

    private void GenerateSlidingMoves(List<int> moves)
    {
        ulong legalSquares = ~hero & checkMask;
        ulong orthogonalPieces = board.GetOrthogonalSlidingBitboard(heroIndex);
        ulong diagonalPieces = board.GetDiagonalSlidingBitboard(heroIndex);

        if (inCheck)
        {
            // cannot move pinned pieces
            orthogonalPieces &= ~pinRays;
            diagonalPieces &= ~pinRays;
        }

        foreach (int index in Bitboard.GetIndicesFromBitboard(orthogonalPieces))
        {
            // This can be optimised with another lookup table maybe?
            for (int d = 0; d < 4; d++)
            {
                ulong targetSquares = Bitboard.GetRayAttacks(hero, opponent, d, index) & legalSquares;

                if (CheckIfPinned(index))
                {
                    // can only move along the pin ray
                    targetSquares &= Data.RayThroughSquares[index, heroKingIndex];
                }

                foreach (int target in Bitboard.GetIndicesFromBitboard(targetSquares))
                {
                    moves.Add(Move.Initialise(Move.Standard, index, target, board.GetPieceTypeAtIndex(index), board.GetPieceTypeAtIndex(target)));
                }
            }
        }

        foreach (int index in Bitboard.GetIndicesFromBitboard(diagonalPieces))
        {
            for (int d = 4; d < 8; d++)
            {
                ulong targetSquares = Bitboard.GetRayAttacks(hero, opponent, d, index) & legalSquares;

                if (CheckIfPinned(index))
                {
                    targetSquares &= Data.RayThroughSquares[index, heroKingIndex];
                }

                foreach (int target in Bitboard.GetIndicesFromBitboard(targetSquares))
                {
                    moves.Add(Move.Initialise(Move.Standard, index, target, board.GetPieceTypeAtIndex(index), board.GetPieceTypeAtIndex(target)));
                }
            }
        }
    }

    private void GenerateKnightMoves(List<int> moves)
    {
        ulong legalSquares = ~hero & checkMask;
        ulong knights = board.GetPieceBitboard(Piece.Knight, heroIndex) & ~pinRays;

        foreach (int index in Bitboard.GetIndicesFromBitboard(knights))
        {
            ulong targetSquares = Data.KnightAttacks[index] & legalSquares;

            foreach (int target in Bitboard.GetIndicesFromBitboard(targetSquares))
            {
                moves.Add(Move.Initialise(Move.Standard, index, target, Piece.Knight, board.GetPieceTypeAtIndex(target)));
            }
        }
    }

    private void GeneratePawnMoves(List<int> moves)
    {
        int dir = heroColour == Piece.White ? 1 : -1;
        int offset = dir * 8;
        ulong pawns = board.GetPieceBitboard(Piece.Pawn, heroIndex);

        ulong promotionMask = heroColour == Piece.White ? Bitboard.Rank8 : Bitboard.Rank1;

        ulong singlePush = Bitboard.ShiftLeft(pawns, offset) & emptySquares;
        ulong promotions = singlePush & promotionMask & checkMask;
        ulong singlePushNoPromotion = singlePush & ~promotionMask & checkMask;

        ulong edgeMask1 = heroColour == Piece.White ? Bitboard.FileA : Bitboard.FileH;
        ulong edgeMask2 = heroColour == Piece.White ? Bitboard.FileH : Bitboard.FileA;

        ulong capture1 = Bitboard.ShiftLeft(pawns & ~edgeMask1, dir * 7) & opponent & checkMask;
        ulong capture2 = Bitboard.ShiftLeft(pawns & ~edgeMask2, dir * 9) & opponent & checkMask;

        ulong promotions1 = capture1 & promotionMask;
        ulong promotions2 = capture2 & promotionMask;
        capture1 &= ~promotionMask;
        capture2 &= ~promotionMask;


        // single pushes without promotion
        foreach (int target in Bitboard.GetIndicesFromBitboard(singlePushNoPromotion))
        {
            int start = target - offset;
            // pawn needs to not be pinned or moving in the ray of the pin
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                moves.Add(Move.Initialise(Move.Standard, start, target, Piece.Pawn, Piece.None));
            }
        }

        // double pushes
        ulong doublePushMask = heroColour == Piece.White ? Bitboard.Rank4 : Bitboard.Rank5;
        ulong doublePush = Bitboard.ShiftLeft(singlePush, offset) & emptySquares & doublePushMask & checkMask;

        foreach (int target in Bitboard.GetIndicesFromBitboard(doublePush))
        {
            int start = target - offset * 2;
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                moves.Add(Move.Initialise(Move.PawnTwoSquares, start, target, Piece.Pawn, Piece.None));
            }
        }

        // captures
        foreach (int target in Bitboard.GetIndicesFromBitboard(capture1))
        {
            int start = target - dir * 7;
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                moves.Add(Move.Initialise(Move.Standard, start, target, Piece.Pawn, board.GetPieceTypeAtIndex(target)));
            }
        }

        foreach (int target in Bitboard.GetIndicesFromBitboard(capture2))
        {
            int start = target - dir * 9;
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                moves.Add(Move.Initialise(Move.Standard, start, target, Piece.Pawn, board.GetPieceTypeAtIndex(target)));
            }
        }

        // promotions
        foreach (int target in Bitboard.GetIndicesFromBitboard(promotions))
        {
            int start = target - dir * 8;
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                AddPromotions(start, target, moves);
            }
        }

        foreach (int target in Bitboard.GetIndicesFromBitboard(promotions1))
        {
            int start = target - dir * 7;
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                AddPromotions(start, target, moves);
            }
        }

        foreach (int target in Bitboard.GetIndicesFromBitboard(promotions2))
        {
            int start = target - dir * 9;
            if (!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[target, heroKingIndex])
            {
                AddPromotions(start, target, moves);
            }
        }

        // en passant

        int enPassantTarget = board.enPassantTarget;
        if (enPassantTarget != -1)
        {
            int capturedPawnIndex = enPassantTarget - offset;

            if ((checkMask & (1ul << capturedPawnIndex)) > 0)
            {
                ulong possiblePawns = pawns & Data.PawnAttacks[opponentIndex][enPassantTarget];

                foreach (int start in Bitboard.GetIndicesFromBitboard(possiblePawns))
                {
                    if ((!CheckIfPinned(start) || Data.RayThroughSquares[start, heroKingIndex] == Data.RayThroughSquares[enPassantTarget, heroKingIndex]) && CheckEnPassant(start, enPassantTarget, capturedPawnIndex))
                    {
                        moves.Add(Move.Initialise(Move.EnPassant, start, enPassantTarget, Piece.Pawn, Piece.Pawn));
                    }
                }
            }
        }
    }

    private void AddPromotions(int start, int target, List<int> moves)
    {
        moves.Add(Move.Initialise(Move.PromoteToBishop, start, target, Piece.Pawn, board.GetPieceTypeAtIndex(target)));
        moves.Add(Move.Initialise(Move.PromoteToKnight, start, target, Piece.Pawn, board.GetPieceTypeAtIndex(target)));
        moves.Add(Move.Initialise(Move.PromoteToQueen, start, target, Piece.Pawn, board.GetPieceTypeAtIndex(target)));
        moves.Add(Move.Initialise(Move.PromoteToRook, start, target, Piece.Pawn, board.GetPieceTypeAtIndex(target)));
    }

    // returns false if the king is in check after playing the en passant
    private bool CheckEnPassant(int start, int target, int captureSquare)
    {
        ulong opponentOrthogonalPieces = board.GetOrthogonalSlidingBitboard(opponentIndex);

        if (opponentOrthogonalPieces != 0)
        {
            ulong ignore = 1ul << start | 1ul << target | 1ul << captureSquare;
            ulong attacks = 0;
            for (int i = 0; i < 4; i++)
            {
                attacks |= Bitboard.GetRayAttacks(hero, opponent, i, heroKingIndex, ignore);
            }

            return (opponentOrthogonalPieces & attacks) == 0;

        }

        return true;
    }
}
