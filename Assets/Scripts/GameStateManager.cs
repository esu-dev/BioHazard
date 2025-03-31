using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
    State _state;


    public void ChangeStateToPlayState()
    {
        _state = new PlayState();
        _state.Enter();
    }

    public void ChangeStateToPauseState()
    {
        _state = new PauseState();
        _state.Enter();
    }

    private void Start()
    {
        _state = new PlayState();
        _state.Enter();
    }


    abstract class State
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
    }

    class PlayState : State
    {
        public override void Enter()
        {
            Cursor.lockState = CursorLockMode.Locked;
            LocalizedTime.timeScale = 1;
        }
    }

    class PauseState : State
    {
        public override void Enter()
        {
            Cursor.lockState = CursorLockMode.None;
            LocalizedTime.timeScale = 0;
        }
    }
}
