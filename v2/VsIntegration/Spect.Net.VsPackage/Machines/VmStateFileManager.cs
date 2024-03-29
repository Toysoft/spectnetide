﻿using Spect.Net.SpectrumEmu;
using Spect.Net.SpectrumEmu.Abstraction.Devices;
using Spect.Net.SpectrumEmu.Abstraction.Devices.Keyboard;
using Spect.Net.SpectrumEmu.Abstraction.Machine;
using Spect.Net.SpectrumEmu.Machine;
using Spect.Net.VsPackage.VsxLibrary;
using Spect.Net.VsPackage.VsxLibrary.Output;
using System;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods

namespace Spect.Net.VsPackage.Machines
{
    /// <summary>
    /// This class is responsible for managing VM files
    /// </summary>
    public static class VmStateFileManager
    {
        public const string VMSTATE_FOLDER = ".SpectNetIde/VmStates";

        /// <summary>
        /// Main Execution cycle loop in Spectrum 48 ROM-0/Spectrum 128 ROM-1
        /// </summary>
        public const ushort SP48_MAIN_EXEC_ADDR = 0x12ac;

        /// <summary>
        /// Main Waiting Loop in Spectrum 128 ROM-0
        /// </summary>
        public const ushort SP128_MAIN_WAITING_LOOP = 0x2653;

        /// <summary>
        /// Return to Editor entry point in Spectrum 128 ROM-0
        /// </summary>
        public const ushort SP128_RETURN_TO_EDITOR = 0x2604;

        /// <summary>
        /// Main Waiting Loop in Spectrum +3E ROM-0
        /// </summary>
        public const ushort SPP3_MAIN_WAITING_LOOP = 0x0706;

        /// <summary>
        /// Return to Editor entry point in Spectrum +3E ROM-0
        /// </summary>
        public const ushort SPP3_RETURN_TO_EDITOR = 0x0937;

        /// <summary>
        /// Time to wait after an emulated menu key has been pressed
        /// </summary>
        public const int WAIT_FOR_MENU_KEY = 250;

        /// <summary>
        /// Number of frames an emulated key is held down
        /// </summary>
        public const int KEY_PRESS_FRAMES = 5;

        public const string SPECTRUM_48_STARTUP = "_sp48.startup.vmstate";
        public const string SPECTRUM_128_STARTUP_48 = "_sp128.startup.48.vmstate";
        public const string SPECTRUM_128_STARTUP_128 = "_sp128.startup.128.vmstate";
        public const string SPECTRUM_P3_STARTUP_48 = "_spP3.startup.48.vmstate";
        public const string SPECTRUM_P3_STARTUP_P3 = "_spP3.startup.P3.vmstate";

        /// <summary>
        /// The package that host the project
        /// </summary>
        private static SpectNetPackage HostPackage => SpectNetPackage.Default;

        /// <summary>
        /// Obtains the current model's name
        /// </summary>
        private static string ModelName => HostPackage.Solution.ActiveProject.ModelName;

        /// <summary>
        /// Obtains the Spectrum virtual machine controller this manager is bound to
        /// </summary>
        private static ISpectrumVmController VmController => HostPackage.EmulatorViewModel.Machine;

        /// <summary>
        /// Obtains the Spectrum virtual machine this manager is bound to
        /// </summary>
        private static ISpectrumVm SpectrumVm => HostPackage.EmulatorViewModel.Machine.SpectrumVm;

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogMessage(string message)
        {
            var pane = OutputWindow.GetPane<SpectrumVmOutputPane>();
            pane.WriteLine(message);
        }

        /// <summary>
        /// Get the name of the folder to save/load machine state files
        /// </summary>
        /// <returns></returns>
        public static string GetStateFolder()
        {
            var solution = SpectNetPackage.Default.Solution.Root;
            var folder = Path.GetDirectoryName(solution.FileName);
            if (folder == null)
            {
                throw new InvalidOperationException("Project root folder seems to be null.");
            }
            var stateFolder = Path.Combine(folder, VMSTATE_FOLDER);
            return stateFolder;
        }

        /// <summary>
        /// Forces the virtual machine to paused state
        /// </summary>
        public static void ForcePausedState()
            => SpectNetPackage.Default.EmulatorViewModel.ForcePauseVmAfterStateRestore();

