namespace Utils.Signals {
  public static partial class Signal {
    public const int None = -1;

    // Generic Signals - 0 to 1000
    public const int Trigger = 0;
    public const int Enable = 1;
    public const int Disable = 2;
    public const int Increment = 3;
    public const int Decrement = 4;
    public const int Start = 5;
    public const int Stop = 6;
    public const int Completed = 7;
    public const int On = 8;
    public const int Off = 9;
    public const int Success = 10;
    public const int Failure = 11;
    public const int Spawn = 12;

    // UI Signals - 1001 to 2000
    public const int UI_Press = 1001;
    public const int UI_SetText = 1002;

    // Puzzle Signals - 2001 to 3000
    public const int Puzzle_PullLevel = 2001;
    public const int Puzzle_NextStep = 2002;
    public const int Puzzle_OpenPanel = 2003;
    public const int Puzzle_ClosePanel = 2004;
    public const int Puzzle_Completed = 2005;
    public const int Puzzle_Break = 2006;
    public const int Puzzle_Fix = 2007;
    public const int Puzzle_Progress = 2008;
    public const int Locked = 2009;
    public const int Unlocked = 2010;

    // Grapple Signals - 3001 to 4000
    public const int Grab = 3001;
    public const int Yank = 3002;
    public const int Release = 3003;
    public const int Reset = 3004;
    public const int Attach = 3005;
    public const int Detach = 3006;
    public const int Drop = 3007;
    public const int FreeMove = 3008;
    public const int HoverBegin = 3009;
    public const int HoverEnd = 3010;
    public const int PushPull = 3011;
    public const int Retracted = 3012;
    public const int YankDislodge = 3013;

    //Bot Limb Names - 4001 to 5000
    public const int Head = 4001;
    public const int Shoulder = 4002;
    public const int Elbow = 4003;
    public const int Body = 4004;
    public const int Hip = 4005;
    public const int Knee = 4006;

    // Grapple Playground - 5001 to 6000
    public const int GrapplePlayground_BotBinComplete = 5001;
    public const int GrapplePlayground_BotDestroyed = 5002;
    public const int GrapplePlayground_PanelOpened = 5003;
    public const int GrapplePlayground_BrokenShapesRemoved = 5004;
    public const int GrapplePlayground_BotReady = 5005;
    public const int GrapplePlayground_BitBinOpened = 5006;
    public const int GrapplePlayground_RewardLeverPulled = 5007;
    public const int GrapplePlayground_RewardShowerReady = 5008;
    public const int GrapplePlayground_RewardStageStarted = 5009;

    // Animations
    public const int AnimationStart = 6001;
    public const int AnimationComplete = 6002;

    // Overrides - 7001 to 8000
    public const int OverrideTexture = 7001;
  }
}