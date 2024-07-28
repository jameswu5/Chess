
namespace Chess.Core;

public static class Square
{
    public static string ConvertIndexToSquareName(int index)
    {
        int rank = (index >> 3) + 1;
        return GetFileName(index) + rank.ToString();
    }

    public static string GetFileName(int index)
    {
        int file = GetFile(index);
        string[] letters = { "#", "a", "b", "c", "d", "e", "f", "g", "h" };
        return letters[file];
    }

    public static int GetFile(int index) => (index & 0b111) + 1;

    public static int GetRank(int index) => (index >> 3) + 1;

    public static int GetIndexFromSquareName(string name)
    {
        int index = 0;

        foreach (char c in name)
        {
            if ("abcdefgh".Contains(c))
            {
                index += c - 'a';
            }
            else
            {
                index += (c - '1') * 8;
            }
        }

        return index;
    }
}