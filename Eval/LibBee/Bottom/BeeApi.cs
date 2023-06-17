using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LibBee.Bottom
{
    internal static class Interfaces
    {
        private const string DllPath = "../../../../LibBee/dll/BeeInterfaces.dll";

        // Row
        [DllImport(DllPath, EntryPoint = "new_row_vector")]
        public static extern UIntPtr NewRowVector();

        [DllImport(DllPath, EntryPoint = "add_row_to_vec")]
        public static extern void AddRow(UIntPtr rowVector, UIntPtr values);


        // Column types
        [DllImport(DllPath, EntryPoint = "convert_to_types_vector")]
        public static extern UIntPtr ConvertToTypesVector(UIntPtr typeNameVec);


        // Table
        [DllImport(DllPath, EntryPoint = "new_table_impl")]
        public static extern UIntPtr NewTableImpl(UIntPtr headings, UIntPtr rows, UIntPtr types, UIntPtr depths);

        [DllImport(DllPath, EntryPoint = "new_table_impl_vector")]
        public static extern UIntPtr NewTableImplVector();

        [DllImport(DllPath, EntryPoint = "add_table_impl_to_vec")]
        public static extern void AddTableImplToVec(UIntPtr vec, UIntPtr table);

        [DllImport(DllPath, EntryPoint = "table_impl_to_string")]
        public static extern string TableImplToString(UIntPtr table);

        [DllImport(DllPath, EntryPoint = "new_output_vector")]
        public static extern UIntPtr NewOutputVector();

        [DllImport(DllPath, EntryPoint = "add_output_pair")]
        public static extern void AddOutputPair(UIntPtr vec, string opCode, UIntPtr table);



        [DllImport(DllPath, EntryPoint = "call_wrap")]
        public static extern string CallWrap(string str);

        [DllImport(DllPath, EntryPoint = "new_bee_program")]
        public static extern UIntPtr NewBeeProgram();

        [DllImport(DllPath, EntryPoint = "print_bee_program")]
        public static extern void PrintBeeProgram(UIntPtr program);



        [DllImport(DllPath, EntryPoint = "read_file_example_tables_from_csv")]
        public static extern UIntPtr ReadFileExampleTables(string path);

        [DllImport(DllPath, EntryPoint = "read_file_example_operations")]
        public static extern UIntPtr ReadFileExampleOperations(string path);

        [DllImport(DllPath, EntryPoint = "read_spreadsheet_example_tables_from_csv")]
        public static extern UIntPtr ReadSpreadsheetExampleTables(string path, bool intContent, bool doubleContent, bool intRowHead, bool intColHead);

        [DllImport(DllPath, EntryPoint = "read_spreadsheet_example_operations")]
        public static extern UIntPtr ReadSpreadsheetExampleOperations(string path);

        [DllImport(DllPath, EntryPoint = "tables_to_str")]
        public static extern string TablesToStr(UIntPtr tableVec);

        [DllImport(DllPath, EntryPoint = "actions_to_str")]
        public static extern string ActionsToStr(UIntPtr actionVec);

        [DllImport(DllPath, EntryPoint = "new_synthesizer")]
        public static extern UIntPtr NewSynthesizer(UIntPtr exampleTableVec, UIntPtr outputVec, UIntPtr constStrVec, UIntPtr constIntVec, UIntPtr constDateVec);

        [DllImport(DllPath, EntryPoint = "synthesize_bidirectional")]
        public static extern string SynthesizeBidirectional(UIntPtr synthesizer, int timeOutMilli, int numHypothesized, int numFeatures, bool concat);
        
        [DllImport(DllPath, EntryPoint = "synthesize_forward")]
        public static extern string SynthesizeForward(UIntPtr synthesizer, int timeOutMilli, bool concat);

        [DllImport(DllPath, EntryPoint = "config_concat_expr")]
        public static extern void ConfigConcatExpr(bool status);
        
        [DllImport(DllPath, EntryPoint = "config_linear_expr")]
        public static extern void ConfigLinearExpr(bool status);
        
        [DllImport(DllPath, EntryPoint = "config_sum_expr")]
        public static extern void ConfigSumExpr(bool status);
        
        [DllImport(DllPath, EntryPoint = "config_div_expr")]
        public static extern void ConfigDivExpr(bool status);

        [DllImport(DllPath, EntryPoint = "config_mod_expr")]
        public static extern void ConfigModExpr(bool status);
    }
}
