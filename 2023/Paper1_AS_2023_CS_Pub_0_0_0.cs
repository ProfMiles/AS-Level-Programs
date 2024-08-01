// Skeleton Program for the AQA AS Summer 2023 examination
// this code should be used in conjunction with the Preliminary Material
// written by the AQA Programmer Team
// developed in Visual Studio 2019

// Version number: 0.0.0


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssemblerSimulator
{
  class Program
  {
    const string EMPTY_STRING = "";
    const int HI_MEM = 20;
    const int MAX_INT = 127; // 8 bits available for operand (two's complement integer)
    const int PC = 0;
    const int ACC = 1;
    const int STATUS = 2;
    const int TOS = 3;
    const int ERR = 4;

    struct AssemblerInstruction
    {
      public string opCode;
      public string operandString;
      public int operandValue;
    }

    private static void DisplayMenu()
    {
      Console.WriteLine();
      Console.WriteLine("Main Menu");
      Console.WriteLine("=========");
      Console.WriteLine("L - Load a program file");
      Console.WriteLine("D - Display source code");
      Console.WriteLine("E - Edit source code");
      Console.WriteLine("A - Assemble program");
      Console.WriteLine("R - Run the program");
      Console.WriteLine("X - Exit simulator");
      Console.WriteLine();
    }

    private static char GetMenuOption()
    {
      string choice = EMPTY_STRING;
      while (choice.Length != 1)
      {
        Console.Write("Enter your choice: ");
        choice = Console.ReadLine();
      }
      return choice[0];
    }

    private static void ResetSourceCode(string[] sourceCode)
    {
      for (int lineNumber = 0; lineNumber < HI_MEM; lineNumber++)
      {
        sourceCode[lineNumber] = EMPTY_STRING;
      }
    }

    private static void ResetMemory(AssemblerInstruction[] memory)
    {
      for (int lineNumber = 0; lineNumber < HI_MEM; lineNumber++)
      {
        memory[lineNumber].opCode = EMPTY_STRING;
        memory[lineNumber].operandString = EMPTY_STRING;
        memory[lineNumber].operandValue = 0;
      }
    }

    private static void DisplaySourceCode(string[] sourceCode)
    {
      Console.WriteLine();
      int numberOfLines = Convert.ToInt32(sourceCode[0]);
      for (int lineNumber = 0; lineNumber < numberOfLines + 1; lineNumber++)
      {
        Console.WriteLine($"{lineNumber,2} {sourceCode[lineNumber],-40}");
      }
      Console.WriteLine();
    }

    private static void LoadFile(string[] sourceCode)
    {
      bool fileExists = false;
      int lineNumber = 0;
      string fileName;
      ResetSourceCode(sourceCode);
      Console.Write("Enter filename to load: ");
      fileName = Console.ReadLine();
      try
      {
        StreamReader fileIn = new StreamReader(fileName + ".txt");
        {
          fileExists = true;
          string instruction = fileIn.ReadLine();
          while (instruction != null )
          {
            lineNumber++;
            sourceCode[lineNumber] = instruction;
            instruction = fileIn.ReadLine();
          }
          fileIn.Close();
          sourceCode[0] = lineNumber.ToString();
        }
      }
      catch (Exception)
      {
        if (!fileExists)
        {
          Console.WriteLine("Error Code 1");
        }
        else
        {
          Console.WriteLine("Error Code 2");
          sourceCode[0] = (lineNumber - 1).ToString();
        }
      }
      if (lineNumber > 0)
      {
        DisplaySourceCode(sourceCode);
      }
    }

    private static void EditSourceCode(string[] sourceCode)
    {
      int lineNumber = 0;
      Console.Write("Enter line number of code to edit: ");
      lineNumber = Convert.ToInt32(Console.ReadLine());
      Console.WriteLine(sourceCode[lineNumber]);
      string choice = EMPTY_STRING;
      while (choice != "C")
      {
        choice = EMPTY_STRING;
        while (choice != "E" && choice != "C")
        {
          Console.WriteLine("E - Edit this line");
          Console.WriteLine("C - Cancel edit");
          Console.Write("Enter your choice: ");
          choice = Console.ReadLine();
        }
        if (choice == "E")
        {
          Console.Write("Enter the new line: ");
          sourceCode[lineNumber] = Console.ReadLine();
        }
        DisplaySourceCode(sourceCode);
      }
    }

    private static void UpdateSymbolTable(Dictionary<string, int> symbolTable, string thisLabel, int lineNumber)
    {
      if (symbolTable.ContainsKey(thisLabel))
      {
        Console.WriteLine("Error Code 3");
      }
      else
      {
        symbolTable.Add(thisLabel, lineNumber);
      }
    }

    private static void ExtractLabel(string instruction, int lineNumber, AssemblerInstruction[] memory, Dictionary<string, int> symbolTable)
    {
      if (instruction.Length > 0)
      {
        string thisLabel = instruction.Substring(0, 5);
        thisLabel = thisLabel.Trim();
        if (thisLabel != EMPTY_STRING)
        {
          if (instruction[5] != ':')
          {
            Console.WriteLine("Error Code 4");
            memory[0].opCode = "ERR";
          }
          else
          {
            UpdateSymbolTable(symbolTable, thisLabel, lineNumber);
          }
        }
      }
    }

    private static void ExtractOpCode(string instruction, int lineNumber, AssemblerInstruction[] memory)
    {
      if (instruction.Length > 9)
      {
        string[] opCodeValues = new string[] { "LDA", "STA", "LDA#", "HLT", "ADD", "JMP", "SUB", "CMP#", "BEQ", "SKP", "JSR", "RTN", "   " };
        string operation = instruction.Substring(7, 3);
        if (instruction.Length > 10)
        {
          string addressMode = instruction.Substring(10, 1);

          if (addressMode == "#")
          {
            operation += addressMode;
          }
        }
        if (opCodeValues.Contains(operation))
        {
          memory[lineNumber].opCode = operation;
        }
        else
        {
          if (operation != EMPTY_STRING)
          {
            Console.WriteLine("Error Code 5");
            memory[0].opCode = "ERR";
          }
        }
      }
    }

    private static void ExtractOperand(string instruction, int lineNumber, AssemblerInstruction[] memory)
    {
      if (instruction.Length >= 13)
      {
        string operand = instruction.Substring(12);
        int thisPosition = -1;
        for (int position = 0; position < operand.Length; position++)
        {
          if (operand[position] == '*')
          {
            thisPosition = position;
          }
        }
        if (thisPosition >= 0)
        {
          operand = operand.Substring(0, thisPosition);
        }
        operand = operand.Trim();
        memory[lineNumber].operandString = operand;
      }
    }

    private static void PassOne(string[] sourceCode, AssemblerInstruction[] memory, Dictionary<string, int> symbolTable)
    {
      int numberOfLines = Convert.ToInt32(sourceCode[0]);
      for (int lineNumber = 1; lineNumber <= numberOfLines; lineNumber++)
      {
        string instruction = sourceCode[lineNumber];
        ExtractLabel(instruction, lineNumber, memory, symbolTable);
        ExtractOpCode(instruction, lineNumber, memory);
        ExtractOperand(instruction, lineNumber, memory);
      }
    }

    private static void PassTwo(AssemblerInstruction[] memory, Dictionary<string, int> symbolTable, int numberOfLines)
    {
      for (int lineNumber = 1; lineNumber <= numberOfLines; lineNumber++)
      {
        string operand = memory[lineNumber].operandString;
        if (operand != EMPTY_STRING)
        {
          if (symbolTable.ContainsKey(operand))
          {
            int operandValue = symbolTable[operand];
            memory[lineNumber].operandValue = operandValue;
          }
          else
          {
            try
            {
              int operandValue = Convert.ToInt32(operand);
              memory[lineNumber].operandValue = operandValue;
            }
            catch (Exception)
            {
              Console.WriteLine("Error Code 6");
              memory[0].opCode = "ERR";
            }
          }
        }
      }
    }

    private static void DisplayMemoryLocation(AssemblerInstruction[] memory, int location)
    {
      Console.Write($"*  {memory[location].opCode,-5}{memory[location].operandValue,-5} | ");
    }

    private static void DisplaySourceCodeLine(string[] sourceCode, int location)
    {
      Console.WriteLine($"{location,3}  |  {sourceCode[location],-40}");
    }

    private static void DisplayCode(string[] sourceCode, AssemblerInstruction[] memory)
    {
      Console.WriteLine("*  Memory     Location  Label  Op   Operand Comment");
      Console.WriteLine("*  Contents                    Code");
      int numberOfLines = Convert.ToInt32(sourceCode[0]);
      DisplayMemoryLocation(memory, 0);
      Console.WriteLine("  0  |");
      for (int location = 1; location < numberOfLines + 1; location++)
      {
        DisplayMemoryLocation(memory, location);
        DisplaySourceCodeLine(sourceCode, location);
      }
    }

    private static void Assemble(string[] sourceCode, AssemblerInstruction[] memory)
    {
      ResetMemory(memory);
      int numberOfLines = Convert.ToInt32(sourceCode[0]);
      Dictionary<string, int> symbolTable = new Dictionary<string, int>();
      PassOne(sourceCode, memory, symbolTable);
      if (memory[0].opCode != "ERR")
      {
        memory[0].opCode = "JMP";
        if (symbolTable.ContainsKey("START"))
        {
          memory[0].operandValue = symbolTable["START"];
        }
        else
        {
          memory[0].operandValue = 1;
        }
        PassTwo(memory, symbolTable, numberOfLines);
      }
    }

    private static string ConvertToBinary(int decimalNumber)
    {
      string binaryString = EMPTY_STRING, bit;
      int remainder;
      while (decimalNumber > 0)
      {
        remainder = decimalNumber % 2;
        bit = remainder.ToString();
        binaryString = bit + binaryString;
        decimalNumber = decimalNumber / 2;
      }
      while (binaryString.Length < 3)
      {
        binaryString = "0" + binaryString;
      }
      return binaryString;
    }

    private static int ConvertToDecimal(string binaryString)
    {
      int decimalNumber = 0, bitValue;
      foreach (char bit in binaryString)
      {
        bitValue = Convert.ToInt32(bit.ToString());
        decimalNumber = decimalNumber * 2 + bitValue;
      }
      return decimalNumber;
    }

    private static void DisplayFrameDelimiter(int frameNumber)
    {
      if (frameNumber == -1)
      {
        Console.WriteLine("***************************************************************");
      }
      else
      {
        Console.WriteLine($"****** Frame {frameNumber} ************************************************");
      }
    }

    private static void DisplayCurrentState(string[] sourceCode, AssemblerInstruction[] memory, int[] registers)
    {
      Console.WriteLine("*");
      DisplayCode(sourceCode, memory);
      Console.WriteLine("*");
      Console.WriteLine($"*  PC:  {registers[PC]}  ACC:  {registers[ACC]}  TOS:  {registers[TOS]}");
      Console.WriteLine("*  Status Register: ZNV");
      Console.WriteLine($"*                   {ConvertToBinary(registers[STATUS])}");
      DisplayFrameDelimiter(-1);
    }

    private static void SetFlags(int value, int[] registers)
    {
      if (value == 0)
      {
        registers[STATUS] = ConvertToDecimal("100");
      }
      else if (value < 0)
      {
        registers[STATUS] = ConvertToDecimal("010");
      }
      else if (value > MAX_INT || value < -(MAX_INT + 1))
      {
        registers[STATUS] = ConvertToDecimal("001");
      }
      else
      {
        registers[STATUS] = ConvertToDecimal("000");
      }
    }

    private static void ReportRunTimeError(string errorMessage, int[] registers)
    {
      Console.WriteLine($"Run time error: {errorMessage}");
      registers[ERR] = 1;
    }

    private static void ExecuteLDA(AssemblerInstruction[] memory, int[] registers, int address)
    {
      registers[ACC] = memory[address].operandValue;
      SetFlags(registers[ACC], registers);
    }

    private static void ExecuteSTA(AssemblerInstruction[] memory, int[] registers, int address)
    {
      memory[address].operandValue = registers[ACC];
    }

    private static void ExecuteLDAimm(int[] registers, int operand)
    {
      registers[ACC] = operand;
      SetFlags(registers[ACC], registers);
    }

    private static void ExecuteADD(AssemblerInstruction[] memory, int[] registers, int address)
    {
      registers[ACC] = registers[ACC] + memory[address].operandValue;
      SetFlags(registers[ACC], registers);
      if (registers[STATUS] == ConvertToDecimal("001"))
      {
        ReportRunTimeError("Overflow", registers);
      }
    }

    private static void ExecuteSUB(AssemblerInstruction[] memory, int[] registers, int address)
    {
      registers[ACC] = registers[ACC] - memory[address].operandValue;
      SetFlags(registers[ACC], registers);
      if (registers[STATUS] == ConvertToDecimal("001"))
      {
        ReportRunTimeError("Overflow", registers);
      }
    }

    private static void ExecuteCMPimm(int[] registers, int operand)
    {
      int value = registers[ACC] - operand;
      SetFlags(value, registers);
    }

    private static void ExecuteBEQ(int[] registers, int address)
    {
      string statusRegister = ConvertToBinary(registers[STATUS]);
      char flagZ = statusRegister[0];
      if (flagZ == '1')
      {
        registers[PC] = address;
      }
    }

    private static void ExecuteJMP(int[] registers, int address)
    {
      registers[PC] = address;
    }

    private static void ExecuteSKP()
    {
    }

    private static void DisplayStack(AssemblerInstruction[] memory, int[] registers)
    {
      Console.WriteLine("Stack contents:");
      Console.WriteLine(" ----");
      for (int index = registers[TOS]; index < HI_MEM; index++)
      {
        Console.WriteLine($"| {memory[index].operandValue,2} |");
      }
      Console.WriteLine(" ----");
    }

    private static void ExecuteJSR(AssemblerInstruction[] memory, int[] registers, int address)
    {
      int stackPointer = registers[TOS] - 1;
      memory[stackPointer].operandValue = registers[PC];
      registers[PC] = address;
      registers[TOS] = stackPointer;
      DisplayStack(memory, registers);
    }

    private static void ExecuteRTN(AssemblerInstruction[] memory, int[] registers)
    {
      int stackPointer = registers[TOS];
      registers[TOS] = registers[TOS] + 1;
      registers[PC] = memory[stackPointer].operandValue;
    }

    private static void Execute(string[] sourceCode, AssemblerInstruction[] memory)
    {
      int[] registers = new int[] { 0, 0, 0, 0, 0 };
      int frameNumber = 0, operand = 0;
      SetFlags(registers[ACC], registers);
      registers[PC] = 0;
      registers[TOS] = HI_MEM;
      DisplayFrameDelimiter(frameNumber);
      DisplayCurrentState(sourceCode, memory, registers);
      string opCode = memory[registers[PC]].opCode;
      while (opCode != "HLT")
      {
        frameNumber++;
        Console.WriteLine();
        DisplayFrameDelimiter(frameNumber);
        operand = memory[registers[PC]].operandValue;
        Console.WriteLine($"*  Current Instruction Register:  {opCode} {operand}");
        registers[PC] = registers[PC] + 1;
        switch (opCode)
        {
          case "LDA":
            ExecuteLDA(memory, registers, operand); break;
          case "STA":
            ExecuteSTA(memory, registers, operand); break;
          case "LDA#":
            ExecuteLDAimm(registers, operand); break;
          case "ADD":
            ExecuteADD(memory, registers, operand); break;
          case "JMP":
            ExecuteJMP(registers, operand); break;
          case "JSR":
            ExecuteJSR(memory, registers, operand); break;
          case "CMP#":
            ExecuteCMPimm(registers, operand); break;
          case "BEQ":
            ExecuteBEQ(registers, operand); break;
          case "SUB":
            ExecuteSUB(memory, registers, operand); break;
          case "SKP":
            ExecuteSKP(); break;
          case "RTN":
            ExecuteRTN(memory, registers); break;
        }
        if (registers[ERR] == 0)
        {
          opCode = memory[registers[PC]].opCode;
          DisplayCurrentState(sourceCode, memory, registers);
        }
        else
        {
          opCode = "HLT";
        }
      }
      Console.WriteLine("Execution terminated");
    }

    private static void AssemblerSimulator()
    {
      string[] sourceCode = new string[HI_MEM];
      AssemblerInstruction[] memory = new AssemblerInstruction[HI_MEM];
      bool finished = false;
      char menuOption;
      ResetSourceCode(sourceCode);
      ResetMemory(memory);
      while (!finished)
      {
        DisplayMenu();
        menuOption = GetMenuOption();
        switch (menuOption)
        {
          case 'L':
            LoadFile(sourceCode);
            ResetMemory(memory);
            break;
          case 'D':
            if (sourceCode[0] == EMPTY_STRING)
            {
              Console.WriteLine("Error Code 7");
            }
            else
            {
              DisplaySourceCode(sourceCode);
            }
            break;
          case 'E':
            if (sourceCode[0] == EMPTY_STRING)
            {
              Console.WriteLine("Error Code 8");
            }
            else
            {
              EditSourceCode(sourceCode);
              ResetMemory(memory);
            }
            break;
          case 'A':
            if (sourceCode[0] == EMPTY_STRING)
            {
              Console.WriteLine("Error Code 9");
            }
            else
            {
              Assemble(sourceCode, memory);
            }
            break;
          case 'R':
            if (memory[0].operandValue == 0)
            {
              Console.WriteLine("Error Code 10");
            }
            else if (memory[0].opCode == "ERR")
            {
              Console.WriteLine("Error Code 11");
            }
            else
            {
              Execute(sourceCode, memory);
            }
            break;
          case 'X':
            finished = true;
            break;
          default:
            Console.WriteLine("You did not choose a valid menu option. Try again");
            break;
        }
      }
      Console.WriteLine("You have chosen to exit the program");
      Console.ReadLine();
    }

    static void Main(string[] args)
    {
      AssemblerSimulator();
    }

  }
}
