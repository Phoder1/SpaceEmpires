using UniRx;

namespace UniKit.SpaceEmpires
{
    public interface IActionable
    {
        IReadOnlyReactiveProperty<float> Speed { get; }

    }
}
