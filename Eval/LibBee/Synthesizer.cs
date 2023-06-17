using LibBee.Model;
using LibBee.Bottom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LibBee
{
    public class Synthesizer
    {
        Dictionary<string, UIntPtr> strTable;
        Dictionary<int, UIntPtr> intTable;
        Dictionary<double, UIntPtr> doubleTable;        
        Dictionary<DateTime, UIntPtr> timeTable;
        Dictionary<string, UIntPtr> refTable;
        private Dictionary<ISet<string>, UIntPtr> refSetTable;
        Dictionary<bool, UIntPtr> boolTable;

        UIntPtr c_input_tables = UIntPtr.Zero;
        UIntPtr c_output_vec = UIntPtr.Zero;

        UIntPtr StrToPtr(string s)
        {
            if (!strTable.ContainsKey(s))
                strTable[s] = Basic.NewString(s);
            return strTable[s];
        }

        UIntPtr IntToPtr(int i)
        {
            if (!intTable.ContainsKey(i))
                intTable[i] = Basic.NewInt(i);
            return intTable[i];
        }

        UIntPtr BoolToPtr(bool b)
        {
            if (!boolTable.ContainsKey(b))
                boolTable[b] = Basic.NewBool(b);
            return boolTable[b];
        }

        UIntPtr DoubleToPtr(double d)
        {
            if (!doubleTable.ContainsKey(d))
                doubleTable[d] = Basic.NewDouble(d);
            return doubleTable[d];
        }

        UIntPtr RefToPtr(string r)
        {
            if (!refTable.ContainsKey(r))
                refTable[r] = Basic.NewString(r);
            return refTable[r];
        }

        UIntPtr RefSetToPtr(ISet<string> rs)
        {
            if (!refSetTable.ContainsKey(rs))
            {
                var setPtr = Basic.NewStringSet();
                foreach (var s in rs)
                    Basic.AddStrToSet(setPtr, s);
                refSetTable[rs] = setPtr;
            }
            return refSetTable[rs];
        }

        UIntPtr TimeToPtr(DateTime t)
        {
            if (!timeTable.ContainsKey(t))
                timeTable[t] = Basic.NewTM(t.Year, t.Month, t.Day);
            return timeTable[t];
        }

        public IEnumerable<BeeTable> InputExamples
        {
            get;
            private set;
        }

        public IEnumerable<ActionCommand> OutputExamples
        {
            get;
            private set;
        }

        public Synthesizer(IEnumerable<BeeTable> inputExamples, IEnumerable<ActionCommand> outputExamples)
        {
            InputExamples = inputExamples;
            OutputExamples = outputExamples;

            strTable = new();
            intTable = new();
            doubleTable = new();
            boolTable = new();
            refTable = new();
            timeTable = new();
            refSetTable = new();
        }

        public Synthesizer(UIntPtr c_input_tables, UIntPtr c_output_vec)
        {
            this.c_input_tables = c_input_tables;
            this.c_output_vec = c_output_vec;
        }
        
        UIntPtr ConvertInputExamples()
        {
            var result = Interfaces.NewTableImplVector();
            foreach (var table in InputExamples)
            {
                var headings = Basic.NewStringVector();
                var typeNames = Basic.NewStringVector();
                var depths = Basic.NewIntVector();
                foreach (var col in table.Columns)
                {
                    Basic.AddStrToVec(headings, col.Name);
                    Basic.AddStrToVec(typeNames, col.GetTypeNameInC());
                    Basic.AddIntToVec(depths, 0);
                }
                var types = Interfaces.ConvertToTypesVector(typeNames);

                var rows = Interfaces.NewRowVector();
                foreach (var row in table.Rows)
                {
                    var values = Basic.NewVoiPtrVector();
                    for (int i = 0; i < table.Columns.Count; ++i)
                    {
                        var type = table.Columns[i].ColumnType;
                        if (type == typeof(int))
                            Basic.AddVoidPtrToVec(values, IntToPtr((int)row[i]));
                        else if (type == typeof(bool))
                            Basic.AddVoidPtrToVec(values, BoolToPtr((bool)row[i]));
                        else if (type == typeof(double))
                            Basic.AddVoidPtrToVec(values, DoubleToPtr((double)row[i]));
                        else if (type == typeof(string))
                            Basic.AddVoidPtrToVec(values, StrToPtr((string)row[i]));
                        else if (type == typeof(DateTime))
                            Basic.AddVoidPtrToVec(values, TimeToPtr((DateTime)row[i]));
                        else
                            Basic.AddVoidPtrToVec(values, RefToPtr(row[i].ToString()));
                    }
                    
                    Interfaces.AddRow(rows, values);
                }

                var c_table = Interfaces.NewTableImpl(headings, rows, types, depths);
                Console.WriteLine(Interfaces.TableImplToString(c_table));
                Interfaces.AddTableImplToVec(result, c_table);
            }

            return result;
        }

        UIntPtr ConvertOutputExamples()
        {
            var outputVec = Interfaces.NewOutputVector();
            Dictionary<MethodBase, List<ActionCommand>> dic = new ();
            foreach (var operationCommand in OutputExamples)
            {
                if (!dic.ContainsKey(operationCommand.Method))
                {
                    dic[operationCommand.Method] = new List<ActionCommand>();
                }
                dic[operationCommand.Method].Add(operationCommand);
            }

            foreach (var (method, commands) in dic)
            {
                var opCode = method.Name;

                var headings = Basic.NewStringVector();
                var typeNames = Basic.NewStringVector();
                var depths = Basic.NewIntVector();

                var columns = commands.First().GetParameters();
                foreach (var (name, type) in columns)
                {
                    Basic.AddStrToVec(headings, name);
                    Basic.AddStrToVec(typeNames, Utils.ConvertTypeName(type));
                    Basic.AddIntToVec(depths, 0);
                }
                var types = Interfaces.ConvertToTypesVector(typeNames);

                var rows = Interfaces.NewRowVector();
                foreach (var command in commands)
                {
                    var values = Basic.NewVoiPtrVector();
                    for (int i = 0; i < columns.Count; ++i)
                    {
                        var type = columns[i].Value;
                        object val;
                        if (i == 0 && !command.IsStatic)
                            val = command.Instance;
                        else if (!command.IsStatic)
                            val = command.Args[i - 1];
                        else
                            val = command.Args[i];

                        if (type == typeof(int))
                            Basic.AddVoidPtrToVec(values, IntToPtr((int)val));
                        else if (type == typeof(bool))
                            Basic.AddVoidPtrToVec(values, BoolToPtr((bool)val));
                        else if (type == typeof(double))
                            Basic.AddVoidPtrToVec(values, DoubleToPtr((double)val));
                        else if (type == typeof(string))
                            Basic.AddVoidPtrToVec(values, StrToPtr((string)val));
                        else if (type.IsArray)
                        {
                            var refSet = new HashSet<string>();
                            foreach (var refer in (Array) val)
                            {
                                refSet.Add(refer.ToString());
                            }
                            Basic.AddVoidPtrToVec(values, RefSetToPtr(refSet));
                        } 
                        else
                            Basic.AddVoidPtrToVec(values, RefToPtr(val.ToString()));
                    }

                    Interfaces.AddRow(rows, values);
                }

                var c_table = Interfaces.NewTableImpl(headings, rows, types, depths);
                Console.WriteLine(Interfaces.TableImplToString(c_table));
                Interfaces.AddOutputPair(outputVec, opCode, c_table);
            }

            return outputVec;
        }


        public IEnumerable<Program> Synthesize(int timeoutMilli, string[] userStrs = null, DateTime[] userTimes = null, int[] userInts = null, bool concat = true, bool bidirectional=true)
        {
            if (c_input_tables == UIntPtr.Zero)
                c_input_tables = ConvertInputExamples();
            if (c_output_vec == UIntPtr.Zero)
                c_output_vec = ConvertOutputExamples();

            var const_strs = Basic.NewStringVector();
            if (userStrs != null)
            {
                foreach (var str in userStrs)
                    Basic.AddStrToVec(const_strs, str);
            }

            var const_ints = Basic.NewIntVector();
            if (userInts != null)
            {
                foreach (var num in userInts)
                    Basic.AddIntToVec(const_ints, num);
            }

            var const_tm_ptrs = Basic.NewTmPtrVector();
            if (userTimes != null)
            {
                foreach (var time in userTimes)
                    Basic.AddTmPtrToVec(const_tm_ptrs, Basic.NewTM(time.Year, time.Month, time.Day));
            }
            var const_tms = Basic.ConvertTmVec(const_tm_ptrs);
            
            Interfaces.ConfigConcatExpr(concat);
            var c_synthesizer = Interfaces.NewSynthesizer(c_input_tables, c_output_vec, const_strs, const_ints, const_tms);
            var program = bidirectional
                ? Interfaces.SynthesizeBidirectional(c_synthesizer, timeoutMilli, 20, 5, concat)
                : Interfaces.SynthesizeForward(c_synthesizer, timeoutMilli, concat);

            Console.WriteLine("success: " + (program != ""));
            Console.WriteLine("program: \n" + program);

            if (program != "")
                return new List<Program> { new Program(program) };
            else
                return new List<Program> { };
        }
    }
}
