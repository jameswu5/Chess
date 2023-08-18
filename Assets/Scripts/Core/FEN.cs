using System.Collections;
using System.Collections.Generic;



public static class FEN
{
    public const string standard = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string test = "8/8/8/8/2n5/8/8/8 w - - 0 1";
    public const string enpassant = "rnbqkbnr/ppp1p1pp/8/8/3pPp2/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
    public const string promotion = "8/4PP2/8/3k1K2/8/8/3pp3/8 w - - 0 1";
    public const string stalemate = "8/8/8/8/8/8/q5k1/5K3 w - - 0 1";
    public const string fiftyMoveRule = "8/8/r2k5/8/8/4K3/8/8 w - - 86 64";
    public const string insufficient = "8/8/8/8/k7/8/6Kp/8 w - - 0 1";
    public const string checkmate = "rnbqkbnr/pppppppp/8/8/8/8/8/4K3 w kq - 0 1";

    public const string PerftTest1 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0";
    public const string PerftTest2 = "r3k2r/p1ppqpb1/bn2Pnp1/4N3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - 0 0";


}
