﻿using Shouldly;
using Spect.Net.Z80Emu.Disasm;

namespace Spect.Net.Z80Emu.Test.Helpers
{
    public static class Z80Tester
    {
        public static void Test(string expected, params byte[] opCodes)
        {
            var project = new Z80DisAsmProject
            {
                Z80Binary = opCodes
            };

            var disasm = new Z80Disassembler(project);

            var output = disasm.Disassemble();
            output.OutputItems.Count.ShouldBe(1);
            output.OutputItems[0].Instruction.ToLower().ShouldBe(expected.ToLower());
        }
    }
}