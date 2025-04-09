using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
    State _currentState;
    State _playState;
    State _pauseState;

    public UnityEvent OnPlayStateEnter => _playState.OnStateEnter;
    public UnityEvent OnPauseStateEnter => _pauseState.OnStateEnter;
    


    public void ChangeStateToPlayState()
    {
        _currentState = _playState;
        _currentState.Enter();
    }

    public void ChangeStateToPauseState()
    {
        _currentState = _pauseState;
        _currentState.Enter();
    }

    private void Start()
    {
        _playState = new PlayState();
        _pauseState = new PauseState();

        ChangeStateToPlayState();
    }


    abstract class State
    {
        public UnityEvent OnStateEnter = new UnityEvent();

        public virtual void Enter() { }
        public virtual void Exit() { }
    }

    class PlayState : State
    {
        public override void Enter()
        {
            Cursor.lockState = CursorLockMode.Locked;
            LocalizedTime.timeScale = 1;

            OnStateEnter.Invoke();
        }
    }

    class PauseState : State
    {
        public override void Enter()
        {
            Cursor.lockState = CursorLockMode.None;
            LocalizedTime.timeScale = 0;

            OnStateEnter.Invoke();
        }
    }
}
