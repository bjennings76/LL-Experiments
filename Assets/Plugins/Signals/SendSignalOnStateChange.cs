using UnityEngine;

namespace Utils.Signals {
  public class SendSignalOnStateChange : StateMachineBehaviour {
    [SerializeField] private StateChange m_On = StateChange.Exit;
    [SerializeField, SignalRef] private int m_Send = Signal.AnimationComplete;
    [SerializeField] private SignalSpace m_To = SignalSpace.Local;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      if (m_On == StateChange.Enter) {
        SendSignal(animator);
      }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      if (m_On == StateChange.Exit) {
        SendSignal(animator);
      }
    }

    private void SendSignal(Component sender) {
      SignalManager.Send(sender, m_Send, null, m_To);
    }

    private enum StateChange {
      Enter,
      Exit
    }
  }
}