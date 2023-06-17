using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LibBee.Bottom
{
    internal static class Basic
    {
        private const string DllPath = "../../../../LibBee/dll/BeeInterfaces.dll";

        [DllImport(DllPath, EntryPoint = "new_int")]
        public static extern UIntPtr NewInt(int i);

        [DllImport(DllPath, EntryPoint = "new_double")]
        public static extern UIntPtr NewDouble(double d);

        [DllImport(DllPath, EntryPoint = "new_bool")]
        public static extern UIntPtr NewBool(bool b);

        [DllImport(DllPath, EntryPoint = "new_string")]
        public static extern UIntPtr NewString(string str);

        [DllImport(DllPath, EntryPoint = "new_tm")]
        public static extern UIntPtr NewTM(int year, int month, int day);


        [DllImport(DllPath, EntryPoint = "new_str_vector")]
        public static extern UIntPtr NewStringVector();

        [DllImport(DllPath, EntryPoint = "add_str_to_vec")]
        public static extern void AddStrToVec(UIntPtr vec, string str);

        [DllImport(DllPath, EntryPoint = "delete_str_vector")]
        public static extern void DeleteStrVec(UIntPtr vec);

        public static void FillStrings(UIntPtr outs, IEnumerable<string> ins)
        {
            foreach (var str in ins)
            {
                AddStrToVec(outs, str);
            }
        }

        [DllImport(DllPath, EntryPoint = "new_str_set")]
        public static extern UIntPtr NewStringSet();

        [DllImport(DllPath, EntryPoint = "add_str_to_set")]
        public static extern void AddStrToSet(UIntPtr set, string str);
        

        [DllImport(DllPath, EntryPoint = "new_int_vector")]
        public static extern UIntPtr NewIntVector();

        [DllImport(DllPath, EntryPoint = "add_int_to_vec")]
        public static extern void AddIntToVec(UIntPtr vec, int i);

        [DllImport(DllPath, EntryPoint = "delete_int_vector")]
        public static extern void DeleteIntVec(UIntPtr vec);

        public static void FillInts(UIntPtr outs, IEnumerable<int> ins)
        {
            foreach (var i in ins)
            {
                AddIntToVec(outs, i);
            }
        }



        [DllImport(DllPath, EntryPoint = "new_voidptr_vector")]
        public static extern UIntPtr NewVoiPtrVector();

        [DllImport(DllPath, EntryPoint = "add_voidptr_to_vec")]
        public static extern void AddVoidPtrToVec(UIntPtr vec, UIntPtr i);

        [DllImport(DllPath, EntryPoint = "delete_voidptr_vector")]
        public static extern void DeleteVoidPtrVec(UIntPtr vec);



        [DllImport(DllPath, EntryPoint = "new_tm_vector")]
        public static extern UIntPtr NewTmVector();

        [DllImport(DllPath, EntryPoint = "new_tm_ptr_vector")]
        public static extern UIntPtr NewTmPtrVector();

        [DllImport(DllPath, EntryPoint = "add_tm_ptr_to_vec")]
        public static extern void AddTmPtrToVec(UIntPtr vec, UIntPtr tm_ptr);

        [DllImport(DllPath, EntryPoint = "convert_tm_vector")]
        public static extern UIntPtr ConvertTmVec(UIntPtr vec);
    }
}
