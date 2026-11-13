// Ergebnis eines ausgewerteten Wurfs. Wird von PinGroup.RegisterThrow erzeugt und
// über BallReturn.ThrowEvaluated an Zuhörer (GameManager, EvaluationAudio, ...) verteilt.
public readonly struct ThrowResult
{
    // Nummer des Wurfs im Frame, beginnend bei 1.
    public readonly int throwInFrame;

    // Pins, die in diesem Wurf gefallen sind.
    public readonly int pinsKnockedDown;

    // Pins, die nach diesem Wurf noch stehen (vor dem Neuaufstellen).
    public readonly int pinsStanding;

    public ThrowResult(int throwInFrame, int pinsKnockedDown, int pinsStanding)
    {
        this.throwInFrame = throwInFrame;
        this.pinsKnockedDown = pinsKnockedDown;
        this.pinsStanding = pinsStanding;
    }

    // Alle Pins mit dem ersten Wurf des Frames.
    public bool IsStrike
    {
        get { return throwInFrame == 1 && pinsStanding == 0 && pinsKnockedDown > 0; }
    }

    // Die restlichen Pins mit einem späteren Wurf des Frames.
    public bool IsSpare
    {
        get { return throwInFrame > 1 && pinsStanding == 0 && pinsKnockedDown > 0; }
    }

    // Kein Pin gefallen, z. B. Kugel in der Rinne.
    public bool IsMiss
    {
        get { return pinsKnockedDown == 0; }
    }
}
