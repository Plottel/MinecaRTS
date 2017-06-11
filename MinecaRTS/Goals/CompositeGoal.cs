using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    /// <summary>
    /// Represents a goal which can be composed of multiple subgoals.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class CompositeGoal<T1, T2> : Goal<T1, T2> where T2 : Bot
    {
        public Stack<Goal<T1, T2>> goals = new Stack<Goal<T1, T2>>();

        public CompositeGoal(T1 owner, T2 bot) : base(owner, bot)
        { }

        public override void Activate() { }

        public override GoalState Process()
        {
            return ProcessSubgoals();
        }

        public GoalState ProcessSubgoals()
        {
            if (goals.Count == 0)
                return GoalState.Complete;

            GoalState subgoalState = goals.Peek().Process();

            if (subgoalState == GoalState.Complete)
            {
                goals.Pop().Terminate();

                if (goals.Count > 0)
                    goals.Peek().Activate();
            }                

            return subgoalState;                
        }

        public override void AddSubgoal(Goal<T1, T2> goal)
        {
            goals.Push(goal);
        }

        new public List<String> ToString(int depth)
        {
            var result = new List<String>();

            string spacing = "";

            for (int i = 0; i < depth; i++)
                spacing += "\t";

            result.Add(spacing + this.GetType().Name);

            foreach (Goal<T1, T2> g in goals)
            {
                if (!(g is CompositeGoal<T1, T2>))
                    result.Add(g.ToString(depth + 1));
                else
                {
                    CompositeGoal<T1, T2> compGoal = g as CompositeGoal<T1, T2>;
                    foreach (string substring in compGoal.ToString(depth + 1))
                        result.Add(substring);
                }
            }
                

            return result;
        }
    }
}
