
public class State
{
    public int fiftyMoveCounter;
    public int castlingRights;
    public int enPassantTarget;
    public ulong zobristKey;
    public bool inCheck;

    public State(int fiftyMoveCounter, int castlingRights, int enPassantTarget, ulong zobristKey, bool inCheck)
    {
        this.fiftyMoveCounter = fiftyMoveCounter;
        this.castlingRights = castlingRights;
        this.enPassantTarget = enPassantTarget;
        this.zobristKey = zobristKey;
        this.inCheck = inCheck;
    }
}