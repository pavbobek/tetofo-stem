using tetofo.EventBus.Mock;
using tetofo.Stem;
using tetofo.Stem.Mock;

namespace tetofo;

public static class Program
{
    public static void Main(string[] args)
    {
        using IStem stem = new MockStem();
        stem.Event(new MockEvent());
    }
}
