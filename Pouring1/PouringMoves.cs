﻿using System;

namespace Pouring1
{
    public partial class Pouring
    {
        private class Empty : Move
        {
            private readonly int _glass;

            public Empty(int glass)
            {
                _glass = glass;
            }

            public override State Change(State state)
            {
                var newState = new State(state);
                newState[_glass] = 0;
                return newState;
            }

            public override string ToString()
            {
                return string.Format("Empty({0})", _glass);
            }
        }

        private class Fill : Move
        {
            private readonly Pouring _pouring;
            private readonly int _glass;

            public Fill(Pouring pouring, int glass)
            {
                _pouring = pouring;
                _glass = glass;
            }

            public override State Change(State state)
            {
                var newState = new State(state);
                newState[_glass] = _pouring._capacities[_glass];
                return newState;
            }

            public override string ToString()
            {
                return string.Format("Fill({0})", _glass);
            }
        }

        private class Pour : Move
        {
            private readonly Pouring _pouring;
            private readonly int _fromGlass;
            private readonly int _toGlass;

            public Pour(Pouring pouring, int fromGlass, int toGlass)
            {
                _pouring = pouring;
                _fromGlass = fromGlass;
                _toGlass = toGlass;
            }

            public override State Change(State state)
            {
                var toCapacity = _pouring._capacities[_toGlass];
                var fromState = state[_fromGlass];
                var toState = state[_toGlass];
                var amount = Math.Min(fromState, toCapacity - toState);
                var newState = new State(state);
                newState[_fromGlass] = fromState - amount;
                newState[_toGlass] = toState + amount;
                return newState;
            }

            public override string ToString()
            {
                return string.Format("Pour({0},{1})", _fromGlass, _toGlass);
            }
        }
    }
}
