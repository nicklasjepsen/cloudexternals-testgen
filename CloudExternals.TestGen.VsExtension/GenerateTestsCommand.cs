using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CloudExternals.TestGen.Shared;
using CloudExternals.TestGen.Shared.OpenAI;
using CloudExternals.TestGen.VsExtension.Options;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Newtonsoft.Json;
using File = System.IO.File;
using Project = Community.VisualStudio.Toolkit.Project;

namespace CloudExternals.TestGen.VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateTestsCommand
    {
        public const string _projTemplateString = @"
<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>{{DOTNET_VERSION}}</TargetFramework>

    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.3.2"" />
    <PackageReference Include=""xunit"" Version=""2.4.2"" />    
    <PackageReference Include=""Moq"" Version=""4.18.4"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.5"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include=""coverlet.collector"" Version=""3.1.2"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>
";

        public static DTE2 _dte;
        public static TestGenerator _generator;
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("eefc6f6d-c9d7-4b53-bf7d-2fe48d8276ab");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateTestsCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GenerateTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ExecuteAsync, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateTestsCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in GenerateTestsCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Instance = new GenerateTestsCommand(package, commandService);
        }


        private async void ExecuteAsync(object sender, EventArgs e)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            var general = await General.GetLiveInstanceAsync();
            var apiKey = general.ApiKey;
            if (string.IsNullOrEmpty(general.ApiKey))
            {
                var model = new InfoBarModel(
                    new[] {
                        new InfoBarTextSpan("No API key found. Please go to Tools > Options > CloudExternals > OpenAI > API Key and enter your OpenAI API key.")
                    },
                    KnownMonikers.SettingsFileError,
                    true);

                var infoBar = await VS.InfoBar.CreateAsync(ToolWindowGuids80.SolutionExplorer, model);
                //infoBar.ActionItemClicked += InfoBar_ActionItemClicked;
                await infoBar.TryShowInfoBarUIAsync();
            }
            else
            {
                _generator = new TestGenerator(
                new OpenAIService(new HttpClient()
                {
                    BaseAddress = new Uri("https://api.openai.com"),
                    DefaultRequestHeaders = { { "Authorization", $"Bearer {apiKey}" } }
                }), new CodeParser(), new MarkdownParser());

                var selectedFiles = GetSourceFilePath()
                    .ToList();
                if (!selectedFiles.Any())
                {
                    var model = new InfoBarModel(
                        new[] {
                            new InfoBarTextSpan("Please select at least 1 .cs file.")
                        },
                        KnownMonikers.CSFile,
                        true);

                    var infoBar = await VS.InfoBar.CreateAsync(ToolWindowGuids80.SolutionExplorer, model);
                    //infoBar.ActionItemClicked += InfoBar_ActionItemClicked;
                    await infoBar.TryShowInfoBarUIAsync();
                }
                else
                {
                    var item = await PhysicalFile.FromFileAsync(selectedFiles.First());
                    var project = item.ContainingProject;
                    var settingsFile = Path.Combine(project.FullPath.Replace(project.Name + ".csproj", ""), "unittest.generator");
                    var settings = new TestGeneratorConfig
                    {
                        NameFormatForTestProject = "[Project].Tests",
                        TestProject = project.Name
                    };
                    if (File.Exists(settingsFile))
                    {
                        settings = JsonConvert.DeserializeObject<TestGeneratorConfig>(File.ReadAllText(settingsFile));
                    }
                    else
                    {
                        File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings));
                        await project.AddExistingFilesAsync(settingsFile);
                    }

                    var tsc = await VS.Services.GetTaskStatusCenterAsync();
                    var options = default(TaskHandlerOptions);
                    options.Title = "Generating unit tests...";
                    options.ActionsAfterCompletion = CompletionActions.None;

                    TaskProgressData data = default;
                    data.CanBeCanceled = true;

                    var handler = tsc.PreRegister(options, data);
                    var task = LongRunningTaskAsync(data, handler, selectedFiles, settings, project);
                    handler.RegisterTask(task);
                }
            }
        }

        private async Task LongRunningTaskAsync(TaskProgressData data, ITaskHandler handler,
            List<string> selectedFiles, TestGeneratorConfig settings, Project project)
        {
            // or from a synchronous method:
            VS.StatusBar.ShowMessageAsync("Generating unit tests...").FireAndForget();
            var testProjName = settings.NameFormatForTestProject
                .Replace("[Project]", project.Name);
            var testProjFileName = testProjName + ".csproj";
            var testProjDir = Directory.CreateDirectory(testProjName);

            var testProject = await VS.Solutions.FindProjectsAsync(testProjName);
            if (testProject == null)
            {
                File.WriteAllText(Path.Combine(testProjDir.FullName, testProjFileName), _projTemplateString
                    .Replace("{{DOTNET_VERSION}}", await project.GetAttributeAsync("TargetFramework")));

                _dte.Solution.AddFromFile(Path.Combine(testProjDir.FullName, testProjFileName));
                testProject = await VS.Solutions.FindProjectsAsync(testProjName);
                await testProject.References.AddAsync(project);
            }
            
            float totalSteps = selectedFiles.Count + 1;
            data.PercentComplete = (int)(1 / totalSteps * 100);
            handler.Progress.Report(data);
            VS.StatusBar.ShowMessageAsync("Generating unit tests...").FireAndForget();
            var tests = await _generator.GenerateTests(selectedFiles.ToArray());

            var c = 1;
            foreach (var generationResult in tests)
            {
                VS.StatusBar.ShowMessageAsync("Generating unit tests classes...").FireAndForget();
                data.PercentComplete = (int)(c / totalSteps * 100);
                handler.Progress.Report(data);
                for (var i = 0; i < generationResult.CodeBlocks.Count; i++)
                {
                    var fn = $"{generationResult.ClassName}_{generationResult.MethodName}_Tests.cs";
                    if (i > 0)
                        fn = $"{i}{fn}";

                    File.WriteAllText(Path.Combine(testProjDir.FullName, fn), generationResult.CodeBlocks[i]);
                    await testProject.AddExistingFilesAsync(Path.Combine(testProjDir.FullName, fn));
                }

                c++;
            }

            VS.StatusBar.ShowMessageAsync("Done generating unit tests.").FireAndForget();
        }

        //private void InfoBar_ActionItemClicked(object sender, InfoBarActionItemEventArgs e)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    _dte.ExecuteCommand("Tools.Options");
        //}

        private IEnumerable<string> GetSourceFilePath()
        {
            var uih = _dte.ToolWindows.SolutionExplorer;
            var selectedItems = (Array)uih.SelectedItems;
            if (null != selectedItems)
            {
                foreach (UIHierarchyItem selItem in selectedItems)
                {
                    var prjItem = selItem.Object as ProjectItem;
                    var filePath = prjItem.Properties.Item("FullPath").Value.ToString();
                    yield return filePath;
                }
            }
        }

        ///// <summary>
        ///// This function is the callback used to execute the command when the menu item is clicked.
        ///// See the constructor to see how the menu item is associated with this function using
        ///// OleMenuCommandService service and MenuCommand class.
        ///// </summary>
        ///// <param name="sender">Event sender.</param>
        ///// <param name="e">Event args.</param>
        //private void Execute(object sender, EventArgs e)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();
        //    string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
        //    string title = "GenerateTestsCommand";




        //    // Show a message box to prove we were here
        //    VsShellUtilities.ShowMessageBox(
        //        this.package,
        //        message,
        //        title,
        //        OLEMSGICON.OLEMSGICON_INFO,
        //        OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        //}
    }
}