        /// <summary>
        /// Define how to reset devices after load
        /// </summary>
        public static void ResetDevicesAfterLoad()
        {
            SpectNetPackage.Default.EmulatorViewModel.Machine.SpectrumVm.BeeperDevice.Reset();
            SpectNetPackage.Default.EmulatorViewModel.Machine.SpectrumVm.BeeperProvider.Reset();
        }

        /// <summary>
        /// Responds to the event when an invalid machine state has been detected
        /// </summary>
        /// <param name="e">Exception instance</param>
        public static void OnInvalidVmMachineStateException(InvalidVmStateException e)
        {
            VsxDialogs.Show(e.OriginalMessage, "Error loading virtual machine state");
        }

        /// <summary>
        /// Responds to the event when an loading the machine state raises an exception
        /// </summary>
        /// <param name="e">Exception instance</param>
        public static void OnLoadVmException(Exception e)
        {
            VsxDialogs.Show($"Unexpected error: {e.Message}", "Error loading virtual machine state");
        }

                                 /// <summary>
                                 /// Prepares a Spectrum virtual machine of the current project for code injection
                                 /// </summary>
                                 /// <param name="sp48Mode">
                                 /// Indicates if machine should run in Spectrum 48K mode
                                 /// </param>
        public static async Task<bool> SetProjectMachineStartupState(bool sp48Mode)
        {
            switch (ModelName)
            {
                case SpectrumModels.ZX_SPECTRUM_128:
                    if (sp48Mode)
                    {
                        return await SetSpectrum128In48StartupState();
                    }
                    else
                    {
                        return await SetSpectrum128In128StartupState();
                    }

                case SpectrumModels.ZX_SPECTRUM_P3_E:
                    if (sp48Mode)
                    {
                        return await SetSpectrumP3In48StartupState();
                    }
                    else
                    {
                        return await SetSpectrumP3InP3StartupState();
                    }

                case SpectrumModels.ZX_SPECTRUM_NEXT:
                    return false;

                default:
                    return await SetSpectrum48StartupState();
            }
        }

        /// <summary>
        /// Prepares a Spectrum 48 virtual machine for code injection
        /// </summary>
        public static async Task<bool> SetSpectrum48StartupState()
        {
            return await SetSpectrumVmStartupState(SPECTRUM_48_STARTUP, CreateSpectrum48StartupState);
        }

        /// <summary>
        /// Prepares a Spectrum 128 virtual machine for code injection in Spectrum 48 mode
        /// </summary>
        public static async Task<bool> SetSpectrum128In48StartupState()
        {
            return await SetSpectrumVmStartupState(SPECTRUM_128_STARTUP_48, CreateSpectrum128Startup48State);
        }

        /// <summary>
        /// Prepares a Spectrum 128 virtual machine for code injection in Spectrum 128 mode
        /// </summary>
        public static async Task<bool> SetSpectrum128In128StartupState()
        {
            return await SetSpectrumVmStartupState(SPECTRUM_128_STARTUP_128, CreateSpectrum128Startup128State);
        }

        /// <summary>
        /// Prepares a Spectrum +3 virtual machine for code injection in Spectrum 48 mode
        /// </summary>
        public static async Task<bool> SetSpectrumP3In48StartupState()
        {
            return await SetSpectrumVmStartupState(SPECTRUM_P3_STARTUP_48, CreateSpectrumP3Startup48State);
        }

        /// <summary>
        /// Prepares a Spectrum +3 virtual machine for code injection in Spectrum +3 mode
        /// </summary>
        public static async Task<bool> SetSpectrumP3InP3StartupState()
        {
            return await SetSpectrumVmStartupState(SPECTRUM_P3_STARTUP_P3, CreateSpectrumP3StartupP3State);
        }

