using System.Collections.Immutable;

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
        State state = new State(0, null, [.. new int[n]], [.. new Time[n]]);
        RecursiveThing(state);
    }

    public void RecursiveThing(State state)
    {
        Console.WriteLine(state);
        // if not doing a job
        if (!state.Job.HasValue)
        {
            // how to not do this with ugly rename shit... 
            Time? nextInterrupt = FindNextInterruption(state.CurrentTime);
            if (nextInterrupt is { } nextIntteruptNotNull)
            {
                State still_not_doing_anything = state with { CurrentTime = nextIntteruptNotNull };
                RecursiveThing(still_not_doing_anything);
                // Extremely slow, but works for small test cases
                foreach (var (_, jobToDo) in 
                         rj .Select((t, index) => (t, index ))
                             .Where(t => t.t == nextIntteruptNotNull))
                {
                    // Create states where all different possible starting jobs at the next time are worked on.
                    RecursiveThing(state with {CurrentTime = nextIntteruptNotNull,  Job = jobToDo});
                }
            }
            else
            {
                // Can't find the next interruption, this state will keep doing nothing until the end??
                // Return some values
                // If haven't done everything yet, return a null or something
                return;
            }
        }

        // If doing a job
        if (state.Job.HasValue)
        {
            int job = state.Job.Value;
            Time timeThisJobDone = (pj[job] /* add the penalty values here */ - state.TimeSpentPerJob[job]) + state.CurrentTime;
            // Find a job that could interrupt this job from completely finishing
            Time? nextInterrupt = FindNextInterruption(state.CurrentTime, timeThisJobDone);
            
            // If there is a possible interruption
            if (nextInterrupt is { } nextTime)
            {
                Time timeStep = nextTime - state.CurrentTime;
                ImmutableArray<Time> timeSpentPerJob = state.TimeSpentPerJob.SetItem(job, state.TimeSpentPerJob[job] + timeStep);
                ImmutableArray<int> interruptions = state.Interruptions.SetItem(job, state.Interruptions[job] + 1);

                State doNothingState = new State(
                    Job: null, 
                    CurrentTime: nextTime, 
                    Interruptions: interruptions, 
                    TimeSpentPerJob: timeSpentPerJob
                    );
                
                foreach (var (_, jobToDo) in 
                         rj .Select((t, index) => (t, index ))
                             .Where(t => t.t == nextTime))
                {
                    
                    // Create states where all different possible starting jobs at the next time are worked on.
                    
                    RecursiveThing(new State(
                            Job: jobToDo, 
                            CurrentTime: nextTime, 
                            Interruptions: interruptions, 
                            TimeSpentPerJob: timeSpentPerJob
                        )
                        );
                }
                
            }
            
            // If there are no interruptions, finish the current job
            else
            {
                State nextState = state with
                {
                    CurrentTime = timeThisJobDone,
                    Job = null,
                    // Not sure if floating point values are going to make this go bad
                    TimeSpentPerJob = state.TimeSpentPerJob.SetItem(job, timeThisJobDone - state.CurrentTime)
                };
                RecursiveThing(nextState);
            }
            
        }
        
    }


    /// <summary>
    /// Returns the first time a new job will be released after the currentTime.
    /// Goes through the whole array to find this value,
    /// if we ever want to do the effort of sorting stuff before running this algorithm this could be logn using binary search,
    /// </summary>
    /// <param name="currentTime"></param>
    /// <param name="beforeThisTime"></param>
    /// <returns></returns>
    private Time? FindNextInterruption(Time currentTime, Time beforeThisTime = Time.MaxValue)
    {
        Time nextInterruption = beforeThisTime;

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
readonly record struct State(Time CurrentTime, int? Job, ImmutableArray<int> Interruptions, ImmutableArray<Time> TimeSpentPerJob)
{
    // public Time CurrentTime { get; init; }
    // // The job that is currently being worked on. Could be null
    // public int? Job { get; init; }
    //
    // public ImmutableArray<Time> Interruptions { get; init; }
    // public ImmutableArray<Time> TimeSpentPerJob { get; init; }
}