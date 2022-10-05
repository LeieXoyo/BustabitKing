namespace BustabitKing;

internal class Gambler
{
    private int _lossCount;
    public bool CanBet;
    public double RoundBalance { get; private set; }

    public Gambler(double roundBalance)
    {
        _lossCount = 0;
        CanBet = false;
        RoundBalance = roundBalance;
    }

    public void Win(string? screenshotPath = null, int? targetCount = null)
    {
        FallBack();
        if (screenshotPath != null)
        {
            Helper.SendMail("Info", $"targetCount: {targetCount} and Screenshot", screenshotPath);
        }
    }

    public void Loss(string? screenshotPath = null, int? targetCount = null)
    {
        _lossCount += 1;
        if (_lossCount >= 25)
        {
            FallBack();
            if (screenshotPath != string.Empty)
            {
                Helper.SendMail("Info", $"targetCount: {targetCount} and Screenshot", screenshotPath);
            }
        }
    }

    public void FallBack()
    {
        _lossCount = 0;
        CanBet = false;
    } 

    public void updateRoundBalance(double roundBalance)
    {
        RoundBalance = roundBalance;
    }

    public int Bet()
    {
        return Convert.ToInt32(RoundBalance / 200);
    }
}