using System;
using System.Runtime.InteropServices;

namespace Tmds.Babeltrace
{
    static unsafe class Interop
    {
        public const string BabeltraceLibrary = "babeltrace";
        public const string BabeltraceCtfLibrary = "babeltrace-ctf";

        [DllImportAttribute(BabeltraceLibrary, EntryPoint = "bt_context_create")]
        public static extern ContextSafeHandle CreateContext();

        [DllImportAttribute(BabeltraceLibrary, EntryPoint = "bt_context_put")]
        public static extern void ReleaseContext(IntPtr handle);

        [DllImportAttribute(BabeltraceLibrary, EntryPoint = "bt_context_add_trace")]
        public static extern int ContextAddTrace(ContextSafeHandle context, string path,
            string format = "ctf", void* packet_seek = null, void* bt_mmap_stream_list = null, void* metadata = null);

        [DllImportAttribute(BabeltraceLibrary, EntryPoint = "bt_iter_next")]
        public static extern int MoveNext(void* iter);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_field_get_error")]
        public static extern int CtfFieldGetError();

        public enum IteratorPosType
        {
            SeekTime,
            SeekRestore,
            SeekCur,
            SeekBegin,
            SeekLast
        }

        public struct IteratorPos
        {
            public IteratorPosType Type;
            ulong SeekTime; // or bt_saved_pos *restore
        }

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_iter_create")]
        public static extern EventIteratorSafeHandle CreateCtfIterator(ContextSafeHandle context,
            IteratorPos* beginPos = null, IteratorPos* endPos = null);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_iter_destroy")]
        public static extern void DestroyCtfIterator(IntPtr handle);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_iter_read_event")]
        public static extern void* GetCurrentEvent(EventIteratorSafeHandle iterator);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_iter")]
        public static extern void* GetIterator(EventIteratorSafeHandle iterator);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_event_name")]
        public static extern IntPtr GetEventName(void* eventDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_timestamp")]
        public static extern ulong GetEventTimestampNs(void* eventDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_cycles")]
        public static extern ulong GetEventTimestampCycles(void* eventDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_top_level_scope")]
        public static extern void* GetCtfScopeDef(void* eventDef, CtfScope scope);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_field")]
        public static extern void* GetEventField(void* eventDef, void* scopeDef, string fieldName);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_uint64")]
        public static extern ulong GetEventUInt64(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_int64")]
        public static extern long GetEventInt64(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_enum_str")]
        public static extern IntPtr GetEventEnumString(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_string")]
        public static extern IntPtr GetEventString(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_enum_int")]
        public static extern void* GetEventEnumIntField(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_field_name")]
        public static extern IntPtr GetFieldName(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_decl_from_def")]
        public static extern void* GetFieldDeclaration(void* fieldDef);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_index")]
        public static extern void* GetFieldAt(void* eventDef, void* fieldDef, int index);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_field_list")]
        public static extern int GetFields(void* eventDef, void* scopeDef, void*** fieldDefs, int* count);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_field_type")]
        public static extern FieldType GetFieldType(void* fieldDeclaration);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_array_len")]
        public static extern int GetArrayLength(void* fieldDeclaration);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_int_signedness")]
        public static extern int IsIntegerSigned(void* fieldDeclaration);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_int_base")]
        public static extern int GetIntegerBase(void* fieldDeclaration);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_int_len")]
        public static extern IntPtr GetIntegerBitLength(void* fieldDeclaration);

        [DllImportAttribute(BabeltraceCtfLibrary, EntryPoint = "bt_ctf_get_encoding")]
        public static extern StringEncoding GetEncoding(void* fieldDeclaration);
    }
}