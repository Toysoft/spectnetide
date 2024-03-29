﻿using Spect.Net.Assembler.Assembler;
using Spect.Net.VsPackage.SolutionItems;
using Spect.Net.VsPackage.VsxLibrary;
using Spect.Net.VsPackage.VsxLibrary.Output;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Spect.Net.VsPackage.Compilers
{
    /// <summary>
    /// This class represents the ZX BASIC compiler
    /// </summary>
    public class ZxBasicCompiler : ICompilerService
    {
        private const string ZXB_NOT_FOUND_MESSAGE =
            "SpectNetIDE cannot run ZXB.EXE ({0}). Please check that you specified the " +
            "correct path in the Spect.Net IDE options page (ZXB utility path) or added it " +
            "to the PATH evnironment variable.";

        private const string ZXBASIC_TEMP_FOLDER = "ZxBasic";

        /// <summary>
        /// Tests if the compiler is available.
        /// </summary>
        /// <returns>True, if the compiler is installed, and so available.</returns>
        public async Task<bool> IsAvailable()
        {
            var runner = new ZxbRunner(SpectNetPackage.Default.Options.ZxbPath, 10000);
            try
            {
                await runner.RunAsync(new ZxbOptions());
            }
            catch (Exception ex)
            {
                VsxDialogs.Show(string.Format(ZXB_NOT_FOUND_MESSAGE, ex.Message),
                    "Error when running ZXB", MessageBoxButton.OK, VsxMessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        /// <summary>
        /// The name of the service
        /// </summary>
        public string ServiceName => "ZX BASIC Compiler";

        private EventHandler<AssemblerMessageArgs> _traceMessageHandler;

        /// <summary>
        /// Gets the handler that displays trace messages
        /// </summary>
        /// <returns>Trace message handler</returns>
        public EventHandler<AssemblerMessageArgs> GetTraceMessageHandler()
        {
            return _traceMessageHandler;
        }

        /// <summary>
        /// Sets the handler that displays trace messages
        /// </summary>
        /// <param name="messageHandler">Message handler to use</param>
        public void SetTraceMessageHandler(EventHandler<AssemblerMessageArgs> messageHandler)
        {
            _traceMessageHandler = messageHandler;
        }

        /// <summary>
        /// Compiles the specified Visua Studio document.
        /// </summary>
        /// <param name="itemPath">VS document item path</param>
        /// <param name="options">Assembler options to use</param>
        /// <param name="output">Assembler output</param>
        /// <returns>True, if compilation is successful; otherwise, false</returns>
        public async Task<AssemblerOutput> CompileDocument(string itemPath,
            AssemblerOptions options)
        {
            var zxbOptions = PrepareZxbOptions(itemPath);
            MergeOptionsFromSource(zxbOptions);
            var output = new AssemblerOutput(new SourceFileItem(itemPath));
            var runner = new ZxbRunner(SpectNetPackage.Default.Options.ZxbPath);
            var result = await runner.RunAsync(zxbOptions);
            if (result.ExitCode != 0)
            {
                // --- Compile error - stop here
                output.Errors.Clear();
                output.Errors.AddRange(result.Errors);
                return output;
            }

            // --- HACK: Take care that "ZXBASIC_HEAP_SIZE EQU" is added to the assembly file
            var asmContents = File.ReadAllText(zxbOptions.OutputFilename);
            var hasHeapSizeLabel = Regex.Match(asmContents, "ZXBASIC_HEAP_SIZE\\s+EQU");
            if (!hasHeapSizeLabel.Success)
            {
                asmContents = Regex.Replace(asmContents, "ZXBASIC_USER_DATA_END\\s+EQU\\s+ZXBASIC_MEM_HEAP",
                    "ZXBASIC_USER_DATA_END EQU ZXBASIC_USER_DATA");
                File.WriteAllText(zxbOptions.OutputFilename, asmContents);
            }

            // --- Second pass, compile the assembly file
            var compiler = new Z80Assembler();
            options.ProcExplicitLocalsOnly = true;
            if (_traceMessageHandler != null)
            {
                compiler.AssemblerMessageCreated += _traceMessageHandler;
            }
            compiler.AssemblerMessageCreated += OnAssemblerMessage;
            try
            {
                output = compiler.CompileFile(zxbOptions.OutputFilename, options);
                output.ModelType = SpectrumModelType.Spectrum48;
            }
            finally
            {
                if (_traceMessageHandler != null)
                {
                    compiler.AssemblerMessageCreated -= _traceMessageHandler;
                }
                compiler.AssemblerMessageCreated -= OnAssemblerMessage;
            }
            return output;
        }

        /// <summary>
        /// Responds to the event when the Z80 assembler releases a message
        /// </summary>
        private void OnAssemblerMessage(object sender, AssemblerMessageArgs e)
        {
            var pane = OutputWindow.GetPane<Z80AssemblerOutputPane>();
            pane.WriteLine(e.Message);
        }

        /// <summary>
        /// Prepares the ZXB options to run
        /// </summary>
        /// <returns></returns>
        private ZxbOptions PrepareZxbOptions(string documentPath)
        {
            var outputBase = Path.Combine(SpectNetPackage.Default.Solution.SolutionDir,
                SolutionStructure.PRIVATE_FOLDER, 
                ZXBASIC_TEMP_FOLDER,
                Path.GetFileName(documentPath));
            var outDir = Path.GetDirectoryName(outputBase);
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            var outputFile = Path.ChangeExtension(outputBase, ".z80asm");

            var packageOptions = SpectNetPackage.Default.Options;
            var options = new ZxbOptions
            {
                ProgramFilename = documentPath,
                OutputFilename = outputFile,
                AsmFormat = true,
                Optimize = packageOptions.Optimize,
                OrgValue = packageOptions.OrgValue,
                ArrayBaseOne = packageOptions.ArrayBaseOne,
                StringBaseOne = packageOptions.StringBaseOne,
                SinclairFlag = packageOptions.SinclairFlag,
                HeapSize = packageOptions.HeapSize,
                DebugMemory = packageOptions.DebugMemory,
                DebugArray = packageOptions.DebugArray,
                StrictBool = packageOptions.StrictBool,
                EnableBreak = packageOptions.EnableBreak,
                ExplicitDim = packageOptions.ExplicitDim,
                StrictTypes = packageOptions.StrictTypes
            };
            return options;
        }

        /// <summary>
        /// Parses the source code and merges options from head comment
        /// </summary>
        /// <param name="options"></param>
        private void MergeOptionsFromSource(ZxbOptions options)
        {
        }
    }
}
