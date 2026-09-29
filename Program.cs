namespace BruteforceJobProblem;
using Time = Double;

class Program
{
    void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        Time[] rj = new Time[n];
        Time[] pj = new Time[n];
        Time[] aj = new Time[n];
        for (int i = 0; i < n; i++)
        {
            Time[] line = Console.ReadLine().Split(' ').Select(x => Time.Parse(x)).ToArray();
            rj[i] = line[0];
            pj[i] = line[1];
            aj[i] = line[2];
        }
        Problem p = new Problem(n, rj, pj, aj);
        p.Solve();
    }
}

