namespace BruteforceJobProblem;
using Time = Double;

class Problem(int n, Time[] rj, Time[] pj, Time[] aj)
{
    private int n = n;
    private Time[] rj = rj;
    private Time[] pj = pj;
    private Time[] aj = aj;

    public void Solve() // Should return the solution, not sure what type it will be
    {
        State state = new State(n);
    }

    public void RecursiveThing(State state)
    {
        // if not doing a job
        if (!state.Job.HasValue)
        {
            // how to not do this with ugly rename shit... 
            Time? nextInterrupt = FindNextInterruption(state.CurrentTime);
            if (nextInterrupt is { } nextIntteruptNotNull)
            {
                State still_not_doing_anything = state with { CurrentTime = nextIntteruptNotNull };
                RecursiveThing(still_not_doing_anything);
            }
            else
            {
                // Can't find the next interruption, this state will keep doing nothing until the end??
                // Return some values
                // If haven't done everything yet, return a null or something
                return;
            }
        }

        if (state.Job.HasValue)
        {
            
        }
        
    }


    /// <summary>
    /// Returns the first time a new job will be released after the currentTime.
    /// Goes through the whole array to find this value,
    /// if we ever want to do the effort of sorting stuff before running this algorithm this could be logn using binary search,
    /// </summary>
    /// <param name="currentTime"></param>
    /// <returns></returns>
    private Time? FindNextInterruption(Time currentTime)
    {
        Time nextInterruption = Time.MaxValue;

        foreach (var releaseTime in rj)
        {
            if (releaseTime < nextInterruption && currentTime > releaseTime)
            {
                nextInterruption = releaseTime;
            }
        }
        
        return Math.Abs(nextInterruption - Time.MaxValue) < 0.1 ? null : nextInterruption;
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="n">Amount of jobs</param>
record struct State(int n)
{
    public Time CurrentTime;
    // The job that is currently being worked on. Could be null
    public int? Job = null; 
    
    public Time[] Interruptions = new Time[n];
    public Time[] TimeSpentPerJob = new Time[n];
}