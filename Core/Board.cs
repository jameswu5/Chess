
namespace Chess.Core;

public class Board
{
    public int[] boardState;
    public int turn;

    public Board()
    {
        Initialise();
    }

    public void Initialise()
    {
        boardState = new int[64];
        turn = -1; // not set to any value
        LoadPosition();
    }

    private void LoadPosition(string FENPosition = FEN.standard) {
        int rank = 7;
        int file = 0;

        string[] sections = FENPosition.Split(' ');

        // first section is the board state
        string boardInformation = sections[0];
        foreach (char c in boardInformation) {
            if (c == '/') {
                rank--;
                file = 0;
            } else if (c >= '0' && c <= '8') {
                file += c - '0';
            } else {
                int pieceColour = char.IsUpper(c) ? Piece.White : Piece.Black;
                int pieceType = Piece.pieceTypes[char.ToUpper(c)];
                int index = (rank << 3) + file;

                int pieceID = pieceColour + pieceType;
                boardState[index] = pieceID;

                // // update the bitboards
                // AddPieceToBitboard(pieceID, index);

                // if (pieceType == Piece.King)
                // {
                //     UpdateKingIndex(pieceColour, index);
                // }

                file++;
            }
        }

        // Second section determines whose turn it is to move
        turn = sections[1] == "w" ? Piece.White : Piece.Black;


        // // Castling Rights
        // foreach (char c in sections[2]) {
        //     switch (c) {
        //         case 'K':
        //             castlingRights |= WhiteKingsideRightMask;
        //             break;
        //         case 'Q':
        //             castlingRights |= WhiteQueensideRightMask;
        //             break;
        //         case 'k':
        //             castlingRights |= BlackKingsideRightMask;
        //             break;
        //         case 'q':
        //             castlingRights |= BlackQueensideRightMask;
        //             break;
        //         default:
        //             break;
        //     }
        // }

        // en passant targets
        // enPassantTarget = sections[3] == "-" ? -1 : Square.GetIndexFromSquareName(sections[3]);

        // // halfmove clock
        // fiftyMoveCounter = Convert.ToInt16(sections[4]);

        // // fullmove clock
        // moveNumber = Convert.ToInt16(sections[5]) - 1;
    }
}