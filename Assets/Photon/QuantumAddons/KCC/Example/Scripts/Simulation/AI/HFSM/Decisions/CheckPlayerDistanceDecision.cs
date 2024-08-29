
namespace Quantum
{
    using System;

    [Serializable]
    public class CheckPlayerDistanceDecision : HFSMDecision
    {
        public override unsafe bool Decide(Frame frame,EntityRef target,ref AIContext aiContext)
        {
            return true;
        }
    }
}