        /// <summary>
        /// Prepres a .vmstate file for loading in the future
        /// </summary>
        /// <param name="vmFile">Name of the .vmstate file within the solution folder</param>
        /// <param name="createAction"></param>
        /// <returns>True, if state successfully restored</returns>
        private static async Task<bool> SetSpectrumVmStartupState(string vmFile, Func<ISpectrumVmController, 
            Task<bool>> createAction)
        {
            // --- We cannot set the desired state if the machine is running
            CheckMachineState();

            // --- Check, if the virtual machine state file exists
            var stateFolder = GetStateFolder();
            var stateFile = Path.Combine(stateFolder, vmFile);
            LogMessage($"Checking VMSTATE file {stateFile}");
            if (File.Exists(stateFile))
            {
                // --- Use the existing state file
                LogMessage($"Loading {stateFile}");
                LoadVmStateFile(stateFile);
                LogMessage("Virtual machine state restored.");
                return true;
            }

            // --- Create the new virtual machine startup state
            LogMessage("Creating virtual machine startup state.");
            var result = await createAction(VmController);

            // --- Save the new state file
            if (result)
            {
                LogMessage($"Saving {stateFile}");
                SaveVmStateFile(stateFile);
            }
            return result;
        }

        /// <summary>
        /// Loads the specified .vmstate file
        /// </summary>
        /// <param name="stateFile">Full name of the .vmstate file</param>
        public static void LoadVmStateFile(string stateFile)
        {
            CheckMachineState();
            var state = File.ReadAllText(stateFile);
            try
            {
                SpectrumVm.SetVmState(state, ModelName);
                ResetDevicesAfterLoad();
                LogMessage($"Forcing Paused state from {VmController.MachineState}");
                ForcePausedState();
            }
            catch (InvalidVmStateException e)
            {
                OnInvalidVmMachineStateException(e);
            }
            catch (Exception e)
            {
                OnLoadVmException(e);
            }
        }

        /// <summary>
        /// Saves the virtual machine state to the specified file
        /// </summary>
        /// <param name="stateFile">File to save the virtual machine state</param>
        public static void SaveVmStateFile(string stateFile)
        {
            CheckMachineState();
            var stateFolder = Path.GetDirectoryName(stateFile);
            var newState = SpectrumVm.GetVmState(ModelName);

            if (!Directory.Exists(stateFolder))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(stateFolder);
            }
            File.WriteAllText(stateFile, newState);
        }

        #region Helpers

        /// <summary>
        /// Checks if machine is in a valid state for VMSTATE operations
        /// </summary>
        private static void CheckMachineState()
        {
            var machineState = VmController.MachineState;
            if (machineState != VmState.Stopped
                && machineState != VmState.None
                && machineState != VmState.Paused
                && machineState != VmState.Pausing)
            {
                throw new InvalidOperationException($"Virtual machine is in an unexpected state: {machineState}");
            }
        }

