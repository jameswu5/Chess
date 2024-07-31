
namespace Chess.Core;

public static class FEN
{
    public const string standard = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string test = "8/8/8/8/2n5/8/8/8 w - - 0 1";
    public const string enpassant = "rnbqkbnr/ppp1p1pp/8/8/3pPp2/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
    public const string promotion = "8/4PP2/8/3k1K2/8/8/3pp3/8 w - - 0 1";
    public const string stalemate = "8/8/8/8/8/8/q5k1/5K3 w - - 0 1";
    public const string checkmate = "8/8/8/8/8/q4k3/8/5K3 b - - 0 1";
    public const string fiftyMoveRule = "8/8/r2k5/8/8/4K3/8/8 w - - 86 64";
    public const string insufficient = "8/8/8/8/k7/8/6Kp/8 w - - 0 1";
    public const string twoRooksLadder = "5r2/5r2/5k2/8/8/4K3/8/8 w - - 0 1";

    public const string PerftTestPos2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0";
    public const string PerftTestPos3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 0";
    public const string PerftTestPos4a = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
    public const string PerftTestPos4b = "r2q1rk1/pP1p2pp/Q4n2/bbp1p3/Np6/1B3NBn/pPPP1PPP/R3K2R b KQ - 0 1";
    public const string PerftTestPos5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
    public const string PerftTestPos6 = "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10";

    public const string Sicilian = "rnbqkbnr/pp2pppp/3p4/8/3NP3/8/PPP2PPP/RNBQKB1R b KQkq - 0 4";
    public const string RuyLopez = "r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";
    public const string French = "rnbqk1nr/ppp2ppp/4p3/3p4/1b1PP3/2N5/PPP2PPP/R1BQKBNR w KQkq - 2 4";
    public const string Italian = "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4";
    public const string CaroKann = "rn1qkbnr/pp2pppp/2p5/3pPb2/3P4/8/PPP2PPP/RNBQKBNR w KQkq - 1 4";
    public const string QueensGambit = "rnbqkb1r/ppp2ppp/4pn2/3p4/2PP4/2N5/PP2PPPP/R1BQKBNR w KQkq - 2 4";
    public const string Slav = "rnbqkb1r/pp2pppp/2p2n2/3p4/2PP4/2N5/PP2PPPP/R1BQKBNR w KQkq - 2 4";
    public const string KingsIndian = "rnbqk2r/ppppppbp/5np1/8/2PP4/2N5/PP2PPPP/R1BQKBNR w KQkq - 2 4";
    public const string English = "rnbqk2r/pppp1ppp/5n2/4p3/1bP5/2N3P1/PP1PPP1P/R1BQKBNR w KQkq - 1 4";
}