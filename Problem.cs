using System.Collections.Immutable;
using System.Text;

namespace BruteforceJobProblem;
using Time = Double;

class Problem(int n, Time[] rj, Time[] pj, Time[] aj)
{
    private ImmutableArray<OutputValue> bestSolution;
    private Time bestTotalCompletionTime = Time.MaxValue;
    private int n = n;
    private Time[] rj = rj;
    private Time[] pj = pj;
    private Time[] aj = aj;
    #if DEBUG
    private int counter = 0;
    #endif

    public void Solve() // Should return the solution, not sure what type it will be
    {
        State state = new State(
            -1, 
            null, 
            [.. new int[n]], 
            [.. new Time[n]], 
            [.. new bool[n]], 
            [],
            0
            );
        RecursiveThing(state);


        List<OutputValue> cleanedUpSolution = [bestSolution.First()];

        foreach (var outputValue in bestSolution.Skip(1))
        {
            if (outputValue.Job == cleanedUpSolution.Last().Job)
            {
                int i = cleanedUpSolution.Count - 1; 
                cleanedUpSolution[i] = cleanedUpSolution[i] with
                {
                    TimeSpent = cleanedUpSolution[i].TimeSpent +  outputValue.TimeSpent
                };
            }
            else
            {
                cleanedUpSolution.Add(outputValue);
            }
        }
            
        foreach (var outputValue in cleanedUpSolution)
        {
            Console.WriteLine($"{outputValue.Job + 1}, {outputValue.StartTime}, {outputValue.TimeSpent}");
        }
        Console.WriteLine(bestTotalCompletionTime);
    }

