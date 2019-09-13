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

using System;
using System.Collections.Generic;

namespace TreeRoutine.TreeSharp
{
    public delegate bool InterruptCheckDecoratorDelegate(object context);

    public class Interrupt : GroupComposite
    {
        public Interrupt(InterruptCheckDecoratorDelegate interruptCondition, Composite child)
            : this(child)
        {
            InterruptCondition = interruptCondition;
        }

        public Interrupt(Composite child)
            : base(child)
        {
        }

        protected InterruptCheckDecoratorDelegate InterruptCondition { get; private set; }

        public Composite DecoratedChild { get { return Children[0]; } }

        protected virtual bool InterruptCheck(object context)
        {
            return false;
        }

        public override void Start(object context)
        {
            if (Children.Count != 1)
            {
                throw new ApplicationException("Decorators must have only one child.");
            }
            base.Start(context);
        }

        public override IEnumerable<RunStatus> Execute(object context)
        {
            DecoratedChild.Start(context);
            while (DecoratedChild.Tick(context) == RunStatus.Running)
            {
				if (InterruptCondition != null)
				{
					if (InterruptCondition(context))
					{
						break;
					}
				}
				else if (InterruptCheck(context))
				{
					break;
				}

                yield return RunStatus.Running;
            }

            DecoratedChild.Stop(context);
            if (DecoratedChild.LastStatus != RunStatus.Success)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            yield return RunStatus.Success;
            yield break;
        }
    }
}
