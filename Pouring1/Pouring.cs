using System;
using System.Collections.Generic;
using System.Linq;
using Code;
using Flinq;

namespace Pouring1
{
    public partial class Pouring
    {
        private readonly int[] _capacities;
        private readonly State _initialState;
        private readonly Path _initialPath;
        private readonly IList<Move> _allPossibleMoves;

        public Pouring(params int[] capacities)
        {
            _capacities = capacities;
            _initialState = new State(_capacities.Map(_ => 0));
            _initialPath = new Path(this, _initialState, System.Linq.Enumerable.Empty<Move>());

            var glasses = System.Linq.Enumerable.Range(0, _capacities.Length).ToList();
            var moves1 = glasses.Map(g => new Empty(g));
            var moves2 = glasses.Map(g => new Fill(this, g));
            var moves3 = glasses.FlatMap(g1 => glasses.Map(g2 => (g1 != g2) ? new Pour(this, g1, g2) : null)).Where(x => x != null);

            _allPossibleMoves = moves1
                .Cast<Move>()
                .Concat(moves2)
                .Concat(moves3)
                .ToList();
        }

        public class State : List<int>
        {
            public State(IEnumerable<int> collection)
                : base(collection)
            {
            }

            public override string ToString()
            {
                return this.MkString("(", ",", ")");
            }
        }

        public abstract class Move
        {
            public abstract State Change(State state);
        }

        public class Path
        {
            private readonly Pouring _pouring;
            public State EndState { get; private set; }
            private readonly IEnumerable<Move> _history;

            public Path(Pouring pouring, State endState, IEnumerable<Move> history)
            {
                _pouring = pouring;
                EndState = endState;
                _history = history;
            }

            public Path Extend(Move move)
            {
                return new Path(_pouring, move.Change(EndState), new[] {move}.Concat(_history));
            }

            public override string ToString()
            {
                var movesAndStates = _history.FoldRight(
                    new List<Tuple<Move, State, string>>(),
                    (move, acc) =>
                        {
                            var previousState = (acc.IsEmpty()) ? _pouring._initialState : acc.First().Item2;
                            var nextState = move.Change(previousState);
                            var paddingPrefix = new string(' ', acc.Count);
                            var moveDescription = string.Format("{0}{1} => {2}", paddingPrefix, move, nextState);
                            return new[] {Tuple.Create(move, nextState, moveDescription)}.Concat(acc).ToList();
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

        private static readonly IEqualityComparer<State> Comparer = new StateEqualityComparer();

        private Stream<IList<Path>> From(IList<Path> paths, IEnumerable<State> explored)
        {
            if (paths.IsEmpty()) return Stream<IList<Path>>.EmptyStream;
            var morePaths = paths
                .FlatMap(p => _allPossibleMoves.Map(p.Extend))
                .Where(p => !System.Linq.Enumerable.Contains(explored, p.EndState, Comparer)).ToList();
            return Stream<IList<Path>>.ConsStream(
                paths,
                () => From(
                    morePaths,
                    explored.Concat(morePaths.Map(p => p.EndState))));
        }

        public IEnumerable<Path> Solutions(int target)
        {
            var pathSets = From(new[] {_initialPath}, new[] {_initialState});
            return pathSets
                .ToEnumerable()
                .SelectMany(pathSet => pathSet.Where(path => path.EndState.Contains(target)));
        }
    }
}
