using System;
using System.Collections.Immutable;

namespace Pouring2
{
    using State = IImmutableList<int>;

    public partial class Pouring
    {
        private class Empty : Move
        {
            private readonly int _glass;

            public Empty(int glass)
            {
                _glass = glass;
            }

            public override State Change(Pouring pouring, State state)
            {
                return state.SetItem(_glass, 0);
            }

            public override string ToString()
            {
                return string.Format("Empty({0})", _glass);
            }
        }

        private class Fill : Move
        {
            private readonly int _glass;

            public Fill(int glass)
            {
                _glass = glass;
            }

            public override State Change(Pouring pouring, State state)
            {
                return state.SetItem(_glass, pouring.Capacities[_glass]);
            }

            public override string ToString()
            {
                return string.Format("Fill({0})", _glass);
            }
        }

        private class Pour : Move
        {
            private readonly int _fromGlass;
            private readonly int _toGlass;

            public Pour(int fromGlass, int toGlass)
            {
                _fromGlass = fromGlass;
                _toGlass = toGlass;
            }

            public override State Change(Pouring pouring, State state)
            {
                var toCapacity = pouring.Capacities[_toGlass];
                var fromState = state[_fromGlass];
                var toState = state[_toGlass];
                var amount = Math.Min(fromState, toCapacity - toState);
                return state
                    .SetItem(_fromGlass, fromState - amount)
                    .SetItem(_toGlass, toState + amount);
            }

            public override string ToString()
            {
                return string.Format("Pour({0},{1})", _fromGlass, _toGlass);
            }
        }
    }
}