        /// <summary>
        /// This method recreates the .vmstate file for a Spectrum 48 virtual machine startup
        /// </summary>
        /// <param name="controller">Virtual machine controller</param>
        /// <returns>True, if preparation successful; otherwise, true</returns>
        private static async Task<bool> CreateSpectrum48StartupState(ISpectrumVmController controller)
        {
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SP48_MAIN_EXEC_ADDR,
                fastTapeMode: true,
                fastVmMode: true));
            return await WaitForTerminationPoint();
        }

        /// <summary>
        /// This method recreates the .vmstate file for a Spectrum 128 virtual machine 
        /// in Spectrum 48 startup mode
        /// </summary>
        /// <param name="controller">Virtual machine controller</param>
        /// <returns>True, if preparation successful; otherwise, true</returns>
        private static async Task<bool> CreateSpectrum128Startup48State(ISpectrumVmController controller)
        {
            // --- Wait while the main menu appears
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SP128_MAIN_WAITING_LOOP,
                fastTapeMode: true,
                fastVmMode: true));
            if (!await WaitForTerminationPoint()) return false;
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 1,
                terminationPoint: SP48_MAIN_EXEC_ADDR,
                fastTapeMode: true,
                fastVmMode: true));

            // --- Move to Spectrum 48 mode
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.Enter);
            return await WaitForTerminationPoint();
        }

        /// <summary>
        /// This method recreates the .vmstate file for a Spectrum 128 virtual machine 
        /// in Spectrum 128 startup mode
        /// </summary>
        /// <param name="controller">Virtual machine controller</param>
        /// <returns>True, if preparation successful; otherwise, true</returns>
        private static async Task<bool> CreateSpectrum128Startup128State(ISpectrumVmController controller)
        {
            // --- Wait while the main menu appears
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SP128_MAIN_WAITING_LOOP,
                fastTapeMode: true,
                fastVmMode: true));
            if (!await WaitForTerminationPoint()) return false;
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SP128_RETURN_TO_EDITOR,
                fastTapeMode: true,
                fastVmMode: true));

            // --- Move to Spectrum 128 mode
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.Enter);
            return await WaitForTerminationPoint();
        }

        /// <summary>
        /// This method recreates the .vmstate file for a Spectrum +3 virtual machine 
        /// in Spectrum 48 startup mode
        /// </summary>
        /// <param name="controller">Virtual machine controller</param>
        /// <returns>True, if preparation successful; otherwise, true</returns>
        private static async Task<bool> CreateSpectrumP3Startup48State(ISpectrumVmController controller)
        {
            // --- Wait while the main menu appears
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SPP3_MAIN_WAITING_LOOP,
                fastTapeMode: true,
                fastVmMode: true));
            if (!await WaitForTerminationPoint()) return false;
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 3,
                terminationPoint: SP48_MAIN_EXEC_ADDR,
                fastTapeMode: true,
                fastVmMode: true));

            // --- Move to Spectrum 48 mode
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.Enter);
            return await WaitForTerminationPoint();
        }

        /// <summary>
        /// This method recreates the .vmstate file for a Spectrum +3 virtual machine 
        /// in Spectrum +3 startup mode
        /// </summary>
        /// <param name="controller">Virtual machine controller</param>
        /// <returns>True, if preparation successful; otherwise, true</returns>
        private static async Task<bool> CreateSpectrumP3StartupP3State(ISpectrumVmController controller)
        {
            // --- Wait while the main menu appears
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SPP3_MAIN_WAITING_LOOP,
                fastTapeMode: true,
                fastVmMode: true));
            if (!await WaitForTerminationPoint()) return false;
            controller.StartWithOptions(new ExecuteCycleOptions(EmulationMode.UntilExecutionPoint,
                terminationRom: 0,
                terminationPoint: SPP3_RETURN_TO_EDITOR,
                fastTapeMode: true,
                fastVmMode: true));

            // --- Move to Spectrum +3 mode
            QueueKeyStroke(SpectrumKeyCode.N6, SpectrumKeyCode.CShift);
            await Task.Delay(WAIT_FOR_MENU_KEY);
            QueueKeyStroke(SpectrumKeyCode.Enter);
            return await WaitForTerminationPoint();
        }

        /// <summary>
        /// Waits while the Spectrum virtual machine starts and reaches its termination point
        /// </summary>
        /// <returns>True, if started within timeout; otherwise, false</returns>
        private static async Task<bool> WaitForTerminationPoint()
        {
            const int TIME_OUT_IN_SECONDS = 5;

            await Task.WhenAny(VmController.CompletionTask, Task.Delay(TIME_OUT_IN_SECONDS * 1000));
            if (VmController.CompletionTask.IsCompleted && VmController.MachineState == VmState.Paused)
            {
                return true;
            }

            var message = $"The ZX Spectrum virtual machine did not start within {TIME_OUT_IN_SECONDS} seconds.";
            LogMessage(message);
            await VmController.Stop();
            return false;
        }

        /// <summary>
        /// Enques an emulated key stroke
        /// </summary>
        /// <param name="primaryCode">Primary key code</param>
        /// <param name="secondaryCode">Secondary key code</param>
        private static void QueueKeyStroke(SpectrumKeyCode primaryCode,
            SpectrumKeyCode? secondaryCode = null)
        {
            if (SpectrumVm == null) return;

            var currentTact = SpectrumVm.Cpu.Tacts;
            var lastTact = currentTact + SpectrumVm.FrameTacts * KEY_PRESS_FRAMES * SpectrumVm.ClockMultiplier;

            SpectrumVm.KeyboardProvider.QueueKeyPress(
                new EmulatedKeyStroke(
                    currentTact,
                    lastTact,
                    primaryCode,
                    secondaryCode));
        }

        #endregion
    }
}

#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
