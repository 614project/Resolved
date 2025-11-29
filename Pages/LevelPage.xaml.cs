using AcNET.Problem;
using Microsoft.UI.Xaml.Controls;
using Resolved.Collections;
using Resolved.Scripts;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Resolved.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LevelPage : Page
{
    Task addLevelsTask = null!;

    public LevelPage()
    {
        InitializeComponent();
        this.Loading += this.LevelPage_Loading;
        this.Loaded += this.LevelPage_Loaded;
    }

    private async void LevelPage_Loaded(object sender , Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await addLevelsTask;
    }

    private void LevelPage_Loading(Microsoft.UI.Xaml.FrameworkElement sender , object args)
    {
        addLevelsTask = Task.Run(() => {
            ResolvedUser? user = ResolvedConfiguration.CurrentUser is string handle ? ResolvedDatabase.Users.FindOne(user => user.Handle == handle) : null;

            int n = SolvedLevel.Colors.Length;
            int[] total = new int[n];
            int[] solved = new int[n];
            int[] unsolved = new int[n];
            foreach (var problem in ResolvedDatabase.Problems.FindAll())
            {
                total[problem.Level]++;
                if (user?.AcceptProblems?.Contains(problem.Id) == true)
                {
                    solved[problem.Level]++;
                }
                else
                {
                    unsolved[problem.Level]++;
                }
            }

            DispatcherQueue.TryEnqueue(() => {
                for (int id = 0 ; id < n ; id++)
                {
                    LevelDataCollection.Add(new(id , total[id] , solved[id] , unsolved[id]));
                }
            });

        });
    }

    public ObservableCollection<LevelFieldData> LevelDataCollection { get; } = [];
}

public class LevelFieldData(int id,int total,int solved,int unsolved)
{
    public int Id { get; set; } = id;
    public string Name => SolvedLevel.Names[Id];
    public string Color => SolvedLevel.Colors[Id];
    public int Total { get; set; } = total;
    public int Solved { get; set; } = solved;
    public int Unsolved { get; set; } = unsolved;
    public double SolvedPercentage => Total == 0 ? 0 : (double)Solved / Total * 100;
}