namespace VSPRExtension
{
    using System;
    using System.Windows.Forms;
    using Extensibility;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.CommandBars;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPRBase;
    using VSPRBase.Common;
    using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private DTE2 _applicationObject;
        private AddIn _addInInstance;

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;
            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;

                //Place the command on the tools menu.
                //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

                //Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[Constants.ToolsMenuName];
                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;
                // get popUp command bars where commands will be registered.
                CommandBars commandBars = (CommandBars)(_applicationObject.CommandBars);
                CommandBar vsBarItem = commandBars["Item"]; //the pop up for clicking a project Item
                CommandBar vsBarWebItem = commandBars["Web Item"];
                CommandBar vsBarMultiItem = commandBars["Cross Project Multi Item"];
                CommandBar vsBarFolder = commandBars["Folder"];
                CommandBar vsBarWebFolder = commandBars["Web Folder"];
                CommandBar solutionExplorerContextMenu = commandBars[Constants.Project]; //the popUpMenu for right clicking a project
                CommandBar vsBarProjectNode = commandBars["Project Node"];

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, Constants.RenameProjectCommand, "Rename Project", "Renames the selected project", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((command != null) && (toolsPopup != null))
                    {
                        //command.AddControl(toolsPopup.CommandBar, 1);
                        command.AddControl(solutionExplorerContextMenu);
                    }
                }
                catch (System.ArgumentException argEx)
                {
                    System.Diagnostics.Debug.Write("Exception in HintPaths:" + argEx.ToString());
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
                }
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == Constants.RenameProjectFullCommand)
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == Constants.RenameProjectFullCommand)
                {
                    handled = RenameProject();
                    return;
                }
            }
        }

        private bool RenameProject()
        {
            try
            {
                string solutionPath = null;
                string oldProjectName = null;
                string newProjectName = null;
                string oldProjectFullName = null;

                // Get the path to the solution
                Solution solution = GetSolution();
                if (solution != null)
                {
                    solutionPath = solution.FullName;
                }

                // Get the path to the project, that is going to be renamed
                Project project = GetCurrentProject();
                if (project != null)
                {
                    oldProjectName = project.Name;
                    oldProjectFullName = project.FullName;
                }

                // Unload the project so we dont get file locks on the folder when renaming
                //project.DTE.SourceControl.IsItemCheckedOut("test");
                Guid guid = UnloadSelectedProject(project);


                newProjectName = GetNewProjectName();
                string newProjectFullName = newProjectName + @"\" + newProjectName;
                if (!string.IsNullOrEmpty(newProjectName) && !string.IsNullOrEmpty(oldProjectName) && !string.IsNullOrEmpty(newProjectName))
                {
                   // StandardRenamer renamer = new StandardRenamer();
                   // renamer.Rename(solutionPath, oldProjectName, newProjectName, false);
                }

                //Guid newGuid = GetProjectGuid(newProjectFullName);

                ReloadRenamedProject(newProjectFullName);

            }
            catch (Exception exception)
            {
                ShowErrorMessage(exception.Message);
            }

            return true;
        }

        private void ReloadRenamedProject(string newProjectFullName)
        {
            if (_applicationObject.SelectedItems.Count == 1 && _applicationObject.SelectedItems.Item(1).Project != null)
            {
                Project pro = _applicationObject.SelectedItems.Item(1).Project;
                IVsSolution2 _solution = (IVsSolution2)Package.GetGlobalService(typeof(SVsSolution));

                Guid rguidEnumOnlyThisType = new Guid();
                IEnumHierarchies ppenum;
                _solution.GetProjectEnum(0u, ref rguidEnumOnlyThisType, out ppenum);


                uint pcProjectsFetched;
                _solution.GetProjectFilesInSolution(0u, 0u, new string[] { }, out pcProjectsFetched);


                IVsHierarchy ppHierarchy;
                _solution.GetProjectOfUniqueName(newProjectFullName, out ppHierarchy);

                object pvar;
                _solution.GetProperty(0, out pvar);



                Guid guid = GetProjectGuid(pro);

                //_solution.ReloadProject(guid);
            }
            //IVsSolution2 _solution = (IVsSolution2)Package.GetGlobalService(typeof(SVsSolution));

            // IVsHierarchy hierarchy;
            //_solution.GetProjectOfUniqueName(newProjectFullName, out hierarchy);

            //Guid guid;
            //_solution.GetGuidOfProject(hierarchy, out guid);

            //UIHierarchy uiHierarchy;
            //UIHierarchyItem uiHierarchyItem;
            //uiHierarchy =  (UIHierarchy)_applicationObject.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
            //object items = uiHierarchy.SelectedItems;


            //uiHierarchyItem = uiHierarchy.GetItem(newProjectFullName);
            //uiHierarchyItem.Select(vsUISelectionType.vsUISelectionTypeSelect);
            //this._applicationObject.ExecuteCommand("Project.UnloadProject");

            this._applicationObject.ExecuteCommand("Project.ReloadProject");

            //Solution sol = _applicationObject.Solution;
            //Guid guid = Guid.Parse(sol.Projects.Kind);

            //IVsSolution4 solution = (IVsSolution4)Package.GetGlobalService(typeof(SVsSolution));
            //solution.ReloadProject(ref guid);
            // IVsHierarchy hierarchy;

            //uint projectCount = 0;
            //int hr = solution.GetProjectFilesInSolution((uint)__VSGETPROJFILESFLAGS.GPFF_SKIPUNLOADEDPROJECTS, 0, null, out projectCount);
            //Debug.Assert(hr == VSConstants.S_OK, "GetProjectFilesInSolution failed.");

            //string[] projectNames = new string[projectCount];
            //hr = solution.GetProjectFilesInSolution((uint)__VSGETPROJFILESFLAGS.GPFF_SKIPUNLOADEDPROJECTS, projectCount, projectNames, out projectCount);
            //Debug.Assert(hr == VSConstants.S_OK, "GetProjectFilesInSolution failed.");
        }

        private static Guid UnloadSelectedProject(Project project)
        {
            IVsSolution4 _solution = (IVsSolution4)Package.GetGlobalService(typeof(SVsSolution));
            Guid guid = GetProjectGuid(project);

            _solution.UnloadProject(ref guid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);

            return guid;
        }

        private static Guid GetProjectGuid(Project project)
        {
            IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);
            if (hierarchy != null)
            {
                Guid projectGuid;
                ErrorHandler.ThrowOnFailure(
                    hierarchy.GetGuidProperty(
                        VSConstants.VSITEMID_ROOT,
                        (int)__VSHPROPID.VSHPROPID_ProjectIDGuid,
                        out projectGuid));
                if (projectGuid != null)
                {
                    return projectGuid;
                }
            }

            return Guid.Empty;
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK);
        }

        private string GetNewProjectName()
        {
            RenamerView renamerView = new RenamerView();
            DialogResult result = renamerView.ShowDialog();
            if (result.Equals(DialogResult.OK))
            {
                string newProjectName = renamerView.NewProjectName;
                renamerView.Dispose();

                return newProjectName;
            }

            return null;
        }

        private Solution GetSolution()
        {
            return _applicationObject.Solution;
        }

        private Project GetCurrentProject()
        {
            if (_applicationObject.Solution == null || _applicationObject.Solution.Projects == null || _applicationObject.Solution.Projects.Count < 1)
                return null;
            if (_applicationObject.SelectedItems.Count == 1 && _applicationObject.SelectedItems.Item(1).Project != null)
                return _applicationObject.SelectedItems.Item(1).Project;
            return null;
        }

    }
}