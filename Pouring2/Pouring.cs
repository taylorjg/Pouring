﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Code;
using Flinq;

namespace Pouring2
{
    using State = IImmutableList<int>;

    public partial class Pouring
    {
        private readonly IImmutableList<int> _capacities;
        private readonly State _initialState;
        private readonly Path _initialPath;
        private readonly IImmutableList<Move> _allPossibleMoves;

        public Pouring(IImmutableList<int> capacities)
        {
            _capacities = capacities;
            _initialState = ImmutableList.CreateRange(_capacities.Map(_ => 0));
            _initialPath = new Path(this, _initialState, ImmutableList<Move>.Empty);

            var glasses = ImmutableList.CreateRange(System.Linq.Enumerable.Range(0, _capacities.Count));
            var moves1 = glasses.Map(g => new Empty(g));
            var moves2 = glasses.Map(g => new Fill(this, g));
            var moves3 = glasses.FlatMap(g1 => glasses.Map(g2 => (g1 != g2) ? new Pour(this, g1, g2) : null)).Where(x => x != null);

            _allPossibleMoves = ImmutableList.CreateRange(moves1
                .Cast<Move>()
                .Concat(moves2)
                .Concat(moves3));
        }

        public abstract class Move
        {
            public abstract State Change(State state);
        }

        public class Path
        {
            private readonly Pouring _pouring;
            private readonly State _endState;
            private readonly IImmutableList<Move> _history;

            public State EndState
            {
                get { return _endState; }
            }

            public IImmutableList<Move> History
            {
                get { return _history; }
            }

            public Path(Pouring pouring, State endState, IImmutableList<Move> history)
            {
                _pouring = pouring;
                _endState = endState;
                _history = history;
            }

            public Path Extend(Move move)
            {
                return new Path(_pouring, move.Change(EndState), _history.Insert(0, move));
            }

            public override string ToString()
            {
                var movesAndStates = _history.FoldRight(
                    ImmutableList<Tuple<Move, State, string>>.Empty,
                    (move, acc) =>
                        {
                            var previousState = (acc.IsEmpty()) ? _pouring._initialState : acc.First().Item2;
                            var nextState = move.Change(previousState);
                            var paddingPrefix = new string(' ', acc.Count);
                            var nextStateDescription = nextState.MkString("(", ",", ")");
                            var stepDescription = string.Format("{0}{1} => {2}", paddingPrefix, move, nextStateDescription);
                            return acc.Insert(0, Tuple.Create(move, nextState, stepDescription));
                        });
                return movesAndStates
                    .Map(x => x.Item3)
                    .Reverse()
                    .MkString(Environment.NewLine);
            }
        }

        private class StateEqualityComparer : IEqualityComparer<State>
        {
            public bool Equals(State x, State y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(State x)
            {
                return x.GetHashCode();
            }
        }

        private class PathEqualityComparer : IEqualityComparer<Path>
        {
            public bool Equals(Path x, Path y)
            {
                return
                    x.EndState.SequenceEqual(y.EndState) &&
                    x.History.SequenceEqual(y.History);
            }

            public int GetHashCode(Path x)
            {
                return x.EndState.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<State> TheStateEqualityComparer = new StateEqualityComparer();
        private static readonly IEqualityComparer<Path> ThePathEqualityComparer = new PathEqualityComparer();

        private Stream<IImmutableSet<Path>> From(IImmutableSet<Path> paths, IImmutableSet<State> explored)
        {
            if (paths.IsEmpty()) return Stream<IImmutableSet<Path>>.EmptyStream;
            var morePathsEnumerable = paths
                .FlatMap(p => _allPossibleMoves.Map(p.Extend))
                .Where(p => !System.Linq.Enumerable.Contains(explored, p.EndState, TheStateEqualityComparer));
            var morePaths = ImmutableHashSet.CreateRange(ThePathEqualityComparer, morePathsEnumerable);
            return Stream<IImmutableSet<Path>>.ConsStream(
                paths,
                () => From(
                    morePaths,
                    explored.Union(ImmutableHashSet.CreateRange(TheStateEqualityComparer, morePaths.Map(p => p.EndState)))));
        }

        public IEnumerable<Path> Solutions(int target)
        {
            var pathSets = From(
                ImmutableHashSet.Create(ThePathEqualityComparer, _initialPath),
                ImmutableHashSet.Create(TheStateEqualityComparer, _initialState));

            return pathSets
                .ToEnumerable()
                .SelectMany(pathSet => pathSet.Where(path => System.Linq.Enumerable.Contains(path.EndState, target)));
        }
    }
}