    public void RecursiveThing(State state)
    {
        #if DEBUG
        counter++;
        #endif
        if (state.FinishedJobs.All(x => x) && state.TotalCompletionTime < bestTotalCompletionTime)
        {
            bestTotalCompletionTime = state.TotalCompletionTime;
            bestSolution = state.Solution;
        }

        if (state.Job is 1 && Math.Abs(state.CurrentTime - 3.0) < 0.01 && state.FinishedJobs.Count(x => x) == 1)
        {
            //
        }
        
        #if DEBUG
        // Console.WriteLine(state);
        Console.WriteLine(counter);
        #endif

        // if not doing a job
        if (!state.Job.HasValue)
        {
            Time? nextInterrupt = FindNextInterruption(state.CurrentTime);
            if (nextInterrupt is { } nextTime)
            {
                RecursiveThing(state with {CurrentTime = nextTime});
                // Get all jobs that have not been finished and can be started on at the next time
                foreach (var (_, jobToDo) in 
                         rj .Select((t, index) => (t, index ))
                             .Where(t => t.t <= nextTime &&
                            !state.FinishedJobs[t.index]))
                {
                    // Create states where all different possible starting jobs at the next time are worked on.
                    RecursiveThing(state with {CurrentTime = nextTime,  Job = jobToDo});
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
            Time timeThisJobDone = (pj[job] * Math.Pow(aj[job], state.Interruptions[job]) - state.TimeSpentPerJob[job]) + state.CurrentTime;
            // Find a job that could interrupt this job from completely finishing
            Time? nextInterrupt = FindNextInterruption(state.CurrentTime, timeThisJobDone);
            
            // If there is a possible interruption
            if (nextInterrupt is { } nextTime)
            {
                
                Time timeStep = nextTime - state.CurrentTime;
                ImmutableArray<Time> timeSpentPerJob = state.TimeSpentPerJob.SetItem(job, state.TimeSpentPerJob[job] + timeStep);
                ImmutableArray<int> interruptions = state.Interruptions.SetItem(job, state.Interruptions[job] + 1);

                // Do nothing at the next time
                RecursiveThing(state with
                {
                    Job = null,
                    CurrentTime = nextTime,
                    TimeSpentPerJob= timeSpentPerJob,
                    Interruptions = interruptions
                });
                foreach (var (_, jobToDo) in 
                         rj .Select((t, jobIndex) => (t, jobIndex ))
                             .Where((t, jobIndex) => t.t <= nextTime &&
                                         !state.FinishedJobs[jobIndex]))
                {
                    // Create states where all different possible starting jobs at the next time are worked on.
                    
                    RecursiveThing(state with {
                            Job= jobToDo, 
                            CurrentTime= nextTime, 
                            Interruptions= interruptions,
                            TimeSpentPerJob= timeSpentPerJob,
                            Solution= [ .. state.Solution, new OutputValue(
                                state.Job.Value,
                                state.CurrentTime,
                                timeStep)]
                             }
                        );
                }
                // We also need to create a state where we continue working on a job
                RecursiveThing(state with
                {
                    CurrentTime= nextTime, 
                    TimeSpentPerJob= timeSpentPerJob,
                    FinishedJobs= state.FinishedJobs,
                    Solution= [ .. state.Solution, new OutputValue(
                        state.Job.Value,
                        state.CurrentTime,
                        timeStep)]
                });
                
            }
            
            // If there are no interruptions, finish the current job
            // Create a state where no jobs are worked on, and create new states for the different states to work on
            else
            {
                Time timeStep = timeThisJobDone - state.CurrentTime;
                if (timeStep < 0)
                {
                    
                }
                OutputValue outputValue = new OutputValue(
                    state.Job.Value,
                    state.CurrentTime,
                    timeStep
                );
                State nextState = state with
                {
                    CurrentTime = timeThisJobDone,
                    Job = null,
                    // Not sure if floating point values are going to make this go bad
                    TimeSpentPerJob = state.TimeSpentPerJob.SetItem(job, state.TimeSpentPerJob[job] + timeStep),
                    FinishedJobs = state.FinishedJobs.SetItem(job, true),
                    Solution = [ .. state.Solution, outputValue],
                    TotalCompletionTime = state.TotalCompletionTime + timeThisJobDone
                };
                RecursiveThing(nextState);
                
                foreach (var (_, jobToDo) in 
                         rj .Select((t, jobIndex) => (t, jobIndex ))
                             .Where((t, jobIndex) => t.t <= timeThisJobDone &&
                                                     !state.FinishedJobs[jobIndex]))
                {
                    // Create states where all different possible starting jobs at the next time are worked on.
                    RecursiveThing(state with
                    {
                        Job = jobToDo,
                        CurrentTime = timeThisJobDone,
                        TimeSpentPerJob = state.TimeSpentPerJob.SetItem(job, state.TimeSpentPerJob[job] + timeStep),
                        FinishedJobs = state.FinishedJobs.SetItem(job, true),
                        Solution = [ .. state.Solution, outputValue],
                        TotalCompletionTime = state.TotalCompletionTime + timeThisJobDone
                    });
                }
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
            if (releaseTime < nextInterruption && currentTime < releaseTime)
            {
                nextInterruption = releaseTime;
            }
        }
        
        return Math.Abs(nextInterruption - beforeThisTime) < 0.001 ? null : nextInterruption;
    }
}

readonly record struct State(
    Time CurrentTime,
    int? Job,
    ImmutableArray<int> Interruptions,
    ImmutableArray<Time> TimeSpentPerJob,
    ImmutableArray<bool> FinishedJobs,
    ImmutableArray<OutputValue> Solution,
    Time TotalCompletionTime)
{
    /// <summary>
    /// Saw this online, it's a nicer way to print the member variable of record structs.
    /// I think it's some sort of ovveride/hook, but it doesn't override anything?
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"CurrentTime = {CurrentTime}, ");
        builder.Append($"Job = {Job?.ToString() ?? "null"}, ");
        builder.Append($"Interruptions = [{string.Join(", ", Interruptions)}], ");
        builder.Append($"TimeSpentPerJob = [{string.Join(", ", TimeSpentPerJob)}]");
        builder.Append($"FinishedJobs = [{string.Join(", ", FinishedJobs)}]");
        builder.Append($"Solution = [{string.Join(", ", Solution)}]");
        return true;
    }
}

record struct OutputValue(
    int Job,
    Time StartTime,
    Time TimeSpent
    )
{
}