namespace BruteforceJobProblem;

class Program
{
    void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        int[] rj = new int[n];
        int[] pj = new int[n];
        int[] aj = new int[n];
        for (int i = 0; i < n; i++)
        {
            int[] line = Console.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
            rj[i] = line[0];
            pj[i] = line[1];
            aj[i] = line[2];
        }
        Problem p = new Problem(n, rj, pj, aj);
        p.Solve();
    }
}


class Problem(int n, int[] rj, int[] pj, int[] aj)
{
    private int n = n;
    private int[] rj = rj;
    private int[] pj = pj;
    private int[] aj = aj;

    public void Solve() // Should return the solution, not sure what type it will be
    {
        
    }
    
}