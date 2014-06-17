package Coursera.ProgFun

class Pouring(capacities: Vector[Int]) {

  type State = Vector[Int]
  val initialState = capacities map (_ => 0)
  val initialPath = new Path(initialState, Nil)

  trait Move {
    def change(state: State): State
  }
  case class Empty(glass: Int) extends Move {
    def change(state: State): State = state updated (glass, 0)
  }
  case class Fill(glass: Int) extends Move {
    def change(state: State): State = state updated (glass, capacities(glass))
  }
  case class Pour(from: Int, to: Int) extends Move {
    def change(state: State): State = {
      val toCapacity = capacities(to)
      val fromState = state(from)
      val toState = state(to)
      val amount = fromState min (toCapacity - toState)
      state updated (from, fromState - amount) updated (to, toState + amount)
    }
  }

  val glasses = 0 until capacities.length
  val moves =
    (for (glass <- glasses) yield Empty(glass)) ++
    (for (glass <- glasses) yield Fill(glass)) ++
    (for (from <- glasses; to <- glasses; if from != to) yield Pour(from, to))

  class Path(val endState: State, history: List[Move]) {
    def extend(move: Move) = new Path(move change endState, move :: history)
    override def toString = {
      val movesAndStates = (history foldRight List[(Move, State, String)]()) ((move, acc) => {
        val previousState = acc match {
          case Nil => initialState
          case (_, ps, _) :: _ => ps
        }
        val nextState = move change previousState
        (move, nextState, s"$move => $nextState") :: acc
      })
      movesAndStates.map(x => x._3).reverse.mkString("[", ", ", "]")
    }
  }

  def from(paths: Set[Path], explored: Set[State]): Stream[Set[Path]] =
    if (paths.isEmpty) Stream.empty
    else {
      val morePaths = for {
        path <- paths
        next <- moves map path.extend
        if !(explored contains next.endState)
      } yield next
      paths #:: from(morePaths, explored ++ (morePaths map (_.endState)))
    }

  val pathSets = from(Set(initialPath), Set(initialState))

  def solutions(target: Int): Stream[Path] =
    for {
      pathSet <- pathSets
      path <- pathSet
      if path.endState contains target
    } yield path
}

object PouringApp {
  def main(args: Array[String]): Unit = {
    new Pouring(Vector(4, 9)).solutions(6).toList.foreach(println)
  }
}
