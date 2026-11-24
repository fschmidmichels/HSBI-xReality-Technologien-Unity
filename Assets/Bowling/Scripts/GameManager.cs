using System;
using Oculus.Interaction;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Ready,
        ThrowRunning,
        Evaluation,
        Restart
    }

    private Grabbable ballGrabbable;

    [SerializeField]
    [Tooltip("LaneControls der Bowlingbahn.")]
    private LaneControls laneControls;

    [SerializeField]
    [Tooltip("BallReturn an der PitZone.")]
    private BallReturn ballReturn;

    [SerializeField]
    [Min(0.1f)]
    [Tooltip("So lange (Sekunden) muss die Kugel stillliegen, bis ein Wurf ohne Ankunft in der PitZone endet " +
             "(z. B. Kugel bleibt in der Rinne oder vor den Pins liegen).")]
    private float ballStoppedDuration = 1.5f;

    [SerializeField]
    [Min(1f)]
    [Tooltip("Obergrenze in Sekunden für einen Wurf, auch wenn die Kugel noch rollt. " +
             "Großzügig wählen: Eine langsame Kugel braucht für die Bahn deutlich mehr als 10 s.")]
    private float maxThrowDuration = 30f;

    [SerializeField]
    [Min(0.01f)]
    [Tooltip("Mindestgeschwindigkeit der Kugel, ab der ein Wurf beginnt.")]
    private float throwStartSpeed = 0.1f;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Maximalgeschwindigkeit in m/s, die noch als Stillstand der Kugel gilt.")]
    private float stoppedSpeedThreshold = 0.02f;

    [SerializeField]
    [Min(0f)]
    [Tooltip("Kurze Wartezeit nach der Auswertung, bevor der nächste Wurf möglich ist.")]
    private float restartDuration = 0.5f;

    [SerializeField]
    private GameState currentState = GameState.Ready;

    private float throwElapsed;
    private float ballStoppedElapsed;
    private float restartElapsed;

    public GameState CurrentState
    {
        get { return currentState; }
    }

    public event Action<GameState> StateChanged;

    private void Awake()
    {
        if (laneControls == null || laneControls.ball == null || ballReturn == null)
        {
            Debug.LogError("GameManager requires LaneControls with a ball and a BallReturn reference.", this);
            enabled = false;
            return;
        }

        ballGrabbable = laneControls.ball.GetComponent<Grabbable>();
        if (ballGrabbable == null)
        {
            Debug.LogError("GameManager requires an ISDK Grabbable on the bowling ball.", this);
            enabled = false;
            return;
        }

        // Ab jetzt zählt BallReturn einen Wurf nur noch, wenn der GameManager ihn mit BeginThrow() startet.
        ballReturn.UseExternalThrowControl();
    }

    private void OnEnable()
    {
        if (ballGrabbable != null)
        {
            ballGrabbable.VelocityThrow.WhenThrown += HandleBallThrown;
        }

        if (ballReturn == null)
        {
            return;
        }

        ballReturn.BallReachedPit += HandleBallReachedPit;
        ballReturn.ThrowEvaluated += HandleThrowEvaluated;
        ballReturn.ThrowCancelled += HandleThrowCancelled;
    }

    private void OnDisable()
    {
        if (ballGrabbable != null)
        {
            ballGrabbable.VelocityThrow.WhenThrown -= HandleBallThrown;
        }

        if (ballReturn == null)
        {
            return;
        }

        ballReturn.BallReachedPit -= HandleBallReachedPit;
        ballReturn.ThrowEvaluated -= HandleThrowEvaluated;
        ballReturn.ThrowCancelled -= HandleThrowCancelled;
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.ThrowRunning:
                throwElapsed += Time.deltaTime;

                // Ein Wurf endet nicht nach fester Zeit, sondern erst, wenn die Kugel liegen bleibt.
                // Eine gehaltene Kugel ist kinematisch und wirkt "still", zählt hier aber nicht.
                if (IsBallStopped() && !laneControls.IsBallHeld)
                {
                    ballStoppedElapsed += Time.deltaTime;
                }
                else
                {
                    ballStoppedElapsed = 0f;
                }

                if (ballStoppedElapsed >= ballStoppedDuration || throwElapsed >= maxThrowDuration)
                {
                    SetState(GameState.Evaluation);
                    ballReturn.EndThrowAfterTimeout();
                }
                break;

            case GameState.Restart:
                restartElapsed += Time.deltaTime;
                // Erst bereit, wenn Pins und Kugel ihre Animation beendet haben.
                bool animating = laneControls.pinGroup.IsResetting || laneControls.IsBallReturning;
                if (restartElapsed >= restartDuration && !animating && IsBallStopped())
                {
                    // Kugel war bei der Auswertung gehalten und wurde danach z. B. fallen gelassen: zurück auf die Ablage.
                    // Die Rückholung ist animiert, deshalb im nächsten Durchlauf erneut prüfen.
                    if (!laneControls.IsBallAtStart && !laneControls.IsBallHeld)
                    {
                        laneControls.ReturnBall();
                        break;
                    }
                    SetState(GameState.Ready);
                }
                break;
        }
    }

    private void HandleBallThrown(Vector3 velocity, Vector3 torque)
    {
        float startSpeedSquared = throwStartSpeed * throwStartSpeed;
        // Wird die Kugel zur Rückholung aus der Hand genommen, meldet das ISDK ebenfalls einen Wurf: ignorieren.
        if (currentState != GameState.Ready || laneControls.IsBallReturning ||
            velocity.sqrMagnitude < startSpeedSquared)
        {
            return;
        }

        ballReturn.BeginThrow();
        throwElapsed = 0f;
        ballStoppedElapsed = 0f;
        SetState(GameState.ThrowRunning);
    }

    private void HandleBallReachedPit()
    {
        if (currentState == GameState.ThrowRunning)
        {
            SetState(GameState.Evaluation);
        }
    }

    private void HandleThrowEvaluated(ThrowResult result)
    {
        if (currentState != GameState.Evaluation)
        {
            return;
        }

        restartElapsed = 0f;
        SetState(GameState.Restart);
    }

    private void HandleThrowCancelled()
    {
        throwElapsed = 0f;
        if (currentState != GameState.Ready)
        {
            SetState(GameState.Ready);
        }
    }

    private bool IsBallStopped()
    {
        return laneControls.ball.linearVelocity.sqrMagnitude <= stoppedSpeedThreshold * stoppedSpeedThreshold;
    }

    private void SetState(GameState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        StateChanged?.Invoke(currentState);
    }
}
