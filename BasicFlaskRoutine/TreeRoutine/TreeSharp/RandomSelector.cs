#region License

// A simplistic Behavior Tree implementation in C#
// Copyright (C) 2010-2011 ApocDev apocdev@gmail.com
// 
// This file is part of TreeSharp
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace TreeRoutine.TreeSharp
{
    /// <summary>
    ///   Will execute randomly a branch of logic. If the branch returns running. It will be called again on next run..
    /// </summary>
    public class RandomSelector : Selector
    {
		private System.Random _random;

		public RandomSelector(int seed, params Composite[] children) 
			: base(children) 
		{
			_random = new System.Random(seed);
		}
			
        public RandomSelector(params Composite[] children)
            : base(children)
        {
			_random = new System.Random();
        }

        public RandomSelector(int seed, ContextChangeHandler contextChange, params Composite[] children)
            : this(seed, children)
        {
            ContextChanger = contextChange;
        }

        public RandomSelector(ContextChangeHandler contextChange, params Composite[] children)
            : this(children)
        {
            ContextChanger = contextChange;
        }

        public override IEnumerable<RunStatus> Execute(object context)
        {
            // lock (Locker)
            {
				while (Children.Count > 0) {
					var node = Children[_random.Next(0, Children.Count)];
					node.Start(context);

					while (node.Tick(context) == RunStatus.Running)
					{
						Selection = node;
						yield return RunStatus.Running;
					}

					// Clear the selection... since we don't have one! Duh.
					Selection = null;
					// Call Stop to allow the node to cleanup anything. Since we don't need it anymore.
					node.Stop(context);
					// If it succeeded (since we're a selector) we return that this GroupNode
					// succeeded in executing.
					if (node.LastStatus == RunStatus.Success)
					{
						yield return RunStatus.Success;
						yield break;
					} else if (node.LastStatus == RunStatus.Failure) {
						yield return RunStatus.Failure;
						yield break;
					}

					// XXX - Removed. This would make us use an extra 'tick' just to get to the next child composite.
					// Still running, so continue on!
					//yield return RunStatus.Running;
					//
				}

                // We ran out of children, and none succeeded. Return failed.
                yield return RunStatus.Failure;
                // Make sure we tell our parent composite, that we're finished.
                yield break;
            }
        }
    }
}